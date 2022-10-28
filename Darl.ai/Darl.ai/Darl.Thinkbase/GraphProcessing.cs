using Darl.Common;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.GraphML;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlLanguage.Processing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Darl.Thinkbase
{
    /// <summary>
    /// Perform the high level processing required to create, maintain and infer from knowledge graphs
    /// </summary>
    public class GraphProcessing : IGraphProcessing
    {

        private readonly IGraphPrimitives _primitives;

        private readonly ILogger _logger;

        private readonly IMetaStructureHandler _metaHandler;

        private readonly int maxKnowledgeStateUpdates = 50;

        private readonly ISubject<KnowledgeState> _knowledgeStateStream = new ReplaySubject<KnowledgeState>(1);

        private readonly IDataLoader _dataLoader;

        public static int maxDepth = 0;


        public Dictionary<string, LineageDefinitionNode> PreloadLineages { get => _metaHandler.PreloadLineages; }

        public GraphProcessing(IGraphPrimitives primitives, ILogger<GraphProcessing> logger, IMetaStructureHandler metaHandler, IDataLoader loader)
        {
            _primitives = primitives;
            _logger = logger;
            _metaHandler = metaHandler;
            _dataLoader = loader;
        }
        public async Task<GraphConnection> CreateGraphConnection(string compositeName, GraphConnectionInput graphConnection, OntologyAction ontology = OntologyAction.ignore)
        {
            var model = await _primitives.Load(compositeName);
            if (!LineageLibrary.CheckLineage(graphConnection.lineage))
                throw new RuleException($"Malformed lineage: {graphConnection.lineage}.");
            if (!_metaHandler.IsConnectionLineage(graphConnection.lineage))
                throw new RuleException($"Connections should have lineages of type 'verb'. This has a lineage of {graphConnection.lineage}.");
            if (ontology != OntologyAction.ignore)//ontological compliance checks
            {
                var startLineage = await GetGraphObjectProperty(compositeName, graphConnection.startId, "lineage");
                var endLineage = await GetGraphObjectProperty(compositeName, graphConnection.endId, "lineage");
                if (ontology == OntologyAction.check)
                {
                    if (!OntologicalCompliance(model, graphConnection.lineage, startLineage, endLineage))
                    {
                        throw new RuleException($"No association exists between {startLineage}, the verb {graphConnection.lineage} and {endLineage}\n if you are sure this is correct use the definitive flag in the call.");
                    }
                }
                else
                {
                    BuildOntology(model, graphConnection.lineage, startLineage, endLineage);
                }
                foreach (var p in graphConnection.properties)
                {
                    if (!LineageLibrary.CheckLineage(p.lineage))
                        throw new RuleException($"Malformed property lineage: {p.lineage}.");
                    if (ontology == OntologyAction.check)
                    {
                        if (!OntologicalCompliance(model, graphConnection.lineage, p.lineage))
                        {
                            throw new RuleException($"No association exists between {graphConnection.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                    else
                    {
                        BuildOntology(model, graphConnection.lineage, p.lineage);
                    }
                }
            }
            return  CreateConnection(model, graphConnection);

        }

        public async Task<GraphObject> CreateGraphObject(string compositeName, GraphObjectInput graphObject, OntologyAction ontology = OntologyAction.ignore)
        {
            var model = await _primitives.Load(compositeName);
            graphObject.lineage = CombineLineages(graphObject.lineage, graphObject.subLineage);
            if (!LineageLibrary.CheckLineage(graphObject.lineage))
                throw new RuleException($"Malformed lineage: {graphObject.lineage}.");
            if (!_metaHandler.IsObjectLineage(graphObject.lineage))
                throw new RuleException($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
            if (ontology != OntologyAction.ignore)//ontological compliance checks
            {
                if (!LineageExists(model, graphObject.lineage))
                {
                    AddObjectToOntology(model, graphObject.lineage);
                }
                if (graphObject.properties != null)
                {
                    foreach (var p in graphObject.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.lineage))
                            throw new RuleException($"Malformed property lineage: {p.lineage}.");
                        if (ontology == OntologyAction.check)
                        {
                            if (!OntologicalCompliance(model, graphObject.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphObject.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            BuildOntology(model, graphObject.lineage, p.lineage);
                        }
                    }
                }
            }
            return CreateObject(model, graphObject);

        }

        public async Task<bool> CreateNewGraph(string userId, string name)
        {
            var graphname = CreateCompositeName(userId, name);
            return await _primitives.CreateModel(graphname);
        }

        public async Task<bool> DeleteGraph(string userId, string name)
        {
            var graphname = CreateCompositeName(userId, name);
            return await _primitives.DeleteModel(graphname);
        }

        public async Task<List<string>> GetGraphs(string userId)
        {
            return await _primitives.ListModels(userId);
        }

        public async Task<IGraphModel?> GetModel(string userId, string name)
        {
            var graphname = CreateCompositeName(userId, name);
            var model = await _primitives.Load(graphname);
            if(model != null)
                model.modelName = graphname;
            return model;
        }


        internal string CreateCompositeName(string userId, string name)
        {
            return _primitives.CreateCompositeName(userId, name);
        }

        public async Task<GraphConnection?> DeleteGraphConnection(string compositeName, string id)
        {
            if (await _primitives.Load(compositeName) is IGraphModel cont)
            {
                if (!cont.edges.ContainsKey(id))
                {
                    return null;
                }
                var conn = cont.edges[id];
                if (cont.vertices.ContainsKey(conn.startId))
                {
                    cont.vertices[conn.startId].Out.Remove(conn);
                }
                if (cont.vertices.ContainsKey(conn.endId))
                {
                    cont.vertices[conn.endId].In.Remove(conn);
                }
                cont.edges.Remove(id);
                return conn;
            }
            return null;
        }

        public async Task<GraphObject?> DeleteGraphObject(string compositeName, string id)
        {
            if (await _primitives.Load(compositeName) is IGraphModel cont)
            {
                if (!cont.vertices.ContainsKey(id))
                {
                    return null;
                }
                var node = cont.vertices[id];
                //delete associated connections
                foreach (var c in node.Out)
                {
                    if (cont.vertices.ContainsKey(c.endId))
                    {
                        var end = cont.vertices[c.endId];
                        var endLink = end.In.Where(a => a.id == c.id).FirstOrDefault();
                        if (endLink != null)
                        {
                            var success = end.In.Remove(endLink);
                        }
                    }
                    cont.edges.Remove(c.id ?? String.Empty);
                }
                foreach (var c in node.In)
                {
                    if (cont.vertices.ContainsKey(c.startId))
                    {
                        var start = cont.vertices[c.startId];
                        var startLink = start.In.Where(a => a.id == c.id).FirstOrDefault();
                        if (startLink != null)
                        {
                            var success = start.Out.Remove(startLink);
                        }
                    }
                    cont.edges.Remove(c.id ?? String.Empty);
                }
                cont.vertices.Remove(id);
                return node;
            }
            return null;
        }

        public async Task<GraphObject?> GetGraphObjectById(string compositeName, string? id)
        {
            if (id == null)
                throw new RuleException($"GetGraphObjectById: Id cannot be null.");
            //if id doesn't match but externalId does, return that
            if (await _primitives.Load(compositeName) is IGraphModel cont)
            {
                if (cont.vertices.ContainsKey(id))
                    return cont.vertices[id];
                return cont.vertices.Values.Where(a => a.externalId == id).FirstOrDefault();
            }
            return null;
        }

        public async Task<List<GraphObject>?> GetGraphObjects(string compositeName, string name, string lineage)
        {
            if (await _primitives.Load(compositeName) is IGraphModel cont)
            {
                return cont.vertices.Values.Where(a => a.name == name && (a.lineage ?? String.Empty).StartsWith(lineage)).ToList();
            }
            return null;
        }

        public async Task<string> GetGraphObjectProperty(string compositeName, string id, string property)
        {
            var o = await GetGraphObjectById(compositeName, id);
            if (o == null)
                return string.Empty;
            return GetAttibuteGivenObject(o, property); return await GetGraphObjectProperty(compositeName, id, property);
        }

        public async Task<GraphObject?> GetGraphObjectByExternalId(string compositeName, string externalId)
        {
            if (await _primitives.Load(compositeName) is IGraphModel cont)
                return cont.vertices.Values.Where(a => a.externalId == externalId).FirstOrDefault();
            return null;
        }

        public async Task<GraphConnection?> UpdateGraphConnection(string compositeName, GraphConnectionUpdate graphConnection, OntologyAction ontology = OntologyAction.ignore)
        {
            var model = await _primitives.Load(compositeName);
            if (ontology != OntologyAction.ignore)//ontological compliance checks
            {
                if (graphConnection.properties != null)
                {
                    foreach (var p in graphConnection.properties)
                    {
                        if (ontology == OntologyAction.check)
                        {
                            if (!OntologicalCompliance(model, graphConnection.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphConnection.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            BuildOntology(model, graphConnection.lineage, p.lineage);
                        }
                    }
                }
            }
            return await UpdateConnection(compositeName, graphConnection);

        }

        public async Task<GraphObject> UpdateGraphObject(string compositeName, GraphObjectUpdate graphObject, OntologyAction ontology = OntologyAction.ignore)
        {
            var model = await _primitives.Load(compositeName);
            graphObject.lineage = CombineLineages(graphObject.lineage, graphObject.subLineage);
            if (!LineageLibrary.CheckLineage(graphObject.lineage))
                throw new RuleException($"Malformed lineage: {graphObject.lineage}.");
            if (!_metaHandler.IsObjectLineage(graphObject.lineage))
                throw new RuleException($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
            if (ontology != OntologyAction.ignore)//ontological compliance checks
            {
                if (!LineageLibrary.CheckLineage(graphObject.lineage))
                    throw new RuleException($"Malformed property lineage: {graphObject.lineage}.");
                if (!LineageExists(model, graphObject.lineage))
                {
                    AddObjectToOntology(model, graphObject.lineage);
                }
                if (graphObject.properties != null)
                {
                    foreach (var p in graphObject.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.lineage))
                            throw new RuleException($"Malformed property lineage: {p.lineage}.");
                        if (ontology == OntologyAction.check)
                        {
                            if (!OntologicalCompliance(model, graphObject.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphObject.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            BuildOntology(model, graphObject.lineage, p.lineage);
                        }
                    }
                }
            }
            return await UpdateObject(compositeName, graphObject);
        }

        /// <summary>
        /// Get a connection based on the node ids and the lineage
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="lineage"></param>
        /// <returns>The partially filled in connection</returns>
        public async Task<GraphConnection?> GetConnectionByIds(string compositeName, string startId, string endId, string lineage)
        {
            if (await _primitives.Load(compositeName) is IGraphModel cont)
            {
                var start = cont.vertices[startId];
                return start.Out.Where(a => a.lineage == lineage && a.endId == endId).FirstOrDefault();
            }
            return null;
        }

        public async Task<GraphConnection?> GetConnectionById(string compositeName, string id)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (!cont.edges.ContainsKey(id))
                return null;
            return cont.edges[id];
        }

        public async Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID)
        {
            try
            {
                var cont = await _primitives.Load(compositeName);
                var start = await GetGraphObjectByExternalId(compositeName, startExternalID);
                var target = await GetGraphObjectByExternalId(compositeName, endExternalID);
                return ShortestPath(cont, start, target);
            }
            catch (Exception)
            {

            }
            return null;
        }

        public async Task<string> ProcessAttribute(string compositeName, string externalID, string propertyName)
        {
            var obj = await GetGraphObjectByExternalId(compositeName, externalID);
            return GetAttibuteGivenObject(obj, propertyName);
        }

        /// <summary>
        /// get categories from objects of the lineage given linked to the root object.
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="rootName"></param>
        /// <param name="childLineage"></param>
        /// <param name="childValueAttribute"></param>
        /// <returns></returns>
        public async Task<List<StringStringPair>> ProcessCategories(string compositeName, string rootExternalID, string childLineage, string childValueAttribute)
        {
                var list = new List<StringStringPair>();
                if (string.IsNullOrEmpty(rootExternalID))
                {
                    var objects = await GetGraphObjectsByLineage(compositeName, childLineage);
                    foreach (var o in objects)
                    {
                        var externalId = GetAttibuteGivenObject(o, nameof(GraphObject.externalId));
                        var value = GetAttibuteGivenObject(o, childValueAttribute);
                        list.Add(new StringStringPair(externalId, value));
                    }
                }
                else
                {
                    var obj = await GetGraphObjectByExternalId(compositeName, rootExternalID);
                    if (obj != null)
                    {
                        foreach (var c in obj.Out)
                        {
                            var other = await GetGraphObjectById(compositeName, c.endId);
                            if (other != null)
                            {
                                if ((other.lineage ?? String.Empty).StartsWith(childLineage))
                                {
                                    var externalId = GetAttibuteGivenObject(other, nameof(GraphObject.externalId));
                                    var value = GetAttibuteGivenObject(other, childValueAttribute);
                                    list.Add(new StringStringPair(externalId, value));
                                }
                            }
                        }
                        foreach (var c in obj.In)
                        {
                            var other = await GetGraphObjectById(compositeName, c.startId);
                            if (other != null)
                            {
                                if ((other.lineage ?? String.Empty).StartsWith(childLineage))
                                {
                                    var externalId = GetAttibuteGivenObject(other, nameof(GraphObject.externalId));
                                    var value = GetAttibuteGivenObject(other, childValueAttribute);
                                    list.Add(new StringStringPair(externalId, value));
                                }
                            }
                        }
                    }
                }
                return list;
            
        }

        /// <summary>
        /// get categories from objects of the lineage given.
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="rootName"></param>
        /// <param name="childLineage"></param>
        /// <param name="childValueAttribute"></param>
        /// <returns></returns>
        public async Task<List<StringStringPair>> ProcessCategories(string compositeName, string childLineage, string childValueAttribute)
        {
            var list = new List<StringStringPair>();
            var cont = await _primitives.Load(compositeName);
            foreach (var v in cont.vertices.Values.Where(a => (a.lineage ?? String.Empty).StartsWith(childLineage)))
            {
                var externalId = GetAttibuteGivenObject(v, nameof(GraphObject.externalId));
                var value = GetAttibuteGivenObject(v, childValueAttribute);
                list.Add(new StringStringPair(externalId, value));
            }
            return list;
        }
        public async Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage)
        {
            var cont = await _primitives.Load(compositeName);
            return cont.vertices.Values.Where(a => a.lineage.StartsWith(lineage)).ToList();
        }

        public async Task Store(string compositeName)
        {
            await _primitives.Store(compositeName);
        }



        public async Task CreateVirtualAttribute(string compositeName, string lineage, GraphAttributeInput att)
        {
            var cont = await _primitives.Load(compositeName) as IGraphModel;
            if (cont.virtualVertices.ContainsKey(lineage))
            {
                var node = cont.virtualVertices[lineage];
                if (node.properties.Where(a => a.lineage == att.lineage).Any())
                {
                    node.properties.Remove(node.properties.Where(a => a.lineage == att.lineage).First());
                }
                node.properties.Add(new GraphAttribute { id = Guid.NewGuid().ToString(), existence = att.existence, confidence = att.confidence ?? 1.0, inferred = false, lineage = att.lineage, name = att.name, type = att.type, value = att.value, _virtual = true });
            }
        }



        public async Task LoadGraphML(string compositeName, Stream graphML, List<StringStringPair> attributes)
        {
            var atts = new Dictionary<string, string>();
            if (attributes != null)
            {
                foreach (var ssp in attributes)
                {
                    if (!atts.ContainsKey(ssp.Name))
                        atts.Add(ssp.Name, ssp.Value);
                    else
                        atts[ssp.Name] = ssp.Value;
                }
            }

            XmlSerializer xSerializer = new XmlSerializer(typeof(graphmltype));

            graphmltype? graphRoot = (graphmltype?)xSerializer.Deserialize(graphML);
            bool virtualPresent = false;
            bool realPresent = false;
            var model = await _primitives.Load(compositeName);
            if (graphRoot != null && graphRoot.Items.Length > 1)
            {
                var virtualGraph = graphRoot.Items.Where(a => ((graphtype)a).desc == "virtual").FirstOrDefault() as graphtype;
                if (virtualGraph != null)
                {
                    virtualPresent = true;
                    foreach (var e in virtualGraph.Items)
                    {
                        if (e is nodetype)
                        {
                            var n = e as nodetype;
                            var lineageObj = n.Items.Where(a => (a as datatype).key == "lineage").FirstOrDefault() as datatype;
                            var lineage = "";
                            if (lineageObj != null)
                                lineage = lineageObj.Text[0];
                            var nameObj = n.Items.Where(a => (a as datatype).key == "name").FirstOrDefault() as datatype;
                            var name = "";
                            if (nameObj != null)
                                name = nameObj.Text[0];
                            var descObj = n.Items.Where(a => (a as datatype).key == "description").FirstOrDefault() as datatype;
                            var desc = "";
                            if (descObj != null)
                                desc = descObj.Text[0];
                            CreateVirtualObject(model, lineage, name, desc);
                        }
                        else if (e is edgetype)
                        {
                            var c = e as edgetype;
                            var childObj = c.data.Where(a => a.key == "startId").FirstOrDefault();
                            var child = "";
                            if (childObj != null)
                                child = childObj.Text[0];
                            var nameObj = c.data.Where(a => a.key == "name").FirstOrDefault();
                            var name = "";
                            if (nameObj != null)
                                name = nameObj.Text[0];
                            var lineageObj = c.data.Where(a => a.key == "endId").FirstOrDefault();
                            var lineage = "";
                            if (lineageObj != null)
                                lineage = lineageObj.Text[0];
                            CreateVirtualConnection(model, child, lineage, name);
                        }
                    }
                }
                var realGraph = graphRoot.Items.Where(a => ((graphtype)a).desc == "real").FirstOrDefault() as graphtype;
                if (realGraph != null)
                {
                    realPresent = true;
                    foreach (var e in realGraph.Items)
                    {
                        if (e is nodetype)
                        {
                            CreateRawObject(model, ConvertNode(e as nodetype, atts));
                        }
                        else if (e is edgetype)
                        {
                            CreateRawConnection(model, ConvertConnection(e as edgetype, atts));
                        }
                    }
                }
                if (realPresent && virtualPresent)
                    return;
            }
            //If we get here we have a conventional graph, not one derived from DARL.
            var graph = graphRoot.Items[0] as graphtype;
            foreach (var e in graph.Items)
            {
                if (e is nodetype)
                {
                    var node = e as nodetype;
                    var obj = ConvertNode(node, atts);
                    CreateObject(model, new GraphObjectInput { externalId = node.id, existence = obj.existence, lineage = obj.lineage, name = obj.name, properties = obj.properties });
                }

            }
            foreach (var e in graph.Items)
            {
                if (e is edgetype)
                {
                    var edge = e as edgetype;
                    var obj = ConvertConnection(edge, atts);
                    CreateConnection(model, new GraphConnectionInput { existence = obj.existence, lineage = obj.lineage, name = obj.name, properties = obj.properties, endId = obj.endId, startId = obj.startId, weight = obj.weight });
                }

            }
        }

        public async Task<Stream> StoreGraphML(string compositeName)
        {
            var graphRoot = new graphmltype();
            var realGraph = new graphtype { desc = "real" };
            var virtualGraph = new graphtype { desc = "virtual" };
            graphRoot.Items = new graphtype[] { virtualGraph, realGraph, };
            //load object with data
            var model = await _primitives.Load(compositeName);
            var atts = new Dictionary<string, keytype>();
            var realNodes = new List<nodetype>();
            var virtualNodes = new List<nodetype>();
            var realConns = new List<edgetype>();
            var virtualConns = new List<edgetype>();

            foreach (var node in model.vertices.Values.ToList())
            {
                realNodes.Add(ConvertNode(node, atts));
            }

            foreach (var node in model.virtualVertices.Values.ToList())
            {
                virtualNodes.Add(ConvertNode(node, atts));
            }

            foreach (var conn in model.edges.Values.ToList())
            {
                realConns.Add(ConvertConnection(conn, atts));
            }

            foreach (var conn in model.virtualEdges.Values.ToList())
            {
                virtualConns.Add(ConvertConnection(conn, atts));
            }

            var virtualObjects = new List<object>(virtualNodes);
            virtualObjects.AddRange(virtualConns);
            virtualGraph.Items = virtualObjects.ToArray();
            var realObjects = new List<object>(realNodes);
            realObjects.AddRange(realConns);
            realGraph.Items = realObjects.ToArray();
            //now add attributes/keys
            graphRoot.key = atts.Values.ToArray();
            XmlSerializer xSerializer = new XmlSerializer(typeof(graphmltype));
            MemoryStream ms = new MemoryStream();
            xSerializer.Serialize(ms, graphRoot);
            ms.Position = 0;
            return ms;
        }


        public async Task<List<MatchedElement>> Match(IGraphModel model, string subjectId, List<string> tokens)
        {
            bool fuzzy = false;
            GraphObject root = GetRecognitionRoot(model, subjectId);
            var matches = new List<MatchedGraphAttribute>();
            var defaultMatches = new List<DefaultMatchGraphAttribute>();
            var values = new List<DarlVar>();
            string path = string.Empty;
            root.Match(model, tokens, values, matches, defaultMatches, path, 0, fuzzy);
            matches.Sort();
            //choose the deepest first default match
            MatchedGraphAttribute lan = null;
            int lanDepth = 0;
            foreach (var match in defaultMatches)
            {
                if (lan == null)
                {
                    lan = new MatchedGraphAttribute { terminus = match.Att, path = match.path };
                    lanDepth = match.Depth;
                }
                else if (match.Depth > lanDepth)
                {
                    lan = new MatchedGraphAttribute { terminus = match.Att, path = match.path };
                    lanDepth = match.Depth;
                }
            }
            var compMatches = new List<MatchedElement>();
            compMatches.Add(lan);
            compMatches.AddRange(matches);
            _logger.LogInformation($"Match in tree {subjectId}: found {compMatches.Count} matches in {model.modelName}");
            return compMatches;
        }

        public async Task<GraphObject> CreateRecognitionRoot(string compositeName, string rootLineage)
        {
            var cont = await _primitives.Load(compositeName) as IGraphModel;
            var rootObject = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = rootLineage, name = "root" };
            if (cont.recognitionRoots.ContainsKey(rootLineage))
            {
                throw new Exception($"Recognition root '{rootLineage}' is already specified");
            }
            cont.recognitionVertices.Add(rootObject.id, rootObject);
            cont.recognitionRoots.Add(rootLineage, rootObject);
            return rootObject;
        }

        public async Task<GraphConnection> CreateRecognitionConnection(string compositeName, GraphConnectionInput graphConnection)
        {
            //should test for DAG if new connection added
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph {compositeName} does not exist.");
            if (cont.recognitionVertices[graphConnection.startId].lineage == GraphObject.terminatingLabel)
            {
                return null;
            }
            var conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = graphConnection.endId, startId = graphConnection.startId, weight = 1.0 };
            if (!cont.recognitionVertices.ContainsKey(conn.startId))
                throw new Exception($"GraphConnection startId '{conn.startId}' does not exist.");
            if (!cont.recognitionVertices.ContainsKey(conn.endId))
                throw new Exception($"GraphConnection endId '{conn.endId}' does not exist.");
            cont.recognitionVertices[conn.startId].Out.Add(conn);
            cont.recognitionVertices[conn.endId].In.Add(conn);
            cont.recognitionEdges.Add(conn.id, conn);
            return conn;
        }

        public async Task<GraphObject> CreateRecognitionObject(string compositeName, GraphObjectInput graphObject)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var obj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = graphObject.lineage.ToLower(), name = graphObject.name, properties = ConvertAttributeInputList(graphObject.properties) };
            cont.recognitionVertices.Add(obj.id, obj);
            return obj;
        }

        public async Task<GraphObject> DeleteRecognitionObject(string compositeName, string id)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont || !cont.recognitionVertices.ContainsKey(id))
                throw new Exception($"GraphConnection id '{id}' does not exist.");
            var obj = cont.recognitionVertices[id];
            foreach (var c in obj.In)
            {
                cont.recognitionVertices[c.startId].Out.Remove(c);
                cont.recognitionEdges.Remove(c.id);
            }
            foreach (var c in obj.Out)
            {
                cont.recognitionVertices[c.endId].In.Remove(c);
                cont.recognitionEdges.Remove(c.id);
            }
            cont.recognitionVertices.Remove(id);
            DeleteRecognitionOrphans(cont);
            return obj;
        }

        public async Task<GraphObject> DeleteRecognitionRoot(string compositeName, string rootLineage)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (cont.recognitionRoots.ContainsKey(rootLineage))
                throw new Exception($"Recognition root '{rootLineage}' does not exist");
            var obj = cont.recognitionRoots[rootLineage];
            cont.recognitionRoots.Remove(rootLineage);
            DeleteRecognitionOrphans(cont);
            return obj;
        }

        public async Task<GraphObject> UpdateRecognitionObject(string compositeName, GraphObjectUpdate go)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (!cont.recognitionVertices.ContainsKey(go.id))
                throw new Exception($"GraphConnection id '{go.id}' does not exist.");
            var obj = cont.recognitionVertices[go.id];
            //update nun-null elements in go
            if (go.existence != null)
            {
                obj.existence = go.existence;
            }
            if (go.lineage != null) //allow change of lineage?
            {
                if (obj.lineage != go.lineage)
                {
                    obj.lineage = go.lineage;
                }
            }
            if (go.name != null)
            {
                if (obj.name != go.name)
                {
                    obj.name = go.name;
                }
            }
            if (go.properties != null && go.properties.Any())
            {
                //merge properties
                foreach (var a in go.properties)
                {
                    var found = obj.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                    if (found != null)
                    {
                        obj.properties.Remove(found);
                    }
                    obj.properties.Add(ConvertAttributeInput(a));
                }
            }
            return obj;
        }

        public async Task<GraphObject> UpdateVirtualObject(string compositeName, GraphObjectUpdate go, bool merge = false)
        {
                if (await _primitives.Load(compositeName) is not IGraphModel cont)
                    throw new Exception($"Graph  '{compositeName}' does not exist.");
                if (!cont.virtualVertices.ContainsKey(go.lineage))
                    return null;
                var node = cont.virtualVertices[go.lineage];
                //update nun-null elements in go
                if (go.name != null)
                {
                    if (node.name != go.name)
                    {
                        node.name = go.name;
                    }
                }
                if (go.properties != null && go.properties.Any())
                {
                    if (merge)
                    {
                        //merge properties
                        foreach (var a in go.properties)
                        {
                            var found = node.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                            if (found != null)
                            {
                                node.properties.Remove(found);
                            }
                            node.properties.Add(ConvertAttributeInput(a));
                        }
                    }
                    else
                    {
                        node.properties = ConvertAttributeInputList(go.properties);
                    }
                }
                return node;
            

        }

        public async Task<List<GraphObject>> NavigateRecognition(string compositeName, string root, string path)
        {
            var list = new List<GraphObject>();
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (!cont.recognitionRoots.ContainsKey(root))
                return list;
            var tokens = path.Split('/').ToList();
            return cont.recognitionRoots[root].Navigate(cont, tokens);
        }

        public async Task<GraphObject> FindRecognition(string compositeName, string root, string path)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (!cont.recognitionRoots.ContainsKey(root))
                return null;
            var tokens = path.Split('/').ToList();
            return cont.recognitionRoots[root].Find(cont, tokens);
        }

        /// <summary>
        /// return objects in the format expected by cytoscape.
        /// </summary>
        /// <param name="compositeName">finds the model</param>
        /// <param name="lineageFilter">empty for all or matching lineages</param>
        /// <returns>Correctly formatted display objects</returns>
        public async Task<DisplayModel> GetRealDisplayGraph(string compositeName, string lineageFilter)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            if (string.IsNullOrEmpty(lineageFilter)) //return everything
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Select(i => new DisplayObject { id = i.id, name = i.name, lineage = ExtractLineage(i.lineage), subLineage = ExtractSubLineage(i.lineage), externalId = i.externalId }));
                dmodel.edges.AddRange(cont.edges.Values.Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            else
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).Select((i => new DisplayObject { id = i.id, name = i.name, lineage = i.lineage, externalId = i.externalId })));
                dmodel.edges.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.In).Intersect(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.Out)).Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            return dmodel;
        }

        public async Task<VRDisplayModel> GetRealVRDisplayGraph(string userId, string graphName, string lineageFilter, string? subjectId)
        {
            var compositeName = userId + "_" + graphName;
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            KnowledgeState ks = null;
            if (!string.IsNullOrEmpty(subjectId))
            {
                ks = await GetKnowledgeState(userId, subjectId, graphName, false);
            }
            var dmodel = new VRDisplayModel { nodes = new List<VRDisplayNode>(), links = new List<VRDisplayLink>() };
            if (string.IsNullOrEmpty(lineageFilter)) //return everything
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Select(i => new VRDisplayNode { id = i.id, name = i.name, lineage = ExtractLineage(i.lineage), subLineage = ExtractSubLineage(i.lineage), externalId = i.externalId, attributes = i.properties != null && ks != null ? i.properties.Select(a => new VRDisplayAtt { name = a.name ?? "", confidence = a.confidence, lineage = a.lineage ?? "", value = ks != null ? ks.GetAttribute(i.id ?? "", a.lineage ?? "")?.value ?? a.value : a.value }).ToList() : new List<VRDisplayAtt>() }));
                dmodel.links.AddRange(cont.edges.Values.Select(i => new VRDisplayLink { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            else
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).Select((i => new VRDisplayNode { id = i.id, name = i.name, lineage = i.lineage, externalId = i.externalId, attributes = i.properties != null ? i.properties.Select(a => new VRDisplayAtt { name = a.name ?? "", confidence = a.confidence, lineage = a.lineage ?? "", value = ks != null ? ks.GetAttribute(i.id ?? "", a.lineage ?? "")?.value ?? a.value : a.value }).ToList() : new List<VRDisplayAtt>() })));
                dmodel.links.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.In).Intersect(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.Out)).Select(i => new VRDisplayLink { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            return dmodel;
        }

        public async Task<DisplayModel> GetVirtualDisplayGraph(string compositeName)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            dmodel.nodes.AddRange(cont.virtualVertices.Values.Select(i => new DisplayObject { id = i.lineage.Replace(',', '-').Replace(':', '-'), name = i.name, lineage = i.lineage }));
            dmodel.edges.AddRange(cont.virtualEdges.Values.Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId.Replace(',', '-').Replace(':', '-'), target = i.endId.Replace(',', '-').Replace(':', '-') }));
            return dmodel;
        }

        public async Task<DisplayModel> GetRecognitionDisplayGraph(string compositeName)
        {
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            foreach (var robj in cont.recognitionRoots.Values)
            {
                RecursivelyAddElements(robj, dmodel, cont);
            }
            return dmodel;
        }

        public async Task<GraphObject?> GetVirtualObjectByLineage(string compositeName, string lineage)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (cont.virtualVertices.ContainsKey(lineage))
                return cont.virtualVertices[lineage];
            return null;
        }

        public async Task<GraphObject?> GetRecognitionObjectById(string compositeName, string id)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            if (cont.recognitionVertices.ContainsKey(id))
                return cont.recognitionVertices[id];
            return null;
        }

        public async Task<bool> SaveKSChanges(string userId, string subjectId, KnowledgeState ks)
        {
            return await _primitives.SaveKSChanges(userId, subjectId, ks);
        }

        public async Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, string graphName, bool externalIds)
        {
            return await _primitives.GetKnowledgeStateByExternalId(userId, extId, graphName, externalIds);
        }

        public async Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            return await _primitives.GetKnowledgeStateByTypeAndAttribute(userId, objectId, graphName, attLineage, attValue);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            return await _primitives.GetKnowledgeStatesByTypeAndAttribute(userId, objectId, graphName, attLineage, attValue);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage)
        {
            return await _primitives.GetKnowledgeStatesByTypeAndAttributeExistence(userId, objectId, graphName, attLineage);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName)
        {
            return await _primitives.GetKnowledgeStatesByType(userId, objectId, graphName);
        }

        public async Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state)
        {

            var kstate = await ConvertKS(state, userId);
            _knowledgeStateStream.OnNext(kstate);
            if (state.transient)
                return kstate;
            return await _primitives.CreateKnowledgeState(kstate);
        }

        public IObservable<KnowledgeState> ObservableKStates()
        {
            return _knowledgeStateStream.AsObservable();
        }

        public async Task<List<KnowledgeState>> CreateKnowledgeStateList(string userId, List<KnowledgeStateInput> states)
        {
            var results = new List<KnowledgeState>();
            foreach (var ks in states.Take(maxKnowledgeStateUpdates))
            {
                results.Add(await CreateKnowledgeState(userId, ks));
            }
            return results;
        }

        public async Task<KnowledgeState> DeleteKnowledgeState(string userId, string subjectId, string graphName)
        {
            return await _primitives.DeleteKnowledgeState(userId, subjectId, graphName);
        }

        public async Task ClearGraphContent(string compositeName)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            cont.edges.Clear();
            cont.recognitionEdges.Clear();
            cont.recognitionRoots.Clear();
            cont.recognitionVertices.Clear();
            cont.vertices.Clear();
            cont.virtualEdges.Clear();
            cont.virtualVertices.Clear();
        }

        public async Task<string> CopyRenameKG(string userId, string name, string newName)
        {
            return await _primitives.CopyRenameKG(userId, name, newName);
        }

        public async Task<GraphAttribute> UpdateRecognitionObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt)
        {
            var obj = await GetRecognitionObjectById(compositeName, objectId);
            return UpdateOrCreateAttribute(obj, graphAtt);
        }

        public async Task<GraphAttribute?> UpdateVirtualObjectAttribute(string compositeName, string lineage, GraphAttributeInput graphAtt)
        {
            var obj = await GetVirtualObjectByLineage(compositeName, lineage);
            if(obj != null)
                return UpdateOrCreateAttribute(obj, graphAtt);
            return null;
        }

        public async Task<GraphAttribute?> DeleteRecognitionObjectAttribute(string compositeName, string objectId, string graphLineage)
        {
            var obj = await GetRecognitionObjectById(compositeName, objectId);
            if(obj != null)
                return DeleteAttribute(obj, graphLineage);
            return null;
        }

        public async Task<GraphAttribute?> DeleteVirtualObjectAttribute(string compositeName, string objectLineage, string graphLineage)
        {
            var obj = await GetVirtualObjectByLineage(compositeName, objectLineage);
            if(obj != null)
                return DeleteAttribute(obj, graphLineage);
            return null;
        }

        public async Task<GraphAttribute?> UpdateGraphObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt)
        {
            var obj = await GetGraphObjectById(compositeName, objectId);
            return UpdateOrCreateAttribute(obj!, graphAtt);
        }

        public async Task<GraphAttribute?> DeleteGraphObjectAttribute(string compositeName, string objectId, string graphLineage)
        {
            var obj = await GetGraphObjectById(compositeName, objectId);
            if(obj != null)
                return DeleteAttribute(obj, graphLineage);
            return null;
        }

        public async Task<List<LineageRecord>> GetLineagesInKG(string compositeName, GraphElementType gtype)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var lins = cont.GetLineages(gtype);
            switch (gtype)
            {
                case GraphElementType.node:
                    lins.AddRange(_metaHandler.DefaultNodeLineages);
                    break;
                case GraphElementType.attribute:
                    lins.AddRange(_metaHandler.DefaultAttLineages);
                    break;
                case GraphElementType.connection:
                    lins.AddRange(_metaHandler.DefaultConnLineages);
                    break;
            }
            return lins.Distinct().OrderBy(a => a.typeWord).ToList();
        }


        public async Task<KnowledgeState> GetKnowledgeState(string userId, string Id, string graphName, bool external = false)
        {
            return await _primitives.GetKnowledgeState(userId, Id, graphName, external);
        }

        public bool FindMetaDisplayStructure(IGraphModel model, GraphObject res, ref DarlVar? pending, List<InteractTestResponse> responses)
        {
            return _metaHandler.FindMetaDisplayStructure(model, res, ref pending, responses);
        }

        public string? FindControlAttribute(IGraphModel model, string id)
        {
            return model.FindControlAttribute(id);
        }

        public void HandleCodelessValue(IGraphModel model, GraphObject res, DarlVar? pending, List<DarlVar> values, KnowledgeState ks)
        {
            _metaHandler.HandleCodelessValue(model, res, pending, values, ks);
        }

        public void HandleCodelessCompletion(IGraphModel model, GraphObject res, KnowledgeState ks)
        {
            _metaHandler.HandleCodelessCompletion(model, res, ks);
        }


        public async Task<GraphAttribute?> GetGraphAttribute(string userId, string graphName, string id, string lineage, string? ksUserId = null)
        {
            var model = await GetModel(userId, graphName);
            if (model == null)
                throw new RuleException($"Graph not found: {graphName}");
            var obj = await GetGraphObjectByExternalId($"{userId}_{graphName}", id);
            if (obj == null)
                obj = await GetGraphObjectById($"{userId}_{graphName}", id);
            if (obj == null)
                throw new RuleException($"Object not found: {id} in {graphName}");
            var ks = string.IsNullOrEmpty(ksUserId) ? null : await GetKnowledgeState(userId, ksUserId, graphName);
            return model.FindDataGraphAttribute(obj.id, lineage, ks);
        }

        public async Task<int> GetKGraphCountAsync(string userId)
        {
            return await _primitives.GetKGraphCountAsync(userId);
        }

        public async Task<string> GetGraphObjectToString(string compositeName, string id)
        {
            var obj = await GetGraphObjectById(compositeName, id);
            if (obj != null)
            {
                return obj.ToString();
            }
            return string.Empty;
        }

        public async Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            return await _primitives.ShareKGraph(userId, name, sharerId, readOnly, hidden);
        }

        public async Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName)
        {
            return await _primitives.GetSetOfKnowledgeStates(userId, ksIds, graphName);
        }

        public async Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName)
        {
            return await _primitives.GetSetofConnectedObjects(userId, ksIds, graphName);
        }

        public Task<string> CreateTimedAccessUrl(string userId, string name)
        {
            return Task.FromResult(_primitives.CreateTimedAccessUrl(userId, name));
        }

        public async Task<ModelMetaData> UpdateKGraph(string userId, string name, ModelMetaData kgupdate)
        {
            if (await _primitives.Load(CreateCompositeName(userId, name)) is not IGraphModel model)
                throw new Exception($"{name} could not be found in your account.");
            if (kgupdate.description != null)
            {
                model.description = kgupdate.description;
            }
            if (kgupdate.author != null)
            {
                model.author = kgupdate.author;
            }
            if (kgupdate.copyright != null)
            {
                model.copyright = kgupdate.copyright;
            }
            if (kgupdate.initialText != null)
            {
                model.initialText = kgupdate.initialText;
            }
            if (kgupdate.licenseUrl != null)
            {
                model.licenseUrl = kgupdate.licenseUrl;
            }
            if (kgupdate.dateDisplay != null)
            {
                model.dateDisplay = kgupdate.dateDisplay;
            }
            if (kgupdate.inferenceTime != null)
            {
                model.inferenceTime = kgupdate.inferenceTime;
            }
            if (kgupdate.fixedTime != null)
            {
                model.fixedTime = kgupdate.fixedTime;
            }
            if (kgupdate.defaultTarget != null)
            {
                model.defaultTarget = kgupdate.defaultTarget;
            }
            await _primitives.Store(CreateCompositeName(userId, name), model);
            return kgupdate;
        }

        public async Task<bool> Exists(string userId, string name)
        {
            return await _primitives.Exists(CreateCompositeName(userId, name));
        }

        public async Task<DisplayModel?> GetRealDisplayGraphWithState(string userId, string graphName, string subjectId)
        {
            var compositeName = userId + "_" + graphName;
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var ks = await GetKnowledgeState(userId, subjectId, graphName, false);
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            dmodel.nodes.AddRange(cont.vertices.Values.Select(i => new DisplayObject { id = i.id, name = i.name, lineage = ExtractLineage(i.lineage), subLineage = ExtractSubLineage(i.lineage), externalId = i.externalId }));
            dmodel.edges.AddRange(cont.edges.Values.Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            return dmodel;
        }

        public async Task<string> LoadExternalData(string userId, string name, string data, string patternPath, List<DataMap> dataMaps, LoadType ltype = LoadType.xml, bool buildGraph = false)
        {
            var model = await GetModel(userId, name);
            if (model == null)
                throw new RuleException($"Graph not found: {name}");
            var kstates = LoadData(userId, name, model, data, patternPath, dataMaps);
            if (buildGraph)
                BuildGraph(model, kstates, dataMaps);
            foreach (var k in kstates)
            {
                await _primitives.CreateKnowledgeState(k);
            }
            return $"{kstates.Count} KnowledgeStates created.";
        }

        public List<KnowledgeState> LoadData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps, LoadType ltype = LoadType.xml)
        {
            var kstates = new List<KnowledgeState>();
            switch (ltype)
            {
                case LoadType.xml:
                    kstates = _dataLoader.LoadXMLData(userId, name, model, data, patternPath, dataMaps);
                    break;
                case LoadType.json:
                    kstates = _dataLoader.LoadJsonData(userId, name, model, data, patternPath, dataMaps);
                    break;
                case LoadType.csv:
                    kstates = _dataLoader.LoadCsvData(userId, name, model, data, patternPath, dataMaps);
                    break;
            }
            return kstates;
        }

        private void BuildGraph(IGraphModel model, List<KnowledgeState> kstates, List<DataMap> dataMaps)
        {
            throw new NotImplementedException();
        }

        public async Task<KnowledgeState> ConvertKSIDs(KnowledgeState ks)
        {
            return await _primitives.ConvertKSIDs(ks);
        }

        public async Task<bool> ExistsInCache(string userId, string graphName)
        {
            return await _primitives.ExistsInCache(userId, graphName);
        }
        public async Task<byte[]> KGContents(string userId, string graphName)
        {
            return await _primitives.KGContents(userId, graphName);
        }

        public async Task<string> CreateTempKG(string userId, string graphName, byte[] bytes)
        {
            return await _primitives.CreateTempKG(userId, graphName, bytes);
        }

        public async Task<IEnumerable<GraphObject>> GetAllRecognitionObjects(string compositeName)
        {
            var cont = await _primitives.Load(compositeName) as IGraphModel;
            return cont.recognitionVertices.Values.ToList();
        }

        public List<GraphAttribute> ConvertAttributeInputList(List<GraphAttributeInput> list)
        {
            var l = new List<GraphAttribute>();
            foreach (var a in list)
                l.Add(ConvertAttributeInput(a));
            return l;
        }

        public List<GraphAttributeInput> ConvertAttributeInputList(List<GraphAttribute> list)
        {
            var l = new List<GraphAttributeInput>();
            foreach (var a in list)
                l.Add(ConvertAttributeInput(a));
            return l;
        }

        #region private_methods

        private GraphAttribute UpdateOrCreateAttribute(GraphObject obj, GraphAttributeInput graphAtt)
        {
            var lineage = CombineLineages(graphAtt.lineage, graphAtt.subLineage);
            if (obj.properties != null && obj.properties.Any(a => a.lineage == lineage))
            {
                var att = obj.properties.Where(a => a.lineage == lineage).FirstOrDefault();
                if (att != null)
                {
                    if (!string.IsNullOrEmpty(graphAtt.value))
                        att.value = graphAtt.value;
                    if (graphAtt.confidence != null)
                        att.confidence = graphAtt.confidence ?? 1.0;
                    att.type = graphAtt.type;
                    if (!string.IsNullOrEmpty(graphAtt.name))
                        att.name = graphAtt.name;
                    if (graphAtt.existence != null && graphAtt.existence.Any())
                    {
                        att.existence = graphAtt.existence;
                    }
                    if (graphAtt.properties != null)
                    {
                        if (att.properties == null)
                            att.properties = new List<GraphAttribute>();
                        foreach (var prop in graphAtt.properties)
                        {
                            if (!att.properties.Any(a => a.name == prop.name && a.value == prop.value))
                                att.properties.Add(new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = prop.lineage, name = prop.name, type = prop.type, value = prop.value, confidence = prop.confidence ?? 1.0 });
                        }
                    }
                }
                return att;
            }
            else
            {
                if (obj.properties == null)
                    obj.properties = new List<GraphAttribute>();
                var att = new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = lineage, confidence = graphAtt.confidence ?? 1.0, existence = graphAtt.existence, name = graphAtt.name, type = graphAtt.type, value = graphAtt.value };
                if (graphAtt.properties != null)
                {
                    att.properties = new List<GraphAttribute>();
                    foreach (var prop in graphAtt.properties)
                    {
                        att.properties.Add(new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = prop.lineage, name = prop.name, type = prop.type, value = prop.value, confidence = prop.confidence ?? 1.0 });
                    }
                }
                obj.properties.Add(att);
                return att;
            }
        }

        private void CreateRawObject(IGraphModel model, GraphObjectInput graphObject)
        {
            var go = new GraphObject { existence = graphObject.existence, externalId = graphObject.externalId, id = Guid.NewGuid().ToString(), inferred = false, lineage = graphObject.lineage, name = graphObject.name, properties = ConvertAttributeInputList(graphObject.properties) };
            model.vertices.Add(go.id, go);
        }



        private void CreateVirtualObject(IGraphModel model, string lineage, string typeword, string description)
        {
            var go = new GraphObject { id = Guid.NewGuid().ToString(), inferred = false, lineage = lineage, name = typeword, _virtual = true, properties = new List<GraphAttribute> { new GraphAttribute { name = "description", value = description, lineage = "noun:01,4,05,21,05", type = GraphAttribute.DataType.textual } } };
            model.virtualVertices.Add(go.lineage, go);
        }

        private GraphConnection CreateConnection(IGraphModel cont, GraphConnectionInput conn)
        {
            if (string.IsNullOrEmpty(conn.id))
                conn.id = Guid.NewGuid().ToString();
            var gc = new GraphConnection { id = conn.id, endId = conn.endId, existence = conn.existence, inferred = false, lineage = conn.lineage, name = conn.name, properties = conn.properties, startId = conn.startId, weight = conn.weight ?? 1.0, _virtual = false };
            if (cont != null && cont.vertices.ContainsKey(conn.startId))
                cont.vertices[conn.startId].Out.Add(gc);
            else
                throw new Exception($"Real vertex id {conn.startId} does not exist");
            if (cont != null && cont.vertices.ContainsKey(conn.endId))
                cont.vertices[conn.endId].In.Add(gc);
            else
                throw new Exception($"Real vertex id {conn.endId} does not exist");
            cont.edges.Add(gc.id, gc);
            return gc;
        }

        private string ExtractLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if (pos == -1)
            {
                return lineage;
            }
            return lineage.Substring(0, pos);
        }
        private string ExtractSubLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if (pos == -1)
            {
                return null;
            }
            return lineage.Substring(pos + 1);
        }

        private void RecursivelyAddElements(GraphObject robj, DisplayModel dmodel, IGraphModel cont)
        {
            dmodel.nodes.Add(new DisplayObject { id = robj.id ?? String.Empty, name = robj.name ?? String.Empty, lineage = robj.lineage ?? String.Empty });
            GraphConnection? orphan = null;
            foreach (var c in robj.Out)
            {
                if (cont.recognitionVertices.ContainsKey(c.endId))
                {
                    dmodel.edges.Add(new DisplayConnection { id = c.id ?? String.Empty, name = c.name ?? String.Empty, source = c.startId, target = c.endId });
                    RecursivelyAddElements(cont.recognitionVertices[c.endId], dmodel, cont);
                }
                else
                {//delete orphans
                    orphan = c;
                }
            }
            if (orphan != null)
            {
                robj.Out.Remove(orphan);
                cont.recognitionEdges.Remove(orphan.id ?? String.Empty);
            }
        }

        private nodetype ConvertNode(GraphObject node, Dictionary<string, keytype> atts)
        {
            var n = new nodetype { id = node.id };
            var items = new List<datatype>();
            if (node.existence != null)
            {
                if (!atts.ContainsKey(nameof(node.existence)))
                {
                    atts.Add(nameof(node.existence), new keytype { @for = keyfortype.node, id = nameof(node.existence), name = nameof(node.existence), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(node.existence), Text = node.existence.Select(a => a.ToString()).ToArray() });
            }
            if (!string.IsNullOrEmpty(node.externalId))
            {
                if (!atts.ContainsKey(nameof(node.externalId)))
                {
                    atts.Add(nameof(node.externalId), new keytype { @for = keyfortype.node, id = nameof(node.externalId), name = nameof(node.externalId), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(node.externalId), Text = new string[] { node.externalId } });
            }
            if (!atts.ContainsKey(nameof(node.inferred)))
            {
                atts.Add(nameof(node.inferred), new keytype { @for = keyfortype.node, id = nameof(node.inferred), name = nameof(node.inferred), type = keytypetype.boolean });
            }
            if (!string.IsNullOrEmpty(node.lineage))
            {
                if (!atts.ContainsKey(nameof(node.lineage)))
                {
                    atts.Add(nameof(node.lineage), new keytype { @for = keyfortype.node, id = nameof(node.lineage), name = nameof(node.lineage), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(node.lineage), Text = new string[] { node.lineage } });
            }
            if (!string.IsNullOrEmpty(node.name))
            {
                if (!atts.ContainsKey(nameof(node.name)))
                {
                    atts.Add(nameof(node.name), new keytype { @for = keyfortype.node, id = nameof(node.name), name = nameof(node.name), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(node.name), Text = new string[] { node.name } });
            }
            if (!atts.ContainsKey("virtual"))
            {
                atts.Add("virtual", new keytype { @for = keyfortype.node, id = "virtual", name = "virtual", type = keytypetype.boolean });
            }
            items.Add(new datatype { key = "virtual", Text = new string[] { node._virtual.ToString() } });
            if (node.properties != null)
            {
                foreach (var s in node.properties)
                {
                    if (!atts.ContainsKey(s.name))
                    {
                        atts.Add(s.name, new keytype { @for = keyfortype.node, id = s.name, name = s.name, type = keytypetype.@string });
                    }
                    items.Add(new datatype { key = s.name, Text = new string[] { s.value } });
                }
            }
            n.Items = items.ToArray();
            return n;
        }

        private GraphObjectInput ConvertNode(nodetype node, Dictionary<string, string> atts)
        {
            var go = new GraphObjectInput { properties = new List<GraphAttributeInput>() };
            foreach (datatype i in node.Items)
            {
                if (atts.ContainsKey(i.key))
                {
                    switch (atts[i.key])
                    {
                        case nameof(GraphObject.name):
                            go.name = i.Text[0];
                            break;
                        case nameof(GraphObject.lineage):
                            go.lineage = i.Text[0];
                            break;
                        case nameof(GraphObject.externalId):
                            go.externalId = i.Text[0];
                            break;
                        case nameof(GraphObject.existence):
                            {
                                go.existence = new List<DarlTime?>();
                                foreach (var d in i.Text)
                                {
                                    go.existence.Add(DarlTime.Parse(d));
                                }
                            }
                            break;
                        default:
                            go.properties.Add(new GraphAttributeInput { name = atts[i.key], value = i.Text[0] });
                            break;
                    }
                }
            }
            return go;
        }

        private GraphConnection ConvertConnection(edgetype edge, Dictionary<string, string> atts)
        {
            var gc = new GraphConnection { id = edge.id, inferred = false, _virtual = false };
            foreach (datatype i in edge.data)
            {
                if (atts.ContainsKey(i.key))
                {
                    switch (atts[i.key])
                    {
                        case nameof(GraphConnection.name):
                            gc.name = i.Text[0];
                            break;
                        case nameof(GraphConnection.lineage):
                            gc.lineage = i.Text[0];
                            break;
                        case nameof(GraphConnection.existence):
                            {
                                gc.existence = new List<DarlTime?>();
                                foreach (var d in i.Text)
                                {
                                    gc.existence.Add(DarlTime.Parse(d));
                                }
                            }
                            break;
                        case nameof(GraphConnection.startId):
                            gc.startId = i.Text[0];
                            break;
                        case nameof(GraphConnection.endId):
                            gc.endId = i.Text[0];
                            break;
                        case nameof(GraphConnection.weight):
                            gc.weight = double.Parse(i.Text[0]);
                            break;
                        default:
                            gc.properties.Add(new GraphAttribute { name = atts[i.key], value = i.Text[0] });
                            break;
                    }
                }
            }
            return gc;
        }


        private edgetype ConvertConnection(GraphConnection conn, Dictionary<string, keytype> atts)
        {
            var c = new edgetype { id = conn.id, directed = true, source = conn.startId, target = conn.endId };
            c.directed = true;
            var items = new List<datatype>();
            if (conn.existence != null)
            {
                if (!atts.ContainsKey(nameof(conn.existence)))
                {
                    atts.Add(nameof(conn.existence), new keytype { @for = keyfortype.edge, id = nameof(conn.existence), name = nameof(conn.existence), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(conn.existence), Text = conn.existence.Select(a => a.ToString()).ToArray() });
            }
            if (!atts.ContainsKey(nameof(conn.inferred)))
            {
                atts.Add(nameof(conn.inferred), new keytype { @for = keyfortype.edge, id = nameof(conn.inferred), name = nameof(conn.inferred), type = keytypetype.boolean });
            }
            if (!string.IsNullOrEmpty(conn.lineage))
            {
                if (!atts.ContainsKey(nameof(conn.lineage)))
                {
                    atts.Add(nameof(conn.lineage), new keytype { @for = keyfortype.edge, id = nameof(conn.lineage), name = nameof(conn.lineage), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(conn.lineage), Text = new string[] { conn.lineage } });
            }
            if (!string.IsNullOrEmpty(conn.name))
            {
                if (!atts.ContainsKey(nameof(conn.name)))
                {
                    atts.Add(nameof(conn.name), new keytype { @for = keyfortype.edge, id = nameof(conn.name), name = nameof(conn.name), type = keytypetype.@string });
                }
                items.Add(new datatype { key = nameof(conn.name), Text = new string[] { conn.name } });
            }
            if (!atts.ContainsKey("virtual"))
            {
                atts.Add("virtual", new keytype { @for = keyfortype.edge, id = "virtual", name = "virtual", type = keytypetype.boolean });
            }
            items.Add(new datatype { key = "virtual", Text = new string[] { conn._virtual.ToString() } });
            if (conn.properties != null)
            {
                foreach (var s in conn.properties)
                {
                    if (!atts.ContainsKey(s.name))
                    {
                        atts.Add(s.name, new keytype { @for = keyfortype.edge, id = s.name, name = s.name, type = keytypetype.@string });
                    }
                    items.Add(new datatype { key = s.name, Text = new string[] { s.value } });
                }
            }
            if (!atts.ContainsKey(nameof(conn.weight)))
            {
                atts.Add(nameof(conn.weight), new keytype { @for = keyfortype.edge, id = nameof(conn.weight), name = nameof(conn.weight), type = keytypetype.@double });
            }
            items.Add(new datatype { key = nameof(conn.weight), Text = new string[] { conn.weight.ToString() } });
            c.data = items.ToArray();
            return c;
        }

        private async Task<KnowledgeState> ConvertKS(KnowledgeStateInput state, string userId)
        {
            if (!state.transient) //check if wjole KG is marked transient
            {
                var model = await GetModel(userId, state.knowledgeGraphName);
                if (model == null)
                    throw new RuleException($"Graph not found: {state.knowledgeGraphName}");
                state.transient = model.transient;
            }
            var kstate = new KnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = state.subjectId, userId = userId, created = DateTime.UtcNow };
            foreach (var s in state.data)
            {
                if (!kstate.data.ContainsKey(s.name))
                {
                    kstate.data.Add(s.name, new List<GraphAttribute>());
                    foreach (var g in s.value)
                    {
                        kstate.data[s.name].Add(new GraphAttribute { confidence = g.confidence ?? 1.0, existence = g.existence, id = Guid.NewGuid().ToString(), inferred = g.inferred ?? false, lineage = g.lineage, name = g.name, value = g.value, type = g.type });
                    }
                }
            }
            return kstate;
        }


        private bool LineageExists(IGraphModel model, string lineage)
        {
            return model.virtualVertices.ContainsKey(lineage);
        }

        private string GetAttibuteGivenObject(GraphObject obj, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(GraphObject.name):
                    return obj.name ?? String.Empty;
                case nameof(GraphObject.existence):
                    return string.Join(",", obj.existence ?? new List<Common.DarlTime?>());
                case nameof(GraphObject.lineage):
                    return obj.lineage ?? String.Empty;
                case nameof(GraphObject.externalId):
                    return obj.externalId;
                case nameof(GraphObject.id):
                    return obj.id ?? String.Empty;
                case nameof(GraphObject.inferred):
                    return obj.inferred.ToString();
                default:
                    if (obj.properties != null && obj.properties.Where(a => a.name == propertyName).Any())
                    {
                        return obj.properties.Where(a => a.name == propertyName).First().value;
                    }
                    return string.Empty;
            }
        }


        private bool VirtualAssociationExists(IGraphModel model, string lineage1, string lineage2)
        {
            if (!model.virtualVertices.ContainsKey(lineage1) || !model.virtualVertices.ContainsKey(lineage2))
                return false;
            var obj1 = model.virtualVertices[lineage1];
            var obj2 = model.virtualVertices[lineage2];
            //get the set of direct ancestors for each object
            var list1 = new List<GraphObject>();
            FollowHypernymy(model, obj1, list1);
            var list2 = new List<GraphObject>();
            var otherIds = list2.Select(a => a.id).ToList();
            FollowHypernymy(model, obj2, list2);
            //now search for connections
            foreach (var o in list1)
            {
                if (o.Out.Where(a => otherIds.Contains(a.id)).Any())
                    return true;
            }
            return false;
        }

        private void FollowHypernymy(IGraphModel model, GraphObject g, List<GraphObject> list)
        {
            if (model is IGraphModel cont)
            {
                foreach (var l in g.Out.Where(a => a.name == "kind_of"))
                {
                    var parent = cont.virtualVertices[l.endId];
                    list.Add(parent);
                    FollowHypernymy(model, parent, list);
                }
            }
        }

        private async Task<GraphConnection?> UpdateConnection(string compositeName, GraphConnectionUpdate gc)
        {
            var conn = await GetConnectionById(compositeName, gc.id);
            if (conn == null)
                return null;
            //update nun-null elements in gc
            bool changed = false;
            if (gc.existence != null)
            {
                conn.existence = gc.existence;
                changed = true;
            }
            if (gc.lineage != null) //allow change of lineage?
            {
                if (conn.lineage != gc.lineage)
                {
                    conn.lineage = gc.lineage;
                    changed = true;
                }
            }
            if (gc.name != null)
            {
                if (conn.name != gc.name)
                {
                    conn.name = gc.name;
                    changed = true;
                }
            }
            if (gc.weight != null)
            {
                if (conn.weight != (gc.weight ?? 1.0))
                {
                    conn.weight = gc.weight ?? 1.0;
                    changed = true;
                }
            }
            if (gc.inferred != null)
            {
                if (conn.inferred != gc.inferred)
                {
                    conn.inferred = gc.inferred ?? false;
                    changed = true;
                }
            }
            if (gc.properties != null && gc.properties.Any())
            {
                conn.properties = gc.properties;
                changed = true;

            }
            return conn;
        }

        private GraphAttribute? DeleteAttribute(GraphObject obj, string attLineage)
        {
            if (obj.properties != null)
            {
                var att = obj.properties.Where(a => a.lineage == attLineage).FirstOrDefault();
                if (att != null)
                {
                    obj.properties.Remove(att);
                }
                return att;
            }
            return null;
        }

        private void CreateVirtualConnection(IGraphModel model, string child, string lineage, string connectionLabel)
        {
            if (!model.virtualEdges.ContainsKey(($"{child}->{lineage}")))
            {
                var gc = new GraphConnection { id = Guid.NewGuid().ToString(), endId = lineage, inferred = false, name = connectionLabel, startId = child, weight = 1.0, _virtual = true };
                if (model.virtualVertices.ContainsKey(child))
                    model.virtualVertices[child].Out.Add(gc);
                else
                    throw new Exception($"Virtual vertex id {child} does not exist");
                if (model.virtualVertices.ContainsKey(lineage))
                    model.virtualVertices[lineage].In.Add(gc);
                else
                    throw new Exception($"Virtual vertex id {lineage} does not exist");
                model.virtualEdges.Add($"{child}->{lineage}", gc);
            }
        }

        private GraphObject GetRecognitionRoot(IGraphModel model, string rootLineage)
        {
            if (model.recognitionRoots.ContainsKey(rootLineage))
                return model.recognitionRoots[rootLineage];
            throw new Exception($"Recognition root '{rootLineage}' does not exist");
        }

        /// <summary>
        /// Dijkstra's shortest path algorithm
        /// </summary>
        /// <param name="model"></param>
        /// <param name="start"></param>
        /// <param name="Target"></param>
        /// <returns>the vertex,edge,vertex... sequence</returns>
        /// <remarks></remarks>
        private List<GraphElement>? ShortestPath(IGraphModel model, GraphObject? start, GraphObject? target)
        {
            if (start != null && target != null)
            {
                var list = new List<GraphElement> { start };
                var coverage = new Dictionary<GraphObject, (double, bool, int)> { { start, (0, true, 0) } };
                try
                {
                    var path = ShortestPathRecursion(model, start, target, list, coverage, 0);
                }
                catch (Exception)
                {

                }
                var shortestPath = new List<GraphElement> { target };
                var next = target;
                while (next != start)
                {
                    if (!coverage.ContainsKey(next))
                    {
                        return null;
                    }
                    var potential = coverage[next].Item1;
                    bool found = false;
                    foreach (var i in next.In)
                    {
                        var begin = model.vertices[i.startId];
                        if (!coverage.ContainsKey(begin))
                            continue;
                        var otherPotential = coverage[begin].Item1;
                        if (potential - otherPotential == i.weight)
                        {
                            shortestPath.Add(i);
                            shortestPath.Add(begin);
                            next = begin;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return null;
                }
                shortestPath.Reverse();
                return shortestPath;
            }
            return null;
        }

        private async Task CorrectBrokenLinks(string compositeName)
        {
            if (await _primitives.Load(compositeName) is not IGraphModel cont)
                throw new Exception($"Graph  '{compositeName}' does not exist.");
            var deleteList = new List<GraphConnection>();
            foreach (var n in cont.vertices.Values)
            {
                foreach (var c in n.Out)
                {
                    if (!cont.vertices.ContainsKey(c.endId))
                        deleteList.Add(c);
                }
                foreach (var c in deleteList)
                {
                    n.Out.Remove(c);
                    cont.edges.Remove(c.id);
                }
            }
        }

        private List<GraphElement>? ShortestPathRecursion(IGraphModel model, GraphObject start, GraphObject target, List<GraphElement> path, Dictionary<GraphObject, (double, bool, int)> coverage, int depth)
        {
            if (depth > maxDepth)
                maxDepth = depth;
            var current = new List<(GraphObject, double, List<GraphElement>)>();
            foreach (var c in start.Out)
            {
                var newPath = new List<GraphElement>(path);
                var next = model.vertices[c.endId];
                newPath.Add(c);
                newPath.Add(next);
                var distance = coverage[start].Item1 + c.weight;
                if (coverage.ContainsKey(next))
                {
                    var oldDistance = coverage[next].Item1;
                    var visitCount = coverage[next].Item3 + 1;

                    if (distance < oldDistance)
                    {
                        coverage[next] = (distance, visitCount >= next.In.Count, visitCount);
                    }
                    else
                    {
                        coverage[next] = (oldDistance, visitCount >= next.In.Count, visitCount);
                    }
                }
                else
                {
                    coverage.Add(next, (distance, next.In.Count == 1, 1));
                }
                current.Add((next, distance, newPath));
                if (coverage[next].Item2)
                {
                    if (next == target)
                        return newPath;
                }

            }
            /*           foreach (var c in start.In)
                       {
                           var newPath = new List<GraphElement>(path);
                           var next = cont.vertices[c.startId];
                           if (coverage.ContainsKey(next))
                           {
                               continue;
                           }
                           newPath.Add(c);
                           newPath.Add(next);
                           var distance = coverage[start].Item1 + c.weight;
                           coverage.Add(next, (distance, true));
                           current.Add((next, distance, newPath));
                           if (next == target)
                               return newPath;
                       }*/
            //sort current by distance descending
            current = current.OrderBy(a => a.Item2).ToList();
            foreach (var v in current)
            {
                if (depth < 10)
                {
                    var found = ShortestPathRecursion(model, v.Item1, target, new List<GraphElement>(v.Item3), coverage, ++depth);
                    if (coverage.ContainsKey(target) && coverage[target].Item2)
                        return found;
                }
            }
            return null;
        }


        /// <summary>
        /// Recursively add an object to an ontology and all its parents
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="lineage"></param>
        /// <returns></returns>
        private void AddObjectToOntology(IGraphModel model, string lineage, string? child = null)
        {
            if (!LineageExists(model, lineage))
            {
                //handle composite lineages
                var divider = lineage.IndexOf('+');
                var lin = divider == -1 ? lineage : lineage.Substring(0, divider);
                //Add this lineage element
                if (LineageLibrary.lineages.ContainsKey(lin))
                {
                    if (!LineageExists(model, lin))
                    {
                        var l = LineageLibrary.lineages[lin];
                        CreateVirtualObject(model, lin, l.typeWord, l.description);
                        if (lin.Contains(","))
                        {
                            //remove last element
                            //call this function recursively with 
                            AddObjectToOntology(model, lin.Substring(0, lin.LastIndexOf(',')), lin);
                        }
                    }
                }
                if (divider != -1) //composite - we've added the primary part, now add the secondary
                {
                    var sub = lineage.Substring(divider + 1);
                    if (!LineageExists(model, sub))
                    {
                        if (LineageLibrary.lineages.ContainsKey(sub))
                        {
                            var l = LineageLibrary.lineages[lin];
                            var s = LineageLibrary.lineages[sub];
                            CreateVirtualObject(model, lineage, $"{l.typeWord}/{s.typeWord}", $"{l.description} \n {s.description}");
                            CreateVirtualConnection(model, lineage, lin, "kind_of");
                        }
                    }
                }
            }
            if (child != null)
            {
                //connect the child to this object.
                CreateVirtualConnection(model, child, lineage, "kind_of");
            }
        }

        private bool OntologicalCompliance(IGraphModel model, string graphObjectLineage, string propertyLineage)
        {
            //are these concepts connected?
            if (VirtualAssociationExists(model, graphObjectLineage, propertyLineage))
                return true;
            //check for 'has' relationship
            return OntologicalCompliance(model, _metaHandler.CommonLineages["have"], graphObjectLineage, propertyLineage);
        }

        private void DeleteRecognitionOrphans(IGraphModel model)
        {
            var complete = false;
            while (!complete)
            {
                var deletions = new List<GraphObject>();
                foreach (var o in model.recognitionVertices.Values)
                {
                    if (!o.In.Any() && !model.recognitionRoots.Values.Contains(o))
                    {
                        deletions.Add(o);
                    }
                }
                foreach (var o in deletions)
                {
                    foreach (var c in o.Out)
                    {
                        model.recognitionVertices[c.endId].In.Remove(c);
                        model.recognitionEdges.Remove(c.id);
                    }
                    model.recognitionVertices.Remove(o.id);
                }
                if (!deletions.Any())
                    complete = true;
            }
        }


        private bool OntologicalCompliance(IGraphModel model, string graphConnectionLineage, string startLineage, string endLineage)
        {
            //Look for a preceding and a following association in this or higher verbs that permits this.
            if (VirtualAssociationExists(model, startLineage, graphConnectionLineage))
            {
                return VirtualAssociationExists(model, endLineage, graphConnectionLineage);
            }
            return false;
        }

        private GraphObject? CreateObject(IGraphModel model, GraphObjectInput graphObject)
        {
            var go = new GraphObject { existence = graphObject.existence, externalId = graphObject.externalId, id = Guid.NewGuid().ToString(), inferred = false, lineage = graphObject.lineage, name = graphObject.name, _virtual = false, properties = graphObject.properties != null ? ConvertAttributeInputList(graphObject.properties) : null };
            model.vertices.Add(go.id, go);
            return go;
        }

        private void CreateRawConnection(IGraphModel cont, GraphConnection conn)
        {
            var gc = new GraphConnection { id = Guid.NewGuid().ToString(), endId = conn.endId, existence = conn.existence, inferred = false, lineage = conn.lineage, name = conn.name, properties = conn.properties, startId = conn.startId, weight = conn.weight, _virtual = conn._virtual };
            if (cont.vertices.ContainsKey(conn.startId))
                cont.vertices[conn.startId].Out.Add(gc);
            else
                throw new Exception($"Real vertex id {conn.startId} does not exist");
            if (cont.vertices.ContainsKey(conn.endId))
                cont.vertices[conn.endId].In.Add(gc);
            else
                throw new Exception($"Real vertex id {conn.endId} does not exist");
            //cont.edges.Add(gc.id, gc);
        }


        private async Task<GraphObject?> UpdateObject(string compositeName, GraphObjectUpdate go)
        {
            var node = await GetGraphObjectById(compositeName, go.id);
            if (node == null)
                return null;
            //update nun-null elements in go
            bool changed = false;
            if (go.existence != null)
            {
                node.existence = go.existence;
                changed = true;
            }
            if (!string.IsNullOrEmpty(go.lineage)) //allow change of lineage?
            {
                if (node.lineage != go.lineage)
                {
                    node.lineage = go.lineage;
                    changed = true;
                }
            }
            if (!string.IsNullOrEmpty(go.name))
            {
                if (node.name != go.name)
                {
                    node.name = go.name;
                    changed = true;
                }
            }
            if (go.properties != null && go.properties.Any())
            {
                foreach (var a in go.properties)
                {
                    if (node.properties == null)
                    {
                        node.properties = new List<GraphAttribute>();
                    }
                    var found = node.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                    if (found != null)
                    {
                        node.properties.Remove(found);
                    }
                    node.properties.Add(ConvertAttributeInput(a));
                }
            }
            else if (go.properties != null && !go.properties.Any())
            {
                if (node.properties != null)
                    node.properties.Clear();
            }
            return node;
        }


        private static GraphAttribute ConvertAttributeInput(GraphAttributeInput a)
        {
            List<GraphAttribute>? properties = null;
            if (a.properties != null)
            {
                properties = new List<GraphAttribute>();
                foreach (var property in a.properties)
                {
                    properties.Add(ConvertAttributeInput(property));
                }
            }
            return new GraphAttribute { id = Guid.NewGuid().ToString(), confidence = a.confidence ?? 1.0, inferred = a.inferred ?? false, value = a.value, existence = a.existence, name = a.name, type = a.type, lineage = a.lineage, properties = properties };
        }

        private static GraphAttributeInput ConvertAttributeInput(GraphAttribute a)
        {
            return new GraphAttributeInput { confidence = a.confidence, inferred = a.inferred, value = a.value, existence = a.existence, name = a.name, type = a.type, lineage = a.lineage };
        }



        /// <summary>
        /// Add the ontology elements for this object
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="graphObjectLineage"></param>
        /// <param name="propertyLineage"></param>
        /// <returns></returns>
        private void BuildOntology(IGraphModel model, string graphObjectLineage, string propertyLineage)
        {
            //add the property to the ontology
            AddObjectToOntology(model, propertyLineage);
            if (_metaHandler.IsObjectLineage(propertyLineage))
            {
                //add the "has" link to the ontology
                CreateVirtualConnection(model, graphObjectLineage, propertyLineage, "has");
            }

        }

        /// <summary>
        /// Add the ontology elements for this connection
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="graphConnectionLineage"></param>
        /// <param name="startLineage"></param>
        /// <param name="endLineage"></param>
        /// <returns></returns>
        private void BuildOntology(IGraphModel model, string graphConnectionLineage, string startLineage, string endLineage)
        {
            if (!LineageExists(model, graphConnectionLineage))
            {
                AddObjectToOntology(model, graphConnectionLineage);
            }
            //now add precedes and follows links
            CreateVirtualConnection(model, startLineage, graphConnectionLineage, "precedes");
            CreateVirtualConnection(model, graphConnectionLineage, endLineage, "follows");
        }

        private string CombineLineages(string lineage, string subLineage)
        {
            if (string.IsNullOrEmpty(subLineage))
                return lineage;
            return $"{lineage}+{subLineage}";
        }


        #endregion
    }

}
