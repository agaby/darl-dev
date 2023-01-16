using Darl.GraphQL.Blazor.Client.Models;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Xml.Linq;
using ThinkBase.ComponentLibrary.Interfaces;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Blazor.Client
{
    public class LocalConnectivity : IClientConnectivity
    {
        private GraphQLHttpClient client;
        private string path = "";
        private ITraceWriter traceWriter = new MemoryTraceWriter();
        private string authCode = "";

        public LocalConnectivity()
        {
            client = new GraphQLHttpClient(path, new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                TraceWriter = traceWriter,
                ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true },
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Converters = { new ConstantCaseEnumConverter() }
            }));
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authCode);
        }
        public async Task<string> AddRealNode(string graphName, string name, string externalId, string lin, string sublin)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, graphObject = new GraphObjectInput {  name = name, externalId = externalId, lineage = lin, subLineage = sublin} },
                Query = @"mutation ($name: String! $graphObject: graphObjectInput!){createGraphObject(graphName: $name graphObject: $graphObject ontology: BUILD){id}}"
            };
            var resp = await client.SendQueryAsync<GraphObjectResponse>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
            return resp.Data.createGraphObject?.id ?? "";
        }

        public Task<Subscription?> AddSubscription(string sub)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateKGraph(string graphName)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName },
                Query = @"mutation ($name: String!){createKGraph(name: $name)}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task<string?> CreateRealConnection(string graphName, string name, string lin, string startId, string endId, string id)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, graphConnection = new GraphConnectionInput{ lineage = lin, name = name, endId = endId,startId = startId } },
                Query = @"mutation ($name: String! $graphConnection: graphConnectionInput!){createGraphConnection(graphName: $name graphConnection: $graphConnection ontology: BUILD){id}}"
            };
            var resp = await client.SendQueryAsync<GraphConnectionResponse>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
            return resp.Data.createGraphConnection?.id ?? "";
        }

        public async Task<bool> CreateRecognitionConnection(string graphName, string startId, string endId)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, graphConnection = new GraphConnectionInput { lineage = "", name = "precedes", endId = endId, startId = startId } },
                Query = @"mutation ($name: String! $graphConnection: graphConnectionInput!){createRecognitionConnection(graphName: $name graphConnection: $graphConnection ontology: BUILD){id}}"
            };
            var resp = await client.SendQueryAsync<GraphConnectionResponse>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task<string> CreateRecognitionNode(string graphName, string lineage, string name)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, graphObject = new GraphObjectInput { name = name, externalId = "", lineage = lineage, subLineage = "" } },
                Query = @"mutation ($name: String! $graphObject: graphObjectInput!){createRecognitionObject(graphName: $name graphObject: $graphObject ontology: BUILD){id}}"
            };
            var resp = await client.SendQueryAsync<GraphObjectResponse>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
            return resp.Data.createGraphObject?.id ?? "";
        }

        public async Task<bool> DeleteKGraph(string graphName)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName },
                Query = @"mutation ($name: String!){deleteKG(name: $name)}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task DeleteRealAttribute(string graphName, string id, string aLin)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id, attLineage = aLin },
                Query = @"mutation ($name: String! $id: String! $attLineage: String!){deleteGraphObjectAttribute(name: $name id: $id attLineage: $attLineage){id}}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
        }

        public Task<bool> DeleteRealConnection(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRealNode(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRecognitionNode(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVirtualAttribute(string graphName, string id, string aLin)
        {
            throw new NotImplementedException();
        }

        public Task<KGraphDescription?> GetKGraphMetaData(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetKGraphs(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLineagesForWord(string word, string wordType = "")
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLineagesinKG(string graphName, string lType)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNodeCode(string name, string id, IClientConnectivity.GraphSource source)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetRealConnectionLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetRealConnectionName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeAttributes(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeExternalId(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeTypeWords(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRecognitionLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRecognitionMarkDown(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTypeWord(string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<(string?, bool, bool)> GetUserSettings(string userId, string defaultKG)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeAttributes(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsDemo(string graphName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsValidLineage(string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<string> LintAsync(string darl)
        {
            throw new NotImplementedException();
        }

        public Task<string> RealKGraphData(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecognitionKGraphData(string name)
        {
            throw new NotImplementedException();
        }

        public Task SaveKGraph(string graphName)
        {
            throw new NotImplementedException();
        }

        public Task SetActiveKGForBot(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultTarget(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateKGraphMetaData(string name, KGraphDescription desc)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateNodeCode(string graphName, string currentNodeId, string editorText, IClientConnectivity.GraphSource src)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNodeMarkDown(string graphName, string currentNodeId, string markDown, IClientConnectivity.GraphSource src)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealConnectionName(string graphName, string id, string text)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeAttribute(string graphName, string id, string newAtt)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeExternalId(string graphName, string id, string newExternalId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeName(string graphName, string id, string newName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRecognitionNode(string graphName, string id, string lineage, string word)
        {
            throw new NotImplementedException();
        }

        public Task UpdateVirtualAttribute(string graphName, string id, string newAtt)
        {
            throw new NotImplementedException();
        }

        public Task<string> VirtualKGraphData(string name)
        {
            throw new NotImplementedException();
        }

        #region private
        private GraphObjectInput Convert(GraphObject go)
        {
            var lineage = go.lineage;
            var subLineage = string.Empty;
            if (go.lineage.Contains('+'))
            {
                lineage = go.lineage.Substring(0, go.lineage.IndexOf('+'));
                subLineage = go.lineage.Substring(go.lineage.IndexOf('+'));
            }
            List<GraphAttributeInput>? properties = null;
            if (go.properties != null)
            {
                properties = new List<GraphAttributeInput>();
                foreach (var p in go.properties)
                {
                    properties.Add(ConvertAttributeInput(p));
                }
            }
            List<DarlTimeInput>? existence = null;
            if (go.existence != null)
            {
                existence = new List<DarlTimeInput>();
                foreach (var e in go.existence)
                {
                    existence.Add(Convert(e));
                }
            }
            return new GraphObjectInput { name = go.name, lineage = lineage, externalId = go.externalId, subLineage = subLineage, properties = properties, existence = existence };
        }

        private GraphObjectUpdate ConvertU(GraphObject go)
        {
            var lineage = go.lineage;
            var subLineage = string.Empty;
            if (go.lineage.Contains('+'))
            {
                lineage = go.lineage.Substring(0, go.lineage.IndexOf('+'));
                subLineage = go.lineage.Substring(go.lineage.IndexOf('+'));
            }
            List<GraphAttributeInput>? properties = null;
            if (go.properties != null)
            {
                properties = new List<GraphAttributeInput>();
                foreach (var p in go.properties)
                {
                    properties.Add(ConvertAttributeInput(p));
                }
            }
            List<DarlTimeInput>? existence = null;
            if (go.existence != null)
            {
                existence = new List<DarlTimeInput>();
                foreach (var e in go.existence)
                {
                    existence.Add(Convert(e));
                }
            }
            return new GraphObjectUpdate { name = go.name, lineage = lineage, externalId = go.externalId, subLineage = subLineage, properties = properties, existence = existence, id = go.id };
        }

        private DarlTimeInput Convert(DarlTime e)
        {
            return new DarlTimeInput { raw = e.raw, precision = e.precision };
        }

        private GraphConnectionInput Convert(GraphConnection gc)
        {
            List<DarlTimeInput>? existence = null;
            if (gc.existence != null)
            {
                existence = new List<DarlTimeInput>();
                foreach (var e in gc.existence)
                {
                    existence.Add(Convert(e));
                }
            }
            return new GraphConnectionInput { existence = existence, name = gc.name, lineage = gc.lineage, startId = gc.startId, endId = gc.endId };
        }

        public static GraphAttributeInput ConvertAttributeInput(GraphAttribute a)
        {
            List<GraphAttributeInput>? properties = null;
            if (a.properties != null)
            {
                properties = new List<GraphAttributeInput>();
                foreach (var b in a.properties) { properties.Add(ConvertAttributeInput(b)); }
            }
            return new GraphAttributeInput { confidence = a.confidence, inferred = a.inferred, value = a.value ?? "", existence = a.existence, name = a.name, type = a.type, lineage = a.lineage, properties = properties };
        }
        #endregion
    }
}
