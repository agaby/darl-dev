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
                    if (!await OntologicalCompliance(model, graphConnection.lineage, startLineage, endLineage))
                    {
                        throw new RuleException($"No association exists between {startLineage}, the verb {graphConnection.lineage} and {endLineage}\n if you are sure this is correct use the definitive flag in the call.");
                    }
                }
                else
                {
                    await BuildOntology(model, graphConnection.lineage, startLineage, endLineage);
                }
                foreach (var p in graphConnection.properties)
                {
                    if (!LineageLibrary.CheckLineage(p.lineage))
                        throw new RuleException($"Malformed property lineage: {p.lineage}.");
                    if (ontology == OntologyAction.check)
                    {
                        if (!await OntologicalCompliance(model, graphConnection.lineage, p.lineage))
                        {
                            throw new RuleException($"No association exists between {graphConnection.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                    else
                    {
                        await BuildOntology(model, graphConnection.lineage, p.lineage);
                    }
                }
            }
            return await _primitives.CreateConnection(compositeName, graphConnection);

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
                if (!await LineageExists(model, graphObject.lineage))
                {
                    await AddObjectToOntology(model, graphObject.lineage);
                }
                if (graphObject.properties != null)
                {
                    foreach (var p in graphObject.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.lineage))
                            throw new RuleException($"Malformed property lineage: {p.lineage}.");
                        if (ontology == OntologyAction.check)
                        {
                            if (!await OntologicalCompliance(model, graphObject.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphObject.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            await BuildOntology(model, graphObject.lineage, p.lineage);
                        }
                    }
                }
            }
            return await _primitives.CreateObject(compositeName, graphObject);

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


        internal static string CreateCompositeName(string userId, string name)
        {
            return userId + "_" + name.Replace(" ", "_");
        }

        public async Task<GraphConnection?> DeleteGraphConnection(string compositeName, string id)
        {
            return await _primitives.DeleteConnection(compositeName, id);
        }

        public async Task<GraphObject?> DeleteGraphObject(string compositeName, string id)
        {
            return await _primitives.DeleteObject(compositeName, id);
        }

        public async Task<GraphObject?> GetGraphObjectById(string compositeName, string? id)
        {
            if (id == null)
                throw new RuleException($"GetGraphObjectById: Id cannot be null.");
            return await _primitives.GetGraphObjectById(compositeName, id ?? "");
        }

        public async Task<List<GraphObject>?> GetGraphObjects(string compositeName, string name, string lineage)
        {
            return await _primitives.GetGraphObjects(compositeName, name, lineage);
        }


        public async Task<string> GetGraphObjectProperty(string compositeName, string id, string property)
        {
            return await _primitives.GetGraphObjectProperty(compositeName, id, property);
        }

        public async Task<GraphObject> GetGraphObjectByExternalId(string compositeName, string externalId)
        {
            return await _primitives.GetGraphObjectByExternalId(compositeName, externalId);
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
                            if (!await OntologicalCompliance(model, graphConnection.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphConnection.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            await BuildOntology(model, graphConnection.lineage, p.lineage);
                        }
                    }
                }
            }
            return await _primitives.UpdateConnection(compositeName, graphConnection);

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
                if (!await LineageExists(model, graphObject.lineage))
                {
                    await AddObjectToOntology(model, graphObject.lineage);
                }
                if (graphObject.properties != null)
                {
                    foreach (var p in graphObject.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.lineage))
                            throw new RuleException($"Malformed property lineage: {p.lineage}.");
                        if (ontology == OntologyAction.check)
                        {
                            if (!await OntologicalCompliance(model, graphObject.lineage, p.lineage))
                            {
                                throw new RuleException($"No association exists between {graphObject.lineage} and {p.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            await BuildOntology(model, graphObject.lineage, p.lineage);
                        }
                    }
                }
            }
            return await _primitives.UpdateObject(compositeName, graphObject);
        }



        /// <summary>
        /// Get a connection based on the node ids and the lineage
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="lineage"></param>
        /// <returns>The partially filled in connection</returns>
        public async Task<GraphConnection> GetConnectionByIds(string compositeName, string startId, string endId, string lineage)
        {
            return await _primitives.GetConnectionByIds(compositeName, startId, endId, lineage);
        }

        public async Task<GraphConnection> GetConnectionById(string compositeName, string id)
        {
            return await _primitives.GetConnectionById(compositeName, id);
        }

        public async Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID)
        {
            return await _primitives.ProcessPath(compositeName, startExternalID, endExternalID);
        }

        public async Task<string> ProcessAttribute(string compositeName, string externalID, string propertyName)
        {
            return await _primitives.GetAttribute(compositeName, externalID, propertyName);
        }

        /// <summary>
        /// get categories from objects of the lineage given linked to the root object.
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="rootName"></param>
        /// <param name="childLineage"></param>
        /// <param name="childValueAttribute"></param>
        /// <returns></returns>
        public async Task<List<StringStringPair>> ProcessCategories(string compositeName, string rootName, string childLineage, string childValueAttribute)
        {
            return await _primitives.GetLinkedCategories(compositeName, rootName, childLineage, childValueAttribute);
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
            return await _primitives.GetCategoriesByLineage(compositeName, childLineage, childValueAttribute);
        }
        public async Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage)
        {
            return await _primitives.GetGraphObjectsByLineage(compositeName, lineage);
        }

        public async Task Store(string compositeName)
        {
            await _primitives.Store(compositeName);
        }



        public async Task CreateVirtualAttribute(string compositeName, string lineage, GraphAttributeInput att)
        {
            await _primitives.CreateVirtualAttribute(compositeName, lineage, att);
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
            if (graphRoot != null && graphRoot.Items.Length > 1)
            {
                var model = await _primitives.Load(compositeName);
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
                            await _primitives.CreateVirtualObject(model, lineage, name, desc);
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
                            await _primitives.CreateVirtualConnection(model, child, lineage, name);
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
                            await _primitives.CreateRawObject(model, ConvertNode(e as nodetype, atts));
                        }
                        else if (e is edgetype)
                        {
                            await _primitives.CreateRawConnection(model, ConvertConnection(e as edgetype, atts));
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
                    await _primitives.CreateObject(compositeName, new GraphObjectInput { externalId = node.id, existence = obj.existence, lineage = obj.lineage, name = obj.name, properties = obj.properties });
                }

            }
            foreach (var e in graph.Items)
            {
                if (e is edgetype)
                {
                    var edge = e as edgetype;
                    var obj = ConvertConnection(edge, atts);
                    await _primitives.CreateConnection(compositeName, new GraphConnectionInput { existence = obj.existence, lineage = obj.lineage, name = obj.name, properties = obj.properties, endId = obj.endId, startId = obj.startId, weight = obj.weight });
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

            foreach (var node in await _primitives.GetAllRealObjects(compositeName))
            {
                realNodes.Add(ConvertNode(node, atts));
            }

            foreach (var node in await _primitives.GetAllVirtualObjects(compositeName))
            {
                virtualNodes.Add(ConvertNode(node, atts));
            }

            foreach (var conn in await _primitives.GetAllRealConnections(compositeName))
            {
                realConns.Add(ConvertConnection(conn, atts));
            }

            foreach (var conn in await _primitives.GetAllVirtualConnections(compositeName))
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


        public async Task<List<MatchedElement>> Match(string compositeName, string subjectId, List<string> tokens)
        {
            bool fuzzy = false;
            var model = await _primitives.Load(compositeName);
            GraphObject root = await _primitives.GetRecognitionRoot(model, subjectId);
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
            _logger.LogDebug($"Match: found {compMatches.Count} matches.");
            return compMatches;
        }

        public async Task<GraphObject> CreateRecognitionRoot(string compositeName, string rootLineage)
        {
            return await _primitives.CreateRecognitionRoot(compositeName, rootLineage);
        }

        public async Task<GraphConnection> CreateRecognitionConnection(string compositeName, GraphConnectionInput graphConnection)
        {
            return await _primitives.CreateRecognitionConnection(compositeName, graphConnection);
        }

        public async Task<GraphObject> CreateRecognitionObject(string compositeName, GraphObjectInput graphObject)
        {
            return await _primitives.CreateRecognitionObject(compositeName, graphObject);
        }

        public async Task<GraphObject> DeleteRecognitionObject(string compositeName, string id)
        {
            return await _primitives.DeleteRecognitionObject(compositeName, id);
        }

        public async Task<GraphObject> DeleteRecognitionRoot(string compositeName, string rootLineage)
        {
            return await _primitives.DeleteRecognitionRoot(compositeName, rootLineage);
        }

        public async Task<GraphObject> UpdateRecognitionObject(string compositeName, GraphObjectUpdate graphObject)
        {
            return await _primitives.UpdateRecognitionObject(compositeName, graphObject);
        }

        public async Task<GraphObject> UpdateVirtualObject(string compositeName, GraphObjectUpdate graphObject, bool merge = false)
        {
            return await _primitives.UpdateVirtualObject(compositeName, graphObject, merge);
        }

        public async Task<List<GraphObject>> NavigateRecognition(string compositeName, string root, string path)
        {
            return await _primitives.NavigateRecognition(compositeName, root, path);
        }

        public async Task<GraphObject> FindRecognition(string compositeName, string root, string path)
        {
            return await _primitives.FindRecognition(compositeName, root, path);
        }

        /// <summary>
        /// return objects in the format expected by cytoscape.
        /// </summary>
        /// <param name="compositeName">finds the model</param>
        /// <param name="lineageFilter">empty for all or matching lineages</param>
        /// <returns>Correctly formatted display objects</returns>
        public async Task<DisplayModel> GetRealDisplayGraph(string compositeName, string lineageFilter)
        {
            return await _primitives.GetRealDisplayGraph(compositeName, lineageFilter);
        }

        public async Task<VRDisplayModel> GetRealVRDisplayGraph(string userId, string graphName, string lineageFilter, string? subjectId)
        {
            return await _primitives.GetRealVRDisplayGraph(userId, graphName, lineageFilter, subjectId);
        }

        public async Task<DisplayModel> GetVirtualDisplayGraph(string compositeName)
        {
            return await _primitives.GetVirtualDisplayGraph(compositeName);
        }

        public async Task<DisplayModel> GetRecognitionDisplayGraph(string compositeName)
        {
            return await _primitives.GetRecognitionDisplayGraph(compositeName);
        }

        public async Task<GraphObject> GetVirtualObjectByLineage(string compositeName, string lineage)
        {
            return await _primitives.GetVirtualObjectByLineage(compositeName, lineage);
        }

        public async Task<GraphObject> GetRecognitionObjectById(string compositeName, string id)
        {
            return await _primitives.GetRecognitionObjectById(compositeName, id);
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
            await _primitives.ClearGraphContent(compositeName);
        }

        public async Task<string> CopyRenameKG(string userId, string name, string newName)
        {
            return await _primitives.CopyRenameKG(userId, name, newName);
        }

        public async Task<GraphAttribute> UpdateRecognitionObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt)
        {
            return await _primitives.UpdateRecognitionObjectAttribute(compositeName, objectId, graphAtt);
        }

        public async Task<GraphAttribute> UpdateVirtualObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt)
        {
            return await _primitives.UpdateVirtualObjectAttribute(compositeName, objectId, graphAtt);
        }

        public async Task<GraphAttribute> DeleteRecognitionObjectAttribute(string compositeName, string objectId, string graphLineage)
        {
            return await _primitives.DeleteRecognitionObjectAttribute(compositeName, objectId, graphLineage);
        }

        public async Task<GraphAttribute> DeleteVirtualObjectAttribute(string compositeName, string objectId, string graphLineage)
        {
            return await _primitives.DeleteVirtualObjectAttribute(compositeName, objectId, graphLineage);
        }

        public async Task<GraphAttribute> UpdateGraphObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt)
        {
            return await _primitives.UpdateGraphObjectAttribute(compositeName, objectId, graphAtt);
        }

        public async Task<GraphAttribute> DeleteGraphObjectAttribute(string compositeName, string objectId, string graphLineage)
        {
            return await _primitives.DeleteGraphObjectAttribute(compositeName, objectId, graphLineage);
        }

        public async Task<List<LineageRecord>> GetLineagesInKG(string compositeName, GraphElementType gtype)
        {
            var lins = await _primitives.GetLineagesInKG(compositeName, gtype);
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
            return lins.OrderBy(a => a.typeWord).ToList();
        }



        public async Task<KnowledgeState> GetKnowledgeState(string userId, string Id, string graphName, bool external = false)
        {
            return await _primitives.GetKnowledgeState(userId, Id, graphName, external);
        }

        public bool FindMetaDisplayStructure(IGraphModel model, GraphObject res, ref DarlVar? pending, List<InteractTestResponse> responses)
        {
            return _metaHandler.FindMetaDisplayStructure(model, res, ref pending, responses);
        }

        public string? FindDisplayAttribute(IGraphModel model, string id)
        {
            return model.FindControlAttribute(id, _metaHandler.CommonLineages["display"]);
        }

        public string? FindCompleteAttribute(IGraphModel model, string id)
        {
            return model.FindControlAttribute(id, _metaHandler.CommonLineages["complete"]);
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
            return await _primitives.GetGraphObjectToString(compositeName, id);
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
            return await _primitives.UpdateKGraph(userId, name, kgupdate);
        }

        public async Task<bool> Exists(string userId, string name)
        {
            return await _primitives.Exists(CreateCompositeName(userId, name));
        }

        public async Task<DisplayModel?> GetRealDisplayGraphWithState(string userId, string graphName, string subjectId)
        {
            return await _primitives.GetRealDisplayGraphWithState(userId, graphName, subjectId);
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




        #region private_methods

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


        private async Task<bool> LineageExists(IGraphModel model, string lineage)
        {
            return await _primitives.LineageExists(model, lineage);
        }



        /// <summary>
        /// Recursively add an object to an ontology and all its parents
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="lineage"></param>
        /// <returns></returns>
        private async Task AddObjectToOntology(IGraphModel model, string lineage, string? child = null)
        {
            if (!await LineageExists(model, lineage))
            {
                //handle composite lineages
                var divider = lineage.IndexOf('+');
                var lin = divider == -1 ? lineage : lineage.Substring(0, divider);
                //Add this lineage element
                if (LineageLibrary.lineages.ContainsKey(lin))
                {
                    if (!await LineageExists(model, lin))
                    {
                        var l = LineageLibrary.lineages[lin];
                        await _primitives.CreateVirtualObject(model, lin, l.typeWord, l.description);
                        if (lin.Contains(","))
                        {
                            //remove last element
                            //call this function recursively with 
                            await AddObjectToOntology(model, lin.Substring(0, lin.LastIndexOf(',')), lin);
                        }
                    }
                }
                if (divider != -1) //composite - we've added the primary part, now add the secondary
                {
                    var sub = lineage.Substring(divider + 1);
                    if (!await LineageExists(model, sub))
                    {
                        if (LineageLibrary.lineages.ContainsKey(sub))
                        {
                            var l = LineageLibrary.lineages[lin];
                            var s = LineageLibrary.lineages[sub];
                            await _primitives.CreateVirtualObject(model, lineage, $"{l.typeWord}/{s.typeWord}", $"{l.description} \n {s.description}");
                            await _primitives.CreateVirtualConnection(model, lineage, lin, "kind_of");
                        }
                    }
                }
            }
            if (child != null)
            {
                //connect the child to this object.
                await _primitives.CreateVirtualConnection(model, child, lineage, "kind_of");
            }
        }

        private async Task<bool> OntologicalCompliance(IGraphModel model, string graphObjectLineage, string propertyLineage)
        {
            //are these concepts connected?
            if (await _primitives.VirtualAssociationExists(model, graphObjectLineage, propertyLineage))
                return true;
            //check for 'has' relationship
            return await OntologicalCompliance(model, _metaHandler.CommonLineages["have"], graphObjectLineage, propertyLineage);
        }

        private async Task<bool> OntologicalCompliance(IGraphModel model, string graphConnectionLineage, string startLineage, string endLineage)
        {
            //Look for a preceding and a following association in this or higher verbs that permits this.
            if (await _primitives.VirtualAssociationExists(model, startLineage, graphConnectionLineage))
            {
                return await _primitives.VirtualAssociationExists(model, endLineage, graphConnectionLineage);
            }
            return false;
        }

        /// <summary>
        /// Add the ontology elements for this object
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="graphObjectLineage"></param>
        /// <param name="propertyLineage"></param>
        /// <returns></returns>
        private async Task BuildOntology(IGraphModel model, string graphObjectLineage, string propertyLineage)
        {
            //add the property to the ontology
            await AddObjectToOntology(model, propertyLineage);
            if (_metaHandler.IsObjectLineage(propertyLineage))
            {
                //add the "has" link to the ontology
                await _primitives.CreateVirtualConnection(model, graphObjectLineage, propertyLineage, "has");
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
        private async Task BuildOntology(IGraphModel model, string graphConnectionLineage, string startLineage, string endLineage)
        {
            if (!await LineageExists(model, graphConnectionLineage))
            {
                await AddObjectToOntology(model, graphConnectionLineage);
            }
            //now add precedes and follows links
            await _primitives.CreateVirtualConnection(model, startLineage, graphConnectionLineage, "precedes");
            await _primitives.CreateVirtualConnection(model, graphConnectionLineage, endLineage, "follows");
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
