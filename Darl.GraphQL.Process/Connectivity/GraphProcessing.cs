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
            if(!definitive)//ontological compliance checks
            {
                //Look for a preceding and a following association in this or higher verbs that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
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
                if (!definitive)//ontological compliance checks
                {
                    //for each property lineage 
                    //Look for a preceding and a following association in the verb 'has' that permits this.
                    //This can be written as a gremlin query
                    //if no path found throw ExecutionError 
                }
                var id = Guid.NewGuid().ToString();
                var dict = new Dictionary<string, object> { { "lineage", graphObject.lineage }, { "id", id }, { "name", graphObject.name }, { "userId", userId }, {"firstname",graphObject.firstname }, { "secondname", graphObject.secondname }, {"inferred",graphObject.inferred } };
                var script = "g.addV(lineage).property('id', id).property('name', name).property('lineage',lineage).property('userId',userId).property('firstname',firstname).property('secondname',secondname).property('inferred',inferred)";
                foreach (var p in graphObject.properties)
                {
                    dict.Add(p.Name, p.Value);
                    script += $".property('{p.Name}', {p.Name})";
                }
                if(graphObject.existence != null)
                { 
                    int index = 0;
                    foreach(var t in graphObject.existence)
                    {
                        var exName = $"existence{index}";
                        dict.Add(exName, t);
                        script += $".property('existence', {exName})";
                        index++;
                    }
                }
                var res = await SubmitWithRetry(gremlinClient, script, dict);
                return new GraphObject {id = id, userId = userId };
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
            throw new NotImplementedException();
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
                await SubmitWithRetry(gremlinClient, "g.V(id).has('userId',userId).drop()", new Dictionary<string, object> { { "userId", userId }, { "id", id } });
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
                    throw new ExecutionError($"id {id} not found");
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
                catch(Exception ex)
                {
                    throw new ExecutionError("Error in reading from Graph database: ", ex);
                }
                return list;
            }
        }

        private GraphObject ConvertGraphObject(dynamic r)
        {
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
                        go.properties.Add(new StringStringPair(key, GetPropertyAsString(props, key)));
                        break;
                }
            }
            return go;
        }

        /// <summary>
        /// Get graph objects with a fuzzy name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name to fuzzy match</param>
        /// <param name="lineage">The kind of the object</param>
        /// <param name="distance">The max Levenshtein distance of a match</param>
        /// <returns></returns>
        public async Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float distance)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                return await FindNearestNameVertex(gremlinClient, lineage, name, distance);
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
            if (!definitive)//ontological compliance checks
            {
                //
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
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
            if (!definitive)//ontological compliance checks
            {
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
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
            return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        }

        public static string GetPropertyAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var prop = GetValueOrDefault(dictionary, key);
            if(prop != null)
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
                                var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).has('name',name)", new Dictionary<string, object> { { "name", address[1].ToLower() }, { "lineage", address[2] } });
                                if (res.Count == 0)
                                {
                                    var defaultRes = new DarlResult("result", $"We don't have any data on {address[1]}.", DarlResult.DataType.textual);
                                    defaultRes.SetWeight(0.5);
                                    return defaultRes;
                                }
                                return new DarlResult("result", CreateNotesText(res.First()), DarlResult.DataType.textual);
                            }
                        case "links":
                            {
                                if (address.Count != 3)
                                {
                                    throw new Exception("Links call to a graph store must have 3 parameters, 'links', the name and the lineage of the end vertex");
                                }
                                var res = await SubmitWithRetry(gremlinClient, "g.V().has('name',name).outE().inv().hasLabel(TextP.startingWith(lineage)).dedup().properties('name')", new Dictionary<string, object> { { "name", address[1].ToLower() }, { "lineage", address[2] } });
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", $"We don't have any links from {address[1]}.", DarlResult.DataType.textual);
                                }
                                return new DarlResult("result", CreateLinksText(res), DarlResult.DataType.textual);
                            }
                        case "path":
                            {
                                if (address.Count != 3)
                                {
                                    throw new Exception("Path call to a graph store must have 3 parameters, 'path', start name and end name");
                                }
                                var res = await SubmitWithRetry(gremlinClient, "g.V().has('name',name1).repeat(out()).until(has('name', name2)).path().limit(1)", new Dictionary<string, object> { { "name1", address[1].ToLower() }, { "name2", address[2].ToLower() } });
                                if (res.Count == 0)
                                {
                                    return new DarlResult("result", $"We can't find a path between {address[1]} and {address[2]}.", DarlResult.DataType.textual);
                                }
                                return new DarlResult("result", CreatePathText(res), DarlResult.DataType.textual);
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

        private static string CreateNotesText(Dictionary<string, object> node)
        {
            var properties = node["properties"] as Dictionary<string, object>;
            string text = $"# {GetValueAsString(properties, "name")}\n";
            if (!string.IsNullOrEmpty(GetValueAsString(properties, webpage)))
            {
                text += $"# [Link]({GetValueAsString(properties, webpage)})\n";
            }
            if (!string.IsNullOrEmpty(GetValueAsString(properties, biography)))
            {
                text += $"# Notes \n{GetValueAsString(properties, biography)}\n";
            }
            return text;
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a vertex matching name(s) or the closest match with distance <= 2
        /// </summary>
        /// <param name="gremlinClient">The client</param>
        /// <param name="lineage">The lineage parent of the vertices</param>
        /// <param name="name">name or lastname</param>
        /// <param name="firstname">where firstname and secondname are specified.</param>
        /// <returns>The vertex data or null</returns>
        public async Task<List<GraphObject>> FindNearestNameVertex(GremlinClient gremlinClient, string lineage, string name, float minimumDistance = 2,string firstname = "")
        {
            var list = new List<GraphObject>();
            var res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).or(has('name',name),has('firstname',firstname).has('secondname',name))", new Dictionary<string, object> { { "name", name.ToLower() }, { "lineage", lineage }, { "firstname", firstname.ToLower() } });
            if (res.Count == 0)
            {
                res = await SubmitWithRetry(gremlinClient, "g.V().hasLabel(TextP.startingWith(lineage)).or(has('name',TextP.startingWith(name)),has('firstname',TextP.startingWith(firstname)).has('secondname',TextP.startingWith(name)))", new Dictionary<string, object> { { "name", name.ToLower().Substring(0, 1) }, { "lineage", lineage }, { "firstname", firstname.ToLower().Substring(0, 1) } });
                if (res.Count == 0)
                    return list; //no vertices with that lineage with names starting with that/those letter(s)
                foreach (var r in res)
                {//calculate Levenshtein distance.
                    var sought = string.IsNullOrEmpty(firstname) ? name : firstname + " " + name;
                    var found = GetCompositeName(r);
                    var dist = LineageLibrary.Similarity(sought, found);
                    if (dist <= minimumDistance)
                    {
                        list.Add(ConvertGraphObject(r));
                    }
                }
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
