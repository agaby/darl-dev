/// <summary>
/// </summary>

﻿using Darl.Lineage;
using Darl.Thinkbase;
using GraphQL;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl_standard.Darl.Thinkbase
{
    public class GremlinGraphPrimitives : IGraphPrimitives
    {
        private static readonly int maxRetryAttempts = 2;


        public async Task<GraphConnection> CreateConnection(GraphConnectionInput graphConnection)
        {
            var dict = new Dictionary<string, object> { { "start", graphConnection.startId }, { "end", graphConnection.endId }, { "connlabel", graphConnection.name }, { "weight", graphConnection.weight ?? 1.0 }, { "lineage", graphConnection.lineage }, { "partition", GraphElement.partitionType.reality.ToString() } };
            var script = "g.V(start).addE(connlabel).to(g.V(end)).property('weight', weight).property('lineage',lineage).property('inferred',false).property('virtual',false).property('partition',partition)";
            AddCommonElements(graphConnection, dict, ref script);
            var res = await SubmitWithRetry(gremlinClient, script, dict);
            return ConvertGraphConnection(res.FirstOrDefault());
        }

        public async Task<bool> LineageExists(string lineage)
        {
            var res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true).has('partition','dreaming')", new Dictionary<string, object> { { "lineage1", lineage } });
            return res.Any();
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
                        case "virtual":
                            gc._virtual = Convert.ToBoolean(GetValueAsString(props, key));
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
                catch (Exception ex)
                {

                }
            }
            while (attempts < maxRetryAttempts);
            throw new ExecutionError($"{attempts} retries failed accessing the Gremlin database: {exceptionText}");
        }
        public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var val = GetValueOrDefault(dictionary, key);
            if (val == null)
                return null;
            return val.ToString();
        }

        public static string GetPropertyAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            var prop = GetValueOrDefault(dictionary, key);
            if (prop is string)
                return prop as string;
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

        public async Task CreateVirtualObject(string lineage, string typeword, string description)
        {
            var dict = new Dictionary<string, object> { { "lineage", lineage }, { "name", l.typeWord }, { "inferred", false }, { "virtual", true }, { "description", l.description }, { "partition", GraphElement.partitionType.dreaming.ToString() } };
            var script = "g.addV(name).property('name', name).property('lineage',lineage).property('inferred',inferred).property('virtual',virtual).property('description', description).property('partition',partition)";
            var res = await SubmitWithRetry(gremlinClient, script, dict);
        }

        public async Task CreateVirtualConnection(string child, string lineage, string connectionLabel)
        {
            var dict = new Dictionary<string, object> { { "start", child }, { "end", lineage }, { "weight", 1.0 }, { "partition", GraphElement.partitionType.dreaming.ToString() }, { "clab", connectionLabel } };
            var script = "g.V().has('lineage',start).has('virtual',true).has('partition',partition).addE(clab).to(g.V().has('lineage',end).has('virtual',true).has('partition',partition)).property('weight', weight).property('virtual',true).property('inferred',false).property('partition',partition)";
            await SubmitWithRetry(gremlinClient, script, dict);
        }

        public async Task<bool> VirtualAssociationExists(string lineage1, string lineage2)
        {
            var res = await SubmitWithRetry(gremlinClient, "g.V().has('lineage',lineage1).has('virtual',true).repeat(both()).until(has('lineage', lineage2).has('virtual',true)).path().limit(1)", new Dictionary<string, object> { { "lineage1", lineage1 }, { "lineage2", lineage2 } });
            return res.Count != 0;
        }

        public Task<GraphObject> CreateObject(GraphObjectInput graphObject)
        {
            try
            {
                var dict = new Dictionary<string, object> { { "lineage", graphObject.lineage }, { "name", graphObject.name.Trim().ToLower() }, { "virtual", false }, { "partition", GraphElement.partitionType.reality.ToString() } };
                var script = "g.addV(name).property('name', name).property('lineage',lineage).property('inferred',false).property('virtual',virtual).property('partition',partition)";
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

        private void AddConditionalElements(GraphElementInput elem, Dictionary<string, object> dict, ref string script)
        {
            AddConditionalElement(nameof(elem.lineage), elem.lineage, dict, ref script);
            AddConditionalElement(nameof(elem.name), elem.name, dict, ref script);
        }

        private void AddConditionalElement(string elemName, string elem, Dictionary<string, object> dict, ref string script)
        {
            if (!string.IsNullOrEmpty(elem))
            {
                dict.Add(elemName, elem);
                script += $".property('{elemName}',{elemName})";
            }
        }

        public async Task<GraphConnection> DeleteConnection(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(ServerFactory(userId), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                await SubmitWithRetry(gremlinClient, "g.E(id).drop()", new Dictionary<string, object> { { "id", id } });
                return null;
            }
        }

        public GremlinServer ServerFactory(string userId)
        {
            if (gremlinLocation == "azure")
            {
                return new GremlinServer(hostname, port, enableSsl: true, username: "/dbs/" + database + "/colls/" + userId, password: authKey);
            }
            else if (gremlinLocation == "local") //defaults will work
            {
                return new GremlinServer();
            }
            else
            {
                throw new ExecutionError($"Configuration error. gremlinLocation must be 'azure' or 'local', was {gremlinLocation}");
            }
        }

        public async Task<GraphObject> DeleteObject(string userId, string id)
        {
            using (var gremlinClient = new GremlinClient(ServerFactory(userId), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await SubmitWithRetry(gremlinClient, "g.V(id).drop()", new Dictionary<string, object> { { "id", id } });
                return null;
            }
        }

        public Task<GraphObject> UpdateObject(GraphObjectUpdate graphObject)
        {
            var dict = new Dictionary<string, object> { { "id", graphObject.id } };
            var script = "g.V().has(id, id)";
            AddConditionalElement(nameof(graphObject.externalId), graphObject.externalId, dict, ref script);
            AddConditionalElements(graphObject, dict, ref script);
            AddCommonElements(graphObject, dict, ref script);
            var res = await SubmitWithRetry(gremlinClient, script, dict);
            return ConvertGraphObject(res.FirstOrDefault());
        }

        public async Task<GraphConnection> UpdateConnection(GraphConnectionUpdate graphConnection)
        {
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

        public Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<GraphObject> GetGraphObjectById(string userId, string id)
        {
            throw new NotImplementedException();
        }

        public Task<GraphObject> GetGraphObjectByExternalId(string userId, string externalId)
        {
            throw new NotImplementedException();
        }

        public async Task<GraphConnection> GetConnectionByIds(string userId, string startId, string endId, string lineage)
        {
            using (var gremlinClient = new GremlinClient(ServerFactory(userId), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
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
            using (var gremlinClient = new GremlinClient(ServerFactory(userId), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
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
                                if ((string)r["label"] == property)
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
    }
}
