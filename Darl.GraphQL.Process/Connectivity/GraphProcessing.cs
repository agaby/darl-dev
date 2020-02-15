using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlLanguage.Processing;
using GraphQL;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GraphProcessing : IGraphProcessing, ILocalStore
    {

        private IConfiguration _config;
        public GremlinServer gremlinServer;
        private TelemetryClient _telemetry;
        private static readonly int maxRetryAttempts = 2;
        private static readonly string biography = "noun:01,4,09,01,3,4,5";
        private static readonly string webpage = "noun:01,4,09,01,3,3,0,8,0";


        public GraphProcessing(IConfiguration config, TelemetryClient telemetry)
        {
            _config = config;
            _telemetry = telemetry;
            var hostname = _config["gremlinHostname"];
            var database = _config["gremlinDatabase"];
            var collection = _config["gremlinCollection"];
            var authKey = _config["gremlinAuthKey"];
            var port = int.Parse(_config["gremlinPort"]);
            gremlinServer = new GremlinServer(hostname, port, enableSsl: true, username: "/dbs/" + database + "/colls/" + collection, password: authKey);
        }


        /// <summary>
        /// Create a graph connection
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="graphConnection">The connection description</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public async Task<GraphConnection> CreateGraphConnection(string userId, GraphConnectionInput graphConnection, bool definitive = false)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphConnection.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphConnection.lineage}.");
                if (!graphConnection.lineage.StartsWith("verb:"))
                    throw new ExecutionError($"Connections should have lineages of type 'verb'. This has a lineage of {graphConnection.lineage}.");
                if (!definitive)//ontological compliance checks
                {
                    var start = await GetGraphObjectById(userId, graphConnection.startId);
                    var end = await GetGraphObjectById(userId, graphConnection.endId);
                    if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, start.lineage, end.lineage))
                    {
                        throw new ExecutionError($"No association exists between {start.lineage}, the verb {graphConnection.lineage} and {end.lineage}\n if you are sure this is correct use the definitive flag in the call.");
                    }
                    foreach (var p in graphConnection.properties)
                    {
                        if (!LineageLibrary.CheckLineage(p.Name))
                            throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                        if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, p.Name))
                        {
                            throw new ExecutionError($"No association exists between {graphConnection.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                }
                var id = Guid.NewGuid().ToString();
                var dict = new Dictionary<string, object> { { "start", graphConnection.startId }, { "end", graphConnection.endId }, { "label", graphConnection.name }, { "weight", graphConnection.weight ?? 1.0 }, { "id", id }, { "userId", userId }, { "lineage", graphConnection.lineage }, { "inferred", graphConnection.inferred ?? false } };
                var script = "g.V(start).addE(label).to(g.V(end)).property('id', id).property('weight', weight).property('userId',userId).property('lineage',lineage).property('inferred',inferred)";
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
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public async Task<GraphObject> CreateGraphObject(string userId, GraphObjectInput graphObject, bool definitive = false)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphObject.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphObject.lineage}.");
                if (!graphObject.lineage.StartsWith("noun:") || graphObject.lineage.StartsWith("proper_noun:"))
                    throw new ExecutionError($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
                if (!definitive)//ontological compliance checks
                {
                    //for each property lineage 
                    if (graphObject.properties != null)
                    {
                        foreach (var p in graphObject.properties)
                        {
                            if (!await OntologicalCompliance(gremlinClient, graphObject.lineage, p.Name))
                            {
                                if (!LineageLibrary.CheckLineage(p.Name))
                                    throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                                throw new ExecutionError($"No association exists between {graphObject.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                            }
                        }
                    }
                }
                var id = Guid.NewGuid().ToString();
                var dict = new Dictionary<string, object> { { "lineage", graphObject.lineage }, { "id", id }, { "name", graphObject.name.Trim().ToLower() }, { "userId", userId }, { "firstname", graphObject.firstname.Trim().ToLower() }, { "secondname", graphObject.secondname.Trim().ToLower() }, { "inferred", graphObject.inferred } };
                var script = "g.addV(lineage).property('id', id).property('name', name).property('lineage',lineage).property('userId',userId).property('firstname',firstname).property('secondname',secondname).property('inferred',inferred)";
                AddCommonElements(graphObject, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return ConvertGraphObject(res.First());
            }
        }

        private void AddCommonElements(GraphElementInput elem, Dictionary<string, object> dict, ref string script)
        {
            if (elem.properties != null)
            {
                int propCount = 0;
                foreach (var p in elem.properties)
                {
                    var propHolder = $"prop{propCount++}";
                    if (!LineageLibrary.CheckLineage(p.Name))
                        throw new ExecutionError($"Malformed property lineage: {p.Name}.");
                    dict.Add(propHolder, p.Value);
                    script += $".property('{p.Name}', {propHolder})";
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
            var res = await SubmitWithRetry(gremlinClient, "g.V('noun:01,2,08,48,24').has('lineage',lineage1).repeat(both()).until(has('lineage', lineage2)).path().limit(1)", new Dictionary<string, object> { { "lineage1", graphObjectLineage }, { "lineage2", propertyLineage } });
            if (res.Count != 0)
                return true;
            //check for 'has' relationship
            return await OntologicalCompliance(gremlinClient, "verb:021", graphObjectLineage, propertyLineage);
        }

        private async Task<bool> OntologicalCompliance(GremlinClient gremlinClient, string graphConnectionLineage, string startLineage, string endLineage)
        {
            //Look for a preceding and a following association in this or higher verbs that permits this.
            var res = await SubmitWithRetry(gremlinClient, "g.V('noun:01,2,08,48,24').has('lineage',lineage1).repeat(both()).until(has('lineage', lineage2)).path().limit(1)", new Dictionary<string, object> { { "lineage1", startLineage }, { "lineage2", graphConnectionLineage } });
            if (res.Count > 0)
            {
                res = await SubmitWithRetry(gremlinClient, "g.V('noun:01,2,08,48,24').has('lineage',lineage1).repeat(both()).until(has('lineage', lineage2)).path().limit(1)", new Dictionary<string, object> { { "lineage1", endLineage }, { "lineage2", graphConnectionLineage } });
                if (res.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The id of the connection to delete</param>
        /// <returns></returns>
        public async Task<GraphConnection> DeleteGraphConnection(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                await SubmitWithRetry(gremlinClient, "g.E(id).has('userId',userId).drop()", new Dictionary<string, object> { { "userId", userId }, { "id", id } });
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
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await SubmitWithRetry(gremlinClient, "g.V(id).has('userId',userId).drop()", new Dictionary<string, object> { { "userId", userId }, { "id", id } });
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
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V().has('id',id).has('userId',userId)", new Dictionary<string, object> { { "id", id }, { "userId", userId } });
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
        /// Get graph objects with an exact name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name</param>
        /// <param name="lineage">The lineage of the object</param>
        /// <returns></returns>
        public async Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var list = new List<GraphObject>();
                try
                {
                    var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).has('name',name).has('userId',userId)", new Dictionary<string, object> { { "name", name.ToLower() }, { "lineage", lineage }, { "userId", userId } });
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
                id = GetValueAsString(r, nameof(GraphObject.id)),
            };
            var props = GetValueOrDefault(r, nameof(GraphObject.properties)) as IReadOnlyDictionary<string, object>;
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
                    case nameof(GraphObject.userId):
                        go.userId = GetPropertyAsString(props, key);
                        break;
                    case nameof(GraphObject.inferred):
                        go.inferred = Convert.ToBoolean(GetPropertyAsString(props, key));
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
                    case nameof(GraphConnection.userId):
                        gc.userId = GetValueAsString(props, key);
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
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                return await FindNearestNameVertex(gremlinClient, lineage, name, similarity);
            }
        }

        /// <summary>
        /// Update a graph connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphConnection">The connection definition - all included fields are updated</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public async Task<GraphConnection> UpdateGraphConnection(string userId, GraphConnectionUpdate graphConnection, bool definitive = false)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!definitive)//ontological compliance checks
                {
                    foreach (var p in graphConnection.properties)
                    {
                        if (!await OntologicalCompliance(gremlinClient, graphConnection.lineage, p.Name))
                        {
                            throw new ExecutionError($"No association exists between {graphConnection.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                }
                var dict = new Dictionary<string, object> { { "id", graphConnection.id }, { "userId", userId } };
                var script = "g.E().property('id', id).property('userId',userId)";
                if (graphConnection.weight != null)
                {
                    dict.Add(nameof(graphConnection.weight), graphConnection.weight);
                    script += $".property('{nameof(graphConnection.weight)}',{nameof(graphConnection.weight)})";
                }
                AddConditionalElements(graphConnection, dict, script);
                AddCommonElements(graphConnection, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return new GraphConnection { id = graphConnection.id, userId = userId };
            }
        }

        /// <summary>
        /// Update a graph object
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphObject">The object definition - all included fields are updated</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public async Task<GraphObject> UpdateGraphObject(string userId, GraphObjectUpdate graphObject, bool definitive = false)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                if (!LineageLibrary.CheckLineage(graphObject.lineage))
                    throw new ExecutionError($"Malformed lineage: {graphObject.lineage}.");
                if (!graphObject.lineage.StartsWith("noun:") || graphObject.lineage.StartsWith("proper_noun:"))
                    throw new ExecutionError($"GraphObjects should have lineages of type 'noun' or 'proper_noun'. This has a lineage of {graphObject.lineage}.");
                if (!definitive)//ontological compliance checks
                {
                    foreach (var p in graphObject.properties)
                    {
                        if (!await OntologicalCompliance(gremlinClient, graphObject.lineage, p.Name))
                        {
                            throw new ExecutionError($"No association exists between {graphObject.lineage} and {p.Name}\n if you are sure this is correct use the definitive flag in the call.");
                        }
                    }
                }
                //required are id and userId. Create scripts and dicts for non-null elements.
                var dict = new Dictionary<string, object> { { "id", graphObject.id }, { "userId", userId } };
                var script = "g.V().has('id', id).has('userId',userId)";
                AddConditionalElement(nameof(graphObject.firstname), graphObject.firstname, dict, script);
                AddConditionalElement(nameof(graphObject.secondname), graphObject.secondname, dict, script);
                AddConditionalElements(graphObject, dict, script);
                AddCommonElements(graphObject, dict, ref script);
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return ConvertGraphObject(res.FirstOrDefault());
            }
        }

        private void AddConditionalElements(GraphElementInput elem, Dictionary<string, object> dict, string script)
        {
            AddConditionalElement(nameof(elem.lineage), elem.lineage, dict, script);
            AddConditionalElement(nameof(elem.name), elem.name, dict, script);
            if (elem.inferred != null)
            {
                dict.Add(nameof(elem.inferred), elem.inferred);
                script += $".property('{nameof(elem.inferred)}',{nameof(elem.inferred)})";
            }
        }

        private void AddConditionalElement(string elemName, string elem, Dictionary<string, object> dict, string script)
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
            return GetValueOrDefault(dictionary, key) as string;
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
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
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
    }
}
