using Chronic;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlLanguage;
using DarlLanguage.Processing;
using GraphQL;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuickGraph.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Darl.GraphQL.Models.Connectivity.IGraphProcessing;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GraphProcessing : IGraphProcessing, ILocalStore, IDisposable
    {

        private IConfiguration _config;
        public GremlinServer gremlinDreamerServer;
        private ILogger _logger;
        private IHttpContextAccessor _context;
        private static readonly int maxRetryAttempts = 2;
        private static readonly string biography = "noun:01,4,09,01,3,4,5";
        private static readonly string webpage = "noun:01,4,09,01,3,3,0,8,0";
        private string hostname;
        private string database;
        private string authKey;
        private int port;




        public GraphProcessing(IConfiguration config, ILogger<GraphProcessing> logger, IHttpContextAccessor context)
        {
            _context = context;
            _config = config;
            _logger = logger;
            if (_config["gremlinLocation"] == "azure")
            {
                hostname = _config["gremlinHostname"];
                database = _config["gremlinDatabase"];
                var collection = _config["gremlinCollection"];
                authKey = _config["gremlinAuthKey"];
                port = int.Parse(_config["gremlinPort"]);
                gremlinDreamerServer = new GremlinServer(hostname, port, enableSsl: true, username: "/dbs/" + database + "/colls/" + collection, password: authKey);
            }
            else if(_config["gremlinLocation"] == "local") //defaults will work
            {
                gremlinDreamerServer = new GremlinServer();
            }
            else
            {
                throw new ExecutionError($"Configuration error. gremlinLocation must be 'azure' or 'local', was {_config["gremlinLocation"]}");
            }
        }

        public async Task<bool> CreateNewGraph(string graphName, string partitionKey)
        {
            if (_config["gremlinLocation"] == "azure")
            {
                ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                };
                using (var client = new DocumentClient(new Uri($"https://{hostname}:{port}/"), authKey, ConnectionPolicy))
                {
                    try
                    {
                        await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(database), new DocumentCollection { Id = graphName, PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { partitionKey } } }, new RequestOptions { OfferThroughput = 400 });
                    }
                    catch (Exception ex)
                    {
                        throw new ExecutionError($"Error in creating a new Graph for user {graphName}", ex);
                    }
                }
            }
            return true;
        }



        /// <summary>
        /// Create a graph connection
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="graphConnection">The connection description</param>
        /// <param name="ontology"> build, check against or ignore ontology </param>
        /// <returns></returns>
        public async Task<GraphConnection> CreateGraphConnection(string userId, GraphConnectionInput graphConnection, OntologyAction ontology = OntologyAction.ignore)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphConnection.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphConnection.lineage}.");
                if (!graphConnection.lineage.StartsWith("verb:"))
                    throw new ExecutionError($"Connections should have lineages of type 'verb'. This has a lineage of {graphConnection.lineage}.");
                if (ontology != OntologyAction.ignore)//ontological compliance checks
                {
                    var startLineage = await GetGraphObjectProperty(userId, graphConnection.startId,"lineage");
                    var endLineage = await GetGraphObjectProperty(userId, graphConnection.endId, "lineage");
                    if (ontology == OntologyAction.check)
                    {
                        if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, startLineage, endLineage))
                        {
                            throw new ExecutionError($"No association exists between {startLineage}, the verb {graphConnection.lineage} and {endLineage}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                    else
                    {
                        await BuildOntology(gremlinClient, graphConnection.lineage, startLineage, endLineage);
                    }
                    foreach (var p in graphConnection.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.Name))
                            throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                        if (ontology == OntologyAction.check)
                        {
                            if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, p.Name))
                            {
                                throw new ExecutionError($"No association exists between {graphConnection.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                        else
                        {
                            await BuildOntology(gremlinClient, graphConnection.lineage, p.Name);
                        }
                    }
                }
                var dict = new Dictionary<string, object> { { "start", graphConnection.startId }, { "end", graphConnection.endId }, { "connlabel", graphConnection.name }, { "weight", graphConnection.weight ?? 1.0 }, { "lineage", graphConnection.lineage }, { "inferred", graphConnection.inferred ?? false } };
                var script = "g.V(start).addE(connlabel).to(g.V(end)).property('weight', weight).property('lineage',lineage).property('inferred',inferred).property('virtual',false)";
                AddCommonElements(graphConnection, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return ConvertGraphConnection(res.FirstOrDefault());
            }
        }

        /// <summary>
        /// Create a graph object
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="graphObject">The object description</param>
        /// <param name="ontology"> build, check against or ignore ontology </param>
        /// <returns></returns>
        public async Task<GraphObject> CreateGraphObject(string userId, GraphObjectInput graphObject, OntologyAction ontology = OntologyAction.ignore)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphObject.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphObject.lineage}.");
                if (!graphObject.lineage.StartsWith("noun:") || graphObject.lineage.StartsWith("proper_noun:"))
                    throw new ExecutionError($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
                if (ontology != OntologyAction.ignore)//ontological compliance checks
                {
                    //for each property lineage 
                    if (graphObject.properties != null)
                    {
                        foreach (var p in graphObject.properties)
                        {
                            if (!LineageLibrary.CheckLineage(p.Name))
                                throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                            if (ontology == OntologyAction.check)
                            {
                                if (!await OntologicalCompliance(gremlinClient, graphObject.lineage, p.Name))
                                {
                                    throw new ExecutionError($"No association exists between {graphObject.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                                }
                            }
                            else
                            {
                               await BuildOntology(gremlinClient, graphObject.lineage, p.Name);
                            }
                        }
                    }
                }
                try
                {
                    var dict = new Dictionary<string, object> { { "lineage", graphObject.lineage }, { "name", graphObject.name.Trim().ToLower() }, { "inferred", graphObject.inferred }, { "virtual", graphObject._virtual ?? false } };
                    var script = "g.addV(name).property('name', name).property('lineage',lineage).property('inferred',inferred).property('virtual',virtual)";
                    AddConditionalElement(nameof(graphObject.firstname), graphObject.firstname, dict, ref script);
                    AddConditionalElement(nameof(graphObject.secondname), graphObject.secondname, dict, ref script);
                    AddConditionalElement(nameof(graphObject.externalId), graphObject.externalId, dict, ref script);
                    AddCommonElements(graphObject, dict, ref script);
                    var res = await SubmitWithRetry(gremlinClient, script, dict);
                    return ConvertGraphObject(res.First());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"CreateGraphObject error writing to Gremlin {ex.Message}");
                    throw ex;
                }
            }
        }

        private void AddCommonElements(GraphElementInput elem, Dictionary<string, object> dict, ref string script)
        {
            if (elem is GraphObjectInput)
            {
                var gelem = elem as GraphObjectInput;
                if (gelem.properties != null)
                {
                    int propCount = 0;
                    foreach (var p in gelem.properties)
                    {
                        var propHolder = $"prop{propCount++}";
                        if (!LineageLibrary.CheckLineage(p.Name))
                            throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                        dict.Add(propHolder, p.Value);
                        script += $".property('{p.Name}', {propHolder})";
                    }
                }
            }
            if (elem.existence != null)
            {
                int index = 0;
                foreach (var t in elem.existence)
                {
                    var exName = $"existence{index}";
                    dict.Add(exName, t);
                    if (elem is GraphObjectInput)
                        script += $".property('existence', {exName})";
                    else
                        script += $".property('{exName}', {exName})";
                    index++;
                }
            }
        }

        private async Task<bool> OntologicalCompliance(GremlinClient gremlinClient, string graphObjectLineage, string propertyLineage)
        {
            //are these concepts connected?
            var res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true).repeat(both()).until(has('lineage', lineage2).has('virtual',true)).path().limit(1)", new Dictionary<string, object> { { "lineage1", graphObjectLineage }, { "lineage2", propertyLineage } });
            if (res.Count != 0)
                return true;
            //check for 'has' relationship
            return await OntologicalCompliance(gremlinClient, "verb:021", graphObjectLineage, propertyLineage);
        }

        private async Task<bool> OntologicalCompliance(GremlinClient gremlinClient, string graphConnectionLineage, string startLineage, string endLineage)
        {
            //Look for a preceding and a following association in this or higher verbs that permits this.
            var res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true).repeat(both()).until(has('lineage', lineage2).has('virtual',true)).path().limit(1)", new Dictionary<string, object> { { "lineage1", startLineage }, { "lineage2", graphConnectionLineage } });
            if (res.Count > 0)
            {
                res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true).repeat(both()).until(has('lineage', lineage2).has('virtual',true)).path().limit(1)", new Dictionary<string, object> { { "lineage1", endLineage }, { "lineage2", graphConnectionLineage } });
                if (res.Count > 0)
                {
                    return true;
                }
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
        private async Task BuildOntology(GremlinClient gremlinClient, string graphObjectLineage, string propertyLineage)
        {
            if(! await LineageExists(gremlinClient, graphObjectLineage))
            {
                await AddObjectToOntology(gremlinClient, graphObjectLineage);
            }
            if (!await LineageExists(gremlinClient, propertyLineage))
            {
                //add the property to the ontology
                await AddObjectToOntology(gremlinClient, propertyLineage);
                if(propertyLineage.StartsWith("noun:"))
                {
                    //add the "has" link to the ontology
                    var dict = new Dictionary<string, object> { { "start", graphObjectLineage }, { "end", propertyLineage }, { "weight", 1.0 } };
                    var script = "g.V().has('lineage',start).addE('has').to(g.V().has('lineage',end)).property('weight', weight).property('virtual',true).property('inferred',false)";
                    await SubmitWithRetry(gremlinClient, script, dict);
                }
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
        private async Task BuildOntology(GremlinClient gremlinClient, string graphConnectionLineage, string startLineage, string endLineage)
        {
            if (!await LineageExists(gremlinClient, graphConnectionLineage))
            {
                await AddObjectToOntology(gremlinClient, graphConnectionLineage);
            }
            //now add precedes and follows links
            var dict = new Dictionary<string, object> { { "start", startLineage }, { "end", graphConnectionLineage }, { "weight", 1.0 }, {"object", endLineage } };
            var script = "g.V().has('lineage',start).addE('precedes').to(g.V().has('lineage',end)).property('weight', weight).property('object',object).property('virtual',true).property('inferred',false)";
            await SubmitWithRetry(gremlinClient, script, dict);
            dict = new Dictionary<string, object> { { "start", graphConnectionLineage }, { "end", endLineage }, { "weight", 1.0 }, { "subject", startLineage } };
            script = "g.V().has('lineage',start).addE('follows').to(g.V().has('lineage',end)).property('weight', weight).property('subject',subject).property('virtual',true).property('inferred',false)";
            await SubmitWithRetry(gremlinClient, script, dict);
        }

        private async Task<bool> LineageExists(GremlinClient gremlinClient, string lineage)
        {
            var res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true)", new Dictionary<string, object> { { "lineage1", lineage }});
            return res.Any();
        }

        /// <summary>
        /// Recursively add an object to an ontology and all its parents
        /// </summary>
        /// <param name="gremlinClient"></param>
        /// <param name="lineage"></param>
        /// <returns></returns>
        private async Task AddObjectToOntology(GremlinClient gremlinClient, string lineage, string child = null)
        {
            if(! await LineageExists(gremlinClient,lineage))
            {
                //Add this lineage element
                if (LineageLibrary.lineages.ContainsKey(lineage))
                {
                    var l = LineageLibrary.lineages[lineage];
                    var dict = new Dictionary<string, object> { { "lineage", lineage }, { "name", l.typeWord }, { "inferred", false }, { "virtual", true }, {"description", l.description} };
                    var script = "g.addV(name).property('name', name).property('lineage',lineage).property('inferred',inferred).property('virtual',virtual).property('description', description)";
                    var res = await SubmitWithRetry(gremlinClient, script, dict);
                    if(lineage.Contains(','))
                    {
                        //remove last element
                        //call this function recursively with 
                        await AddObjectToOntology(gremlinClient, lineage.Substring(0, lineage.LastIndexOf(',')),lineage);
                    }
                }
            }
            if(child != null)
            {
                //connect the child to this object.
                var dict = new Dictionary<string, object> { { "start", child }, { "end", lineage }, { "weight", 1.0 }};
                var script = "g.V().has('lineage',start).addE('kind_of').to(g.V().has('lineage',end)).property('weight', weight).property('virtual',true).property('inferred',false)";
                await SubmitWithRetry(gremlinClient, script, dict);
            }           
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The id of the connection to delete</param>
        /// <returns></returns>
        public async Task<GraphConnection> DeleteGraphConnection(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                await SubmitWithRetry(gremlinClient, "g.E(id).drop()", new Dictionary<string, object> { { "id", id } });
                return null;
            }
        }

        /// <summary>
        /// Delete an object
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The object's id</param>
        /// <returns></returns>
        public async Task<GraphObject> DeleteGraphObject(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await SubmitWithRetry(gremlinClient, "g.V(id).drop()", new Dictionary<string, object> { { "id", id } });
                return null;
            }
        }

        /// <summary>
        /// Get a graph object by the id
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The object's id</param>
        /// <returns>The object</returns>
        public async Task<GraphObject> GetGraphObjectById(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V(objectid)", new Dictionary<string, object> { { "objectid", id }});
                    if (res.Count != 0)
                    {
                        foreach (var r in res)
                        {
                            return ConvertGraphObject(r);
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
            }
        }

        /// <summary>
        /// Get a connection based on the node ids and the lineage
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="lineage"></param>
        /// <returns>The partially filled in connection</returns>
        public async Task<GraphConnection> GetConnectionByIds(string userId, string startId, string endId, string lineage )
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V(startId).outE().has('lineage',lineage).where(otherV().hasId(endId))", new Dictionary<string, object> { { "startId", startId }, { "endId", endId }, { "lineage", lineage } });
                    if (res.Count != 0)
                    {
                        foreach (var r in res)
                        {
                            return new GraphConnection { id = GetValueAsString(r, nameof(GraphObject.id)), startId = startId, endId = endId, name = GetValueAsString(r, "label"), lineage = lineage };                      
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
            }

        }

        public async Task<string> GetGraphObjectProperty(string userId, string id, string property)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V(objectid).properties()", new Dictionary<string, object> { { "objectid", id } });
                    if (res.Count != 0)
                    {
                        foreach (IReadOnlyDictionary<string, object> r in res)
                        {
                            if (r.ContainsKey("label"))
                            {
                                if((string)r["label"] == property)
                                {
                                    return r["value"].ToString();
                                }
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
            }
        }

        public async Task<string> GetGraphConnectionProperty(string userId, string startId, string endId, string lineage, string property)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V(startId).outE().has('lineage',lineage).where(otherV().hasId(endId)).properties()", new Dictionary<string, object> { { "startId", startId }, { "endId", endId }, { "lineage", lineage } });
                    if (res.Count != 0)
                    {
                        foreach (IReadOnlyDictionary<string, object> r in res)
                        {
                            if (r.ContainsKey("key"))
                            {
                                if ((string)r["key"] == property)
                                {
                                    return r["value"].ToString();
                                }
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
            }
        }

        public async Task SetGraphConnectionProperty(string userId, string startId, string endId, string lineage, string property, string value)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V(startId).outE().has('lineage',lineage).where(otherV().hasId(endId)).property(prop,val)", new Dictionary<string, object> { { "startId", startId }, { "endId", endId }, { "lineage", lineage }, { "prop", property }, {"val", value } });
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
            }
        }

        /// <summary>
        /// Get graph objects with an exact name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name</param>
        /// <param name="lineage">The lineage of the object</param>
        /// <returns></returns>
        public async Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var list = new List<GraphObject>();
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).has('name',name)", new Dictionary<string, object> { { "name", name.ToLower() }, { "lineage", lineage } });
                    if (res.Count != 0)
                    {
                        foreach (var r in res)
                        {
                            list.Add(ConvertGraphObject(r));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
                return list;
            }
        }

        private GraphObject ConvertGraphObject(dynamic r)
        {
            if (r == null)
                return null;
            var go = new GraphObject
            {
                id = GetValueAsString(r, nameof(GraphObject.id))
            };
            var props = GetValueOrDefault(r, nameof(GraphObject.properties)) as IReadOnlyDictionary<string, object>;
            if (props != null)
            {
                foreach (var key in props.Keys)
                {
                    switch (key)
                    {
                        case nameof(GraphObject.name):
                            go.name = GetPropertyAsString(props, key);
                            break;
                        case nameof(GraphObject.lineage):
                            go.lineage = GetPropertyAsString(props, key);
                            break;
                        case nameof(GraphObject.firstname):
                            go.firstname = GetPropertyAsString(props, key);
                            break;
                        case nameof(GraphObject.secondname):
                            go.secondname = GetPropertyAsString(props, key);
                            break;
                        case nameof(GraphObject.externalId):
                            go.externalId = GetPropertyAsString(props, key);
                            break;
                        case nameof(GraphObject.inferred):
                            go.inferred = Convert.ToBoolean(GetPropertyAsString(props, key));
                            break;
                        case "virtual":
                            go._virtual = Convert.ToBoolean(GetPropertyAsString(props, key));
                            break;
                        case nameof(GraphObject.existence):
                            go.existence = GetPropertyAsListOfDateTime(props, key);
                            break;
                        default:
                            if (go.properties == null)
                                go.properties = new List<StringStringPair>();
                            go.properties.Add(new StringStringPair(key, GetPropertyAsString(props, key)));
                            break;
                    }
                }
            }
            return go;
        }

        private GraphConnection ConvertGraphConnection(dynamic r)
        {
            if (r == null)
                return null;
            var gc = new GraphConnection
            {
                id = GetValueAsString(r, nameof(GraphConnection.id)),
                startId = GetValueAsString(r, "outV"),
                endId = GetValueAsString(r, "inV"),
                name = GetValueAsString(r, "label")
            };
            var props = GetValueOrDefault(r, nameof(GraphConnection.properties)) as IReadOnlyDictionary<string, object>;
            if (props != null)
            {
                foreach (var key in props.Keys)
                {
                    switch (key)
                    {
                        case nameof(GraphConnection.lineage):
                            gc.lineage = GetValueAsString(props, key);
                            break;
                        case nameof(GraphConnection.weight):
                            {
                                if (double.TryParse(GetValueAsString(props, key), out double weight))
                                    gc.weight = weight;
                                else
                                    gc.weight = 1.0;
                            }
                            break;
                        case nameof(GraphConnection.inferred):
                            gc.inferred = Convert.ToBoolean(GetValueAsString(props, key));
                            break;
                        case "existence0":
                        case "existence1":
                        case "existence2":
                        case "existence3":
                            if (gc.existence == null)
                                gc.existence = new List<DateTime>();
                            gc.existence.Add((DateTime)GetValueOrDefault(props, key));
                            break;
                        default:
                            if (gc.properties == null)
                                gc.properties = new List<StringStringPair>();
                            gc.properties.Add(new StringStringPair(key, GetPropertyAsString(props, key)));
                            break;
                    }
                }
            }
            return gc;
        }

        /// <summary>
        /// Get graph objects with a fuzzy name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name to fuzzy match</param>
        /// <param name="lineage">The kind of the object</param>
        /// <param name="similarity">The minimum similarity of a match</param>
        /// <returns></returns>
        public async Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float similarity)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                return await FindNearestNameVertex(gremlinClient, lineage, name, similarity);
            }
        }

        /// <summary>
        /// Update a graph connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphConnection">The connection definition - all included fields are updated</param>
        /// <param name="ontology"> build, check against or ignore ontology </param>
        /// <returns></returns>
        public async Task<GraphConnection> UpdateGraphConnection(string userId, GraphConnectionUpdate graphConnection, OntologyAction ontology = OntologyAction.ignore)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (ontology != OntologyAction.ignore)//ontological compliance checks
                {
                    if (graphConnection.properties != null)
                    {
                        foreach (var p in graphConnection.properties)
                        {
                            if (ontology == OntologyAction.check)
                            {
                                if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, p.Name))
                                {
                                    throw new ExecutionError($"No association exists between {graphConnection.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                                }
                            }
                            else
                            {
                                await BuildOntology(gremlinClient, graphConnection.lineage, p.Name);
                            }
                        }
                    }
                }
                var dict = new Dictionary<string, object> { { "id", graphConnection.id } };
                var script = "g.E().property('id', id)";
                if (graphConnection.weight != null)
                {
                    dict.Add(nameof(graphConnection.weight), graphConnection.weight);
                    script += $".property('{nameof(graphConnection.weight)}',{nameof(graphConnection.weight)})";
                }
                AddConditionalElements(graphConnection, dict, ref script);
                AddCommonElements(graphConnection, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return new GraphConnection { id = graphConnection.id };
            }
        }

        /// <summary>
        /// Update a graph object
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphObject">The object definition - all included fields are updated</param>
        /// <param name="ontology"> build, check against or ignore ontology </param>
        /// <returns></returns>
        public async Task<GraphObject> UpdateGraphObject(string userId, GraphObjectUpdate graphObject, OntologyAction ontology = OntologyAction.ignore)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphObject.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphObject.lineage}.");
                if (!graphObject.lineage.StartsWith("noun:") || graphObject.lineage.StartsWith("proper_noun:"))
                    throw new ExecutionError($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
                if (ontology != OntologyAction.ignore)//ontological compliance checks
                {
                    if (graphObject.properties != null)
                    {
                        foreach (var p in graphObject.properties)
                        {
                            if (ontology == OntologyAction.check)
                            {
                                if (!await OntologicalCompliance(gremlinClient, graphObject.lineage, p.Name))
                                {
                                    throw new ExecutionError($"No association exists between {graphObject.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                                }
                            }
                            else
                            {
                                await BuildOntology(gremlinClient, graphObject.lineage, p.Name);
                            }
                        }
                    }
                }
                //required is id . Create scripts and dicts for non-null elements.
                var dict = new Dictionary<string, object> { { "id", graphObject.id } };
                var script = "g.V().has(id, id)";
                AddConditionalElement(nameof(graphObject.firstname), graphObject.firstname, dict, ref script);
                AddConditionalElement(nameof(graphObject.secondname), graphObject.secondname, dict, ref script);
                AddConditionalElement(nameof(graphObject.externalId), graphObject.externalId, dict, ref script);
                AddConditionalElements(graphObject, dict, ref script);
                AddCommonElements(graphObject, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return ConvertGraphObject(res.FirstOrDefault());
            }
        }

        private void AddConditionalElements(GraphElementInput elem, Dictionary<string, object> dict, ref string script)
        {
            AddConditionalElement(nameof(elem.lineage), elem.lineage, dict, ref script);
            AddConditionalElement(nameof(elem.name), elem.name, dict, ref script);
            if (elem.inferred != null)
            {
                dict.Add(nameof(elem.inferred), elem.inferred);
                script += $".property('{nameof(elem.inferred)}',{nameof(elem.inferred)})";
            }
            if (elem._virtual != null)
            {
                dict.Add("virtual", elem._virtual);
                script += $".property('virtual',virtual)";
            }
        }

        private void AddConditionalElement(string elemName, string elem, Dictionary<string, object> dict, ref string script)
        {
            if (!string.IsNullOrEmpty(elem))
            {
                dict.Add(elemName, elem);
                script += $".property('{elemName}',{elemName})";
            }
        }

        public static async Task<ResultSet<dynamic>> SubmitWithRetry(GremlinClient gc, string script, Dictionary<string, object> dict)
        {
            var attempts = 0;
            var exceptionText = String.Empty;
            do
            {
                try
                {
                    var res = await gc.SubmitAsync<dynamic>(script, dict);
                    return res;
                }
                catch (ResponseException e)
                {
                    attempts++;
                    var backofMs = GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms");
                    if (!string.IsNullOrEmpty(backofMs))
                    {
                        if (int.TryParse(backofMs, out int delay))
                            Task.Delay(delay).Wait();
                        else
                            Task.Delay(100).Wait();
                    }
                    exceptionText = e.ToString();
                }
            }
            while (attempts < maxRetryAttempts);
            throw new ExecutionError($"{attempts} retries failed accessing the Gremlin database: {exceptionText}");
        }
        public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var val =  GetValueOrDefault(dictionary, key);
            if (val == null)
                return null;
            return val.ToString();
        }

        public static string GetPropertyAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var prop = GetValueOrDefault(dictionary, key);
            if (prop != null)
            {
                foreach (var sp in prop as IEnumerable)
                {
                    return GetValueOrDefault(sp as IReadOnlyDictionary<string, object>, "value") as string;
                }
            }
            return string.Empty;
        }

        public static List<DateTime> GetPropertyAsListOfDateTime(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var list = new List<DateTime>();
            var prop = GetValueOrDefault(dictionary, key);
            if (prop != null)
            {
                foreach (var sp in prop as IEnumerable)
                {
                    list.Add((DateTime)GetValueOrDefault(sp as IReadOnlyDictionary<string, object>, "value"));
                }
            }
            return list;
        }

        public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
                //item is itself a dictionary containing id and value
            }
            return null;
        }

        public async Task<DarlResult> ReadAsync(List<string> address)
        {           
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                //add name lookup for fuzzy match
                if (address.Count > 1)
                {
                    switch (address[0].ToLower())
                    {
                        case "text":
                            {
                                //3 parameters
                                if (address.Count != 3)
                                {
                                    throw new Exception("Text call to a graph store must have 3 parameters, 'text', the name and the lineage");
                                }
                                var res = await FindNearestNameVertex(gremlinClient, address[2].Trim().ToLower(), address[1].Trim().ToLower());
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                return new DarlResult("result", CreateNotesText(res.First()), DarlResult.DataType.textual);
                            }
                        case "links":
                            {
                                if (address.Count != 4)
                                {
                                    throw new Exception("Links call to a graph store must have 4 parameters, 'links', the name, lineage of the start vertex and the lineage of the end vertex");
                                }
                                var lookup = await FindNearestNameVertex(gremlinClient, address[2].Trim().ToLower(), address[1].Trim().ToLower());
                                if (lookup.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage1)).has('name',name).outE().inv().hasLabel(TextP.startingWith(lineage2)).dedup().properties('name')", new Dictionary<string, object> { { "name", lookup.First().name }, { "lineage1", address[2] }, { "lineage2", address[3] } });
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                return new DarlResult("result", CreateLinksText(res), DarlResult.DataType.textual);
                            }
                        case "path":
                            {
                                if (address.Count != 5)
                                {
                                    throw new Exception("Path call to a graph store must have 5 parameters, 'path', start name, end name, start lineage and end lineage");
                                }
                                var start = await FindNearestNameVertex(gremlinClient, address[3].Trim().ToLower(), address[1].Trim().ToLower());
                                var end = await FindNearestNameVertex(gremlinClient, address[4].Trim().ToLower(), address[2].Trim().ToLower());
                                if (start.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                if (end.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                var res = await SubmitWithRetry(gremlinClient, "g.V().has('id',id1).repeat(out()).until(has('id', id2)).path().limit(1)", new Dictionary<string, object> { { "id1", start.First().id }, { "id2", end.First().id } });
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                return new DarlResult("result", CreatePathText(res), DarlResult.DataType.textual);
                            }
                        case "attribute":
                            {
                                if (address.Count != 4)
                                {
                                    throw new Exception("Attribute call to a graph store must have 4 parameters, 'text', the name, the object lineage and the attribute lineage");
                                }
                                var res = await FindNearestNameVertex(gremlinClient, address[2].Trim().ToLower(), address[1].Trim().ToLower());
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", 0.0, true);
                                }
                                var prop = res.First().properties.Where(a => a.Name.StartsWith(address[3])).FirstOrDefault();
                                if (prop != null)
                                    return new DarlResult("result", prop.Value, DarlResult.DataType.textual);
                                else
                                    return new DarlResult("result", 0.0, true);
                            }
                    }
                }
            }
            return new DarlResult(0.0, true);
        }

        private string CreatePathText(ResultSet<dynamic> res)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Path:");
            var p = res.First();
            var x = ((Dictionary<string, object>)p)["objects"];
            foreach (var o in x as IEnumerable)
            {
                var prop = ((Dictionary<string, object>)o)["properties"];
                var subProp = ((Dictionary<string, object>)prop)["name"];
                foreach (var sp in subProp as IEnumerable)
                {
                    sb.AppendLine($"+ { ((Dictionary<string, object>)sp)["value"].ToString()}");
                }
            }
            return sb.ToString();
        }

        private string CreateLinksText(ResultSet<dynamic> res)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Connections:");
            foreach (var r in res)
            {
                sb.AppendLine($"+ {((Dictionary<string, object>)r)["value"].ToString()}");
            }
            return sb.ToString();
        }

        private static string CreateNotesText(GraphObject node)
        {
            string text = $"# {node.name}\n";
            foreach (var p in node.properties)
            {
                if (p.Name == webpage)
                    text += $"# [Link]({p.Value})\n";
                else if (p.Name == biography)
                    text += $"# Notes \n{p.Value}\n";
            }
            return text;
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a vertex matching name(s) or the closest match with similarity > 0.7
        /// </summary>
        /// <param name="gremlinClient">The client</param>
        /// <param name="lineage">The lineage parent of the vertices</param>
        /// <param name="name">name or lastname</param>
        /// <param name="firstname">where firstname and secondname are specified.</param>
        /// <returns>The vertex data or null</returns>
        public async Task<List<GraphObject>> FindNearestNameVertex(GremlinClient gremlinClient, string lineage, string name, float minimumSimilarity = 0.7f, string firstname = "")
        {
            var list = new List<GraphObject>();
            var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).or(has('name',name),has('firstname',firstname).has('secondname',name))", new Dictionary<string, object> { { "name", name.ToLower() }, { "lineage", lineage }, { "firstname", firstname.ToLower() } });
            if (res.Count == 0)
            {
                res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).or(has('name',TextP.startingWith(name)),has('firstname',TextP.startingWith(firstname)).has('secondname',TextP.startingWith(name)))", new Dictionary<string, object> { { "name", name.ToLower().Substring(0, 1) }, { "lineage", lineage }, { "firstname", firstname.Length > 1 ? firstname.ToLower().Substring(0, 1) : "" } });
                if (res.Count == 0)
                    return list; //no vertices with that lineage with names starting with that/those letter(s)
                var sought = string.IsNullOrEmpty(firstname) ? name : firstname + " " + name;
                var distances = new List<(GraphObject, double)>();
                foreach (var r in res)
                {//calculate Levenshtein distance.
                    var found = GetCompositeName(r);
                    var dist = LineageLibrary.Similarity(sought, found);
                    if (dist >= minimumSimilarity)
                    {
                        var go = ConvertGraphObject(r);
                        distances.Add((go, dist));
                    }
                }
                distances.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                list = distances.Select(a => a.Item1).ToList();
            }
            else
            {
                foreach (var r in res)
                {
                    list.Add(ConvertGraphObject(r));
                }
            }
            return list;
        }

        private static string GetCompositeName(Dictionary<string, object> source)
        {
            if (source.ContainsKey("properties"))
            {
                var props = source["properties"] as Dictionary<string, object>;
                if (props.ContainsKey("name"))
                {
                    var val = ((IEnumerable<dynamic>)props["name"]).First() as Dictionary<string, object>;
                    return val["value"] as string;
                }
                else if (props.ContainsKey("firstname") && props.ContainsKey("secondname"))
                {
                    var val1 = ((IEnumerable<dynamic>)props["firstname"]).First() as Dictionary<string, object>;
                    var val2 = ((IEnumerable<dynamic>)props["secondname"]).First() as Dictionary<string, object>;
                    return (string)val1["value"] + " " + (string)val2["value"];
                }
            }
            return string.Empty;
        }

        public async Task<string> gremlinPassThrough(string userId, string query)
        {
            using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await SubmitWithRetry(gremlinClient, query, new Dictionary<string, object> { });
                return JsonConvert.SerializeObject(res);
            }
        }
        public async Task<InferenceRecord> InferPath(GraphObjectInput start, GraphObjectInput end, string userId, string targetOutput)
        {
            var ir = new InferenceRecord();
            try
            {
                var startObjects = await GetGraphObjects(userId, start.name, start.lineage);
                if (startObjects.Count < 1)
                {
                    throw new Exception($"A matching object to start does not exist in the Knowledge graph.");
                }
                if (startObjects.Count > 1)
                {
                    throw new Exception($"Multiple matching objects to start exist in the Knowledge graph.");
                }
                var startObject = startObjects.First();
                var startId = startObject.id;

                var endObjects = await GetGraphObjects(userId, end.name, end.lineage);
                if (endObjects.Count < 1)
                {
                    throw new Exception($"A matching object to end does not exist in the Knowledge graph.");
                }
                if (endObjects.Count > 1)
                {
                    throw new Exception($"Multiple matching objects to end exist in the Knowledge graph.");
                }
                var endObject = endObjects.First();
                var endId = endObject.id;

                var vertices = new Dictionary<string, GraphObject>();
                var edges = new Dictionary<string, List<GraphConnection>>();
                var inputs = new HashSet<string>();
                var outputs = new HashSet<string>();
                var rules = new HashSet<string>();
                var lineages = new HashSet<string>();
                var all = new Dictionary<string, string>();
                var any = new Dictionary<string, string>();
                using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
                {
                    //get all the paths between the start and end objects and then collect the vertices that occur in those paths.
                    var query = $"g.V().has('id',id1).repeat(outE().inV()).until(has('id', id2)).path().unfold().dedup()";
                    var res1 = await SubmitWithRetry(gremlinClient, query, new Dictionary<string, object> { { "id1", startId }, { "id2", endId } });
                    //results contain all edges and all vertices
                    if (res1.Count != 0)
                    {
                        foreach (var r in res1)
                        {
                            if (IsVertex(r))
                            {
                                GraphObject go = ConvertGraphObject(r);
                                vertices.Add(go.id, go);
                                lineages.Add(go.lineage);
                            }
                            else
                            {
                                GraphConnection conn = ConvertGraphConnection(r);
                                if (!edges.ContainsKey(conn.endId))
                                    edges.Add(conn.endId, new List<GraphConnection>());
                                edges[conn.endId].Add(conn);
                            }
                        }
                    }
                    foreach (var l in lineages)
                    {
                        var lquery = "g.V().has('lineage',lin1).has('virtual',true)";
                        var lres = await SubmitWithRetry(gremlinClient, lquery, new Dictionary<string, object> { { "lin1", l } });
                        if (lres.Count > 0)
                        {
                            GraphObject gv = ConvertGraphObject(lres.First());
                            if (gv.properties != null)
                            {
                                if (gv.properties.Any(a => a.Name == "all"))
                                {
                                    all.Add(l, gv.properties.First(a => a.Name == "all").Value);
                                }
                                if (gv.properties.Any(a => a.Name == "any"))
                                {
                                    any.Add(l, gv.properties.First(a => a.Name == "any").Value);
                                }
                            }
                        }
                    }
                }
                var rootName = ConvertToDarl(endObject, inputs, outputs, rules, edges, vertices, all, any);
                var rulesetName = "compliance";
                var darl = new StringBuilder($"ruleset {rulesetName}\n{{\n");
                darl.AppendLine(string.Join('\n', inputs));
                darl.AppendLine();
                darl.AppendLine(string.Join('\n', outputs));
                darl.AppendLine();
                darl.AppendLine(string.Join('\n', rules));
                darl.AppendLine("\n}");
                var runtime = new DarlRunTime();
                var darlSource = darl.ToString();
                var tree = runtime.CreateTree(darlSource);
                //assume the properties are name - degree of truth pairs
                if (startObject.properties == null)
                    startObject.properties = start.properties ?? new List<StringStringPair>();
                else
                {
                    if(start.properties != null)
                        startObject.properties.AddRange(start.properties);
                }
                var values = new List<DarlResult>();
                if (startObject.properties != null)
                {
                    foreach (var p in startObject.properties)
                    {
                        if (double.TryParse(p.Value, out double dval))
                        {
                            if (dval >= 0.5)
                                values.Add(new DarlResult(p.Name, "true", DarlResult.DataType.categorical, dval));
                            else
                                values.Add(new DarlResult(p.Name, "false", DarlResult.DataType.categorical, 1.0 - dval));
                        }
                    }
                }
                var outs = runtime.GetOutputNames(tree);
                var res = await runtime.Evaluate(tree, values);
                foreach (var r in res)
                {
                    if (!r.IsUnknown())
                    {
                        var degreeOfTruth = ((string)r.Value == "true" ? r.GetWeight() : 1.0 - r.GetWeight());
                        var newProp = new StringStringPair(r.name, degreeOfTruth.ToString());
                        if (startObject.properties.Any(a => a.Name == r.name))
                        {
                            var existing = startObject.properties.First(a => a.Name == r.name);
                            startObject.properties.Remove(existing);
                            startObject.properties.Add(newProp);
                        }
                        else if (!r.name.StartsWith(rulesetName)) //ignore local values
                        {
                            startObject.properties.Add(newProp);
                        }
                        if (r.name == targetOutput)
                        {
                            ir.unknown = false;
                            ir.confidence = degreeOfTruth;
                        }
                    }
                }
                var saliences = runtime.CalculateSaliences(res, tree);
                var sortedSaliences = saliences.OrderByDescending(a => a.Value).ThenBy(a => a.Key).ToList();
                ir.source = startObject;
                ir.recommendations = new List<StringStringPair>();
                //update confidence and unknown status based on target result.
                foreach (var s in sortedSaliences)
                {
                    ir.recommendations.Add(new StringStringPair(s.Key, s.Value.ToString()));
                }
                _logger.LogWarning($"{nameof(InferPath)}: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in InferPath", ex);
                throw new ExecutionError($"Error in InferPath: {ex.Message}", ex);
            }
            return ir;
        }
        public string ConvertToDarl(GraphObject go, HashSet<string> inputs, HashSet<string> outputs, HashSet<string> rules, Dictionary<string, List<GraphConnection>> edges, Dictionary<string, GraphObject> vertices, Dictionary<string, string> all, Dictionary<string, string> any)
        {
            var fullOutName = $"{ConvertNameToDarlName(go.name)}_inferred";
            var fullInName = $"{ConvertNameToDarlName(go.name)}_achieved";
            bool isLeaf = true;
            if (all.ContainsKey(go.lineage))
            {
                var lineage = all[go.lineage];
                string rule = "if ";
                List<string> operands = new List<string>();
                foreach (var c in edges[go.id])
                {
                    var start = vertices[c.startId];
                    if (start.lineage == lineage)
                        operands.Add($" {ConvertToDarl(start, inputs, outputs, rules, edges, vertices, all, any)} is true ");
                }
                rule += string.Join("and", operands);
                rule += $" then {fullOutName} will be true;";
                if (operands.Any())
                {
                    rules.Add(rule);
                    isLeaf = false;
                }
            }
            if (any.ContainsKey(go.lineage))
            {
                var lineage = any[go.lineage];
                string rule = "if ";
                List<string> operands = new List<string>();
                foreach (var c in edges[go.id])
                {
                    var start = vertices[c.startId];
                    if (start.lineage == lineage)
                        operands.Add($" {ConvertToDarl(start, inputs, outputs, rules, edges, vertices, all, any)} is true ");
                }
                rule += string.Join("or", operands);
                rule += $" then {fullOutName} will be true;";
                if (operands.Any())
                {
                    rules.Add(rule);
                    isLeaf = false;
                }
            }
            if (isLeaf)
            {
                inputs.Add($"input categorical {fullInName} {{true,false}};");
                return fullInName;
            }
            else
            {
                if (!go.inferred)
                {
                    rules.Add($"if {fullInName} is true then {fullOutName} will be true;");
                    inputs.Add($"input categorical {fullInName} {{true,false}};");
                }
                outputs.Add($"output categorical {fullOutName} {{true,false}};");
                return fullOutName;
            }
        }

        /// <summary>
        /// Update connections and weights within a knowledge graph based on associations
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="values">A list of specifications, one of which indexes the associations.</param>
        /// <returns>A report on what has been done.</returns>
        /// <remarks>values contains a list of KGTrainingValue objects, each of which identifies a group of objects by lineage, and the property that contains the text to be matched.
        /// Also contained in each is a set of example texts. One KGTrainingValue is set to be the index by a flag. The nearest node to each example text is found and the match, and index of the node recorded.
        /// This set is returned for 
        /// </remarks>
        public async Task<string> UpdateKGFromAssociationData(string userId, List<KGTrainingValue> values, string connectionLineage, string connectionName)
        {
            //check all arrays are the same length
            int arrayLength = -1;
            bool indexFound = false;
            if(values.Count < 2)
            {
                throw new ExecutionError($"Two or more values in the list expected: one index and at least one associated value");
            }
            foreach (var v in values)
            {
                if (v.values.Count != arrayLength)
                {
                    if (arrayLength == -1)
                    {
                        arrayLength = v.values.Count;
                    }
                    else
                    {
                        throw new ExecutionError($"The first value contains {arrayLength} values, but one of the values arrays has a different length.");
                    }
                }
                if(v.index)
                {
                    if(indexFound)
                    {
                        throw new ExecutionError($"More than one value set as index");
                    }
                    indexFound = true;
                }
            }
            if (!indexFound)
            {
                throw new ExecutionError($"No value set as index");
            }

            //process each set of nodes individually
            var aggregatedResults = new List<KGMatchResult>();
            var lockObject = new object();
            Parallel.ForEach(values,
                () => new KGMatchResult(),
                (match, loopState, results) =>
                {
                    //get all the labels for objects in the KG with one of those lineages
                    results.valueProperty.AddRange(match.valueProperty);
                    var labels = new List<StringStringPair>();
                    using (var gremlinClient = new GremlinClient(gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
                    {
                        foreach (var l in match.valueLineages)
                        {
                            try
                            {
                                var dict = new Dictionary<string, object> { { "lineage", l }};
                                string extension = string.Empty;
                                string inclusion = string.Empty;
                                foreach (var vp in match.valueProperty)
                                {
                                    extension += $".by('{vp}')";
                                    inclusion += $"'{vp}',";
                                }
                                inclusion = inclusion.Substring(0, inclusion.Length - 1); //trim final','
                                var script = $"g.V().has('lineage',TextP.startingWith(lineage)).has('virtual',false).project('id',{inclusion}).by(id){extension}";
                                var res = SubmitWithRetry(gremlinClient, script, dict).Result;
                                if (res.Count != 0)
                                {
                                    foreach (IReadOnlyDictionary<string, object> r in res)
                                    {
                                        var sb = new StringBuilder();
                                        foreach (var vp in match.valueProperty)
                                        {
                                            if (r.ContainsKey(vp))
                                            {
                                                sb.AppendFormat("{0} ", r[vp].ToString());
                                            }
                                        }
                                        labels.Add(new StringStringPair(r["id"].ToString(), sb.ToString()));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new ExecutionError("Internal error in Association processing", ex);
                            }
                        }
                        //build the match tree
                        match.graph = new MatchGraph();//remove for parallel
                        match.graph.CreateTree(labels);
                        //Match up the values
                        results.index = match.index;
                        //make this parallel too.
                        //preset the result with null pointers
                        for(int n = 0; n < match.values.Count; n++)
                        {
                            results.results.Add(null);
                        }
                        //use a parallel for
                       for (int n = 0; n < match.values.Count; n++)
                       {
                            var s = match.values[n];
                            var resList = new List<MatchResult>();
                            foreach(var t in s)
                            {
                                resList.Add(match.graph.Find(t));
                            }
                            results.results[n] = resList;
                        }
/*                        Parallel.For(0, match.values.Count,
                            () => new FindLoopRecord(),
                            (n,loop,resList) => {
                                var graph = new MatchGraph();
                                graph.CreateTree(labels);
                                var s = match.values[n];
                                foreach (var t in s)
                                {
                                    resList.res.Add(graph.Find(t));
                                    resList.index = n;
                                }
                                return resList;
                            },
                            (resList) =>
                            {
                                lock (lockObject) { results.results[resList.index] = resList.res; }
                            }
                            );*/
                    }
                    return results;
                },
                (results) => { lock (lockObject) { aggregatedResults.Add(results); } }
                ); 
            var report = new StringBuilder();
            report.AppendLine($"Association learning run at {DateTime.UtcNow} UTC.");
            report.AppendLine($"{values.Count - 1} associations possible with {values[0].values.Count} examples.");
            // now modify the knowledge graph
            //first find the index record
            var index = aggregatedResults.Where(a => a.index).FirstOrDefault();
            aggregatedResults.Remove(index); //take it out of the list
            int resultCount = 0;
            for (int n = 0; n < index.results.Count; n++)
            {
                if (index.results[n] != null && index.results[n].Any() && index.results[n][0] != null)
                {
                    resultCount++;
                    var matchedText = await GetGraphObjectProperty(userId, index.results[n][0].index, "name");
                    report.AppendLine($"index text: '{index.results[n][0].sourceText}' matches text '{matchedText}' associated with node index { index.results[n][0].index} weight: {index.results[n][0].confidence} ");
                    foreach(var v in aggregatedResults)
                    {
                        if(v.results[n] != null)
                        {
                            foreach(var a in v.results[n])
                            {
                                if (a != null)
                                {
                                    var matchedsubText = await GetGraphObjectProperty(userId, a.index, v.valueProperty[0]);
                                    report.AppendLine($"\t Has an association with text: '{a.sourceText}', original text '{matchedsubText}',  associated with node index { a.index} weight: {a.confidence} ");
                                    //see if connection exists, if so, add weight, if not add connection with weight
                                    var conn = await GetConnectionByIds(userId, index.results[n][0].index, a.index, connectionLineage);
                                    if(conn == null)
                                    {
                                        await CreateGraphConnection(userId, new GraphConnectionInput { startId = index.results[n][0].index, endId = a.index, _virtual = false, inferred = false, name = connectionName, lineage = connectionLineage, weight = a.confidence }, OntologyAction.build);
                                    }
                                    else
                                    {
                                        var weight = await GetGraphConnectionProperty(userId, index.results[n][0].index, a.index, connectionLineage, "weight");
                                        if(!string.IsNullOrEmpty(weight))
                                        {
                                            var newWeight = double.Parse(weight) + a.confidence;
                                            await SetGraphConnectionProperty(userId, index.results[n][0].index, a.index, connectionLineage, "weight", newWeight.ToString());
                                        }
                                    }
                                    //Hierarchical update here
                                }
                            }
                        }
                    }
                }
            }
            return report.ToString();
        }

        private bool IsVertex(dynamic r)
        {
            return GetValueAsString(r, "type") == "vertex";
        }

        static string ConvertNameToDarlName(string name)
        {
            return name.Replace(' ', '_').Replace("&", "and");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        internal class FindLoopRecord
        {
            internal int index { get; set; }

            internal List<MatchResult> res { get; set; } = new List<MatchResult>();

        }

    }
}
