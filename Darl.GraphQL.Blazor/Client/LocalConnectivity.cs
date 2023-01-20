using Darl.GraphQL.Blazor.Client.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml.Linq;
using ThinkBase.ComponentLibrary.Interfaces;
using ThinkBase.ComponentLibrary.Models;
using static Darl.GraphQL.Blazor.Client.Models.LineageRecordResponse;
using static System.Net.Mime.MediaTypeNames;

namespace Darl.GraphQL.Blazor.Client
{
    public class LocalConnectivity : IClientConnectivity
    {
        private GraphQLHttpClient client;
        private string path = "";
        private ITraceWriter traceWriter = new MemoryTraceWriter();
        private string authCode = "";
        private readonly string completedLineage = "adjective:5500";
        private readonly string textLineage = "noun:01,4,04,02,07,01";
        JsonSerializerOptions options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };


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

        public async Task<bool> DeleteRealConnection(string graphName, string id)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"mutation ($name: String! $id: String!){deleteGraphConnection(graphName: $name id: $id)}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task<bool> DeleteRealNode(string graphName, string id)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"mutation ($name: String! $id: String!){deleteGraphObject(graphName: $name id: $id)}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task<bool> DeleteRecognitionNode(string graphName, string id)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"mutation ($name: String! $id: String!){deleteRecognitionObject(name: $name id: $id)}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                return false;
            return true;
        }

        public async Task DeleteVirtualAttribute(string graphName, string id, string aLin)
        {
            var req = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id, attLineage = aLin },
                Query = @"mutation ($name: String! $id: String! $attLineage: String!){deleteVirtualObjectAttribute(name: $name lineage: $id attLineage: $attLineage){id}}"
            };
            var resp = await client.SendQueryAsync<object>(req);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
        }

        public async Task<KGraphDescription?> GetKGraphMetaData(string graphName)
        {
            GraphQLResponse<KGraphResponse>? model = null;
            try
            {
                var modelReq = new GraphQLHttpRequest
                {
                    Variables = new { name = graphName },
                    Query = "query ($name: String! ){kGraphByName(name: $name){name model{author copyright dateDisplay description fixedTime{raw} inferenceTime initialtext licenseUrl defaultTarget transient}}"
                };
                model = await client.SendQueryAsync<KGraphResponse>(modelReq);
                var metaData = model.Data.kGraphByName.model;
                return new KGraphDescription { name = graphName, model = new KGraphMetaData { author = metaData?.author, copyright = metaData?.copyright, description = metaData?.description, initialText = metaData?.initialText, licenseUrl = metaData?.licenseUrl} };
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<string>> GetKGraphs(string userId)
        {
            var modelReq = new GraphQLHttpRequest
            {
                Query = "query {kgraphs{name}}"
            };
            var list = await client.SendQueryAsync<KGraphsResponse>(modelReq);
            return list.Data.kgraphs!.Select(a => a.Name).ToList();
        }

        public async Task<string> GetLineagesForWord(string word, string wordType = "")
        {
            var modelReq = new GraphQLHttpRequest
            {
                Variables = new { word = word },
                Query = "query (word: String! ){getLineagesForWord(word: $word){description lineage lineageType, typeWord}"
            };
            var res = await client.SendQueryAsync<LineageRecordResponse>(modelReq);
            return System.Text.Json.JsonSerializer.Serialize(res.Data.getLineagesForWord, options);
        }

        public async Task<string> GetLineagesinKG(string graphName, string lType)
        {
            GraphElementTypes type = (GraphElementTypes)Enum.Parse(typeof(GraphElementTypes), lType);
            var modelReq = new GraphQLHttpRequest
            {
                Variables = new { graphName = graphName, graphType = type },
                Query = "query (graphName: String! graphType:  graphElementTypes!){getLineagesInKG(graphName: $graphName, graphType: $graphType){description lineage lineageType, typeWord}"
            };
            var resp = await client.SendQueryAsync<LineageRecordResponse>(modelReq);
            if (resp.Errors != null && resp.Errors.Count() > 0)
                throw new Exception(resp.Errors[0].Message);
            return System.Text.Json.JsonSerializer.Serialize(resp.Data.getLineagesForWord, options);
        }

        public async Task<string> GetNodeCode(string graphName, string id, IClientConnectivity.GraphSource source)
        {
            var rules = string.Empty;
            switch(source) 
            {
                case IClientConnectivity.GraphSource.real:
                    var req = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = id, lineage = completedLineage},
                        Query = @"query ($name: String! $id: String! $lineage: String!){getGraphAttribute(graphName: $name id: $id lineage: $lineage){value id name lineage}}"
                    };
                    var resp = await client.SendQueryAsync<GraphAttributeResponse>(req);
                    if (resp.Errors != null && resp.Errors.Count() > 0)
                        throw new Exception(resp.Errors[0].Message);
                    rules = resp.Data.getGraphAttribute?.value;
                    break;
                case IClientConnectivity.GraphSource.virt:
                    var vreq = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = id },
                        Query = @"query ($name: String! $id: String! $lineage: String!){getVirtualObjectByLineage(graphName: $name lineage: $id){id name lineage externalId properties{lineage name value}}}"
                    };
                    var vresp = await client.SendQueryAsync<GraphObjectResponse>(vreq);
                    if (vresp.Errors != null && vresp.Errors.Count() > 0)
                        throw new Exception(vresp.Errors[0].Message);
                    var att = vresp.Data.getVirtualObjectByLineage?.properties?.FirstOrDefault(a => a.lineage == completedLineage);
                    if (att != null)
                        rules = att.value;
                    break;
                case IClientConnectivity.GraphSource.rec:
                    var rreq = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = id },
                        Query = @"query ($name: String! $id: String!){getRecognitionObjectById(graphName: $name id: $id){id name lineage externalId properties{lineage name value}}}"
                    };
                    var rresp = await client.SendQueryAsync<GraphObjectResponse>(rreq);
                    if (rresp.Errors != null && rresp.Errors.Count() > 0)
                        throw new Exception(rresp.Errors[0].Message);
                    var ratt = rresp.Data.getRecognitionObjectById?.properties?.FirstOrDefault(a => a.lineage == completedLineage);
                    if (ratt != null)
                        rules = ratt.value;
                    break;
            }
            if(rules == null || rules.Trim() == string.Empty)
            {
                var sreq = new GraphQLHttpRequest()
                {
                    Variables = new { name = graphName, id = id, lineage = completedLineage },
                    Query = @"query ($name: String! $id: String!){getSuggestedRuleset(graphName: $name lineage: $id){id name lineage externalId properties{lineage name value}}}"
                };
                var sresp = await client.SendQueryAsync<SuggestedRulesetResponse>(sreq);
                if (sresp.Errors != null && sresp.Errors.Count() > 0)
                    throw new Exception(sresp.Errors[0].Message);
                if (sresp.Data.getSuggestedRuleset != null)
                    rules = sresp.Data.getSuggestedRuleset;
            }
            return rules ?? "";
        }

        public async Task<string?> GetRealConnectionLineage(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id},
                Query = @"query ($name: String! $id: String){getGraphConnectionById(graphName: $name lineage: $id){id name lineage }}"
            };
            var sresp = await client.SendQueryAsync<GraphConnectionResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            return sresp.Data.getGraphConnectionById.lineage;
        }

        public async Task<string?> GetRealConnectionName(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphConnectionById(graphName: $name lineage: $id){id name lineage }}"
            };
            var sresp = await client.SendQueryAsync<GraphConnectionResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            return sresp.Data.getGraphConnectionById.name;
        }

        public async Task<string> GetRealNodeAttributes(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name lineage: $id){id name externalId properties {id lineage name value confidence properties {id lineage name value confidence}} }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var obj = sresp.Data.getGraphObjectById;
            if (obj != null)
            {
                if (obj.properties != null)
                {
                    var result = System.Text.Json.JsonSerializer.Serialize(obj.properties, options);
                    return result;
                }
            }
            return "[]";
        }

        public async Task<string> GetRealNodeExternalId(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name lineage: $id){id name externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            return sresp.Data.getGraphObjectById?.lineage ?? "";
        }

        public async Task<string> GetRealNodeName(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name lineage: $id){id name externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            return sresp.Data.getGraphObjectById?.name ?? "";
        }

        public async Task<string> GetRealNodeTypeWords(string graphName, string id)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name lineage: $id){id name externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var lin = sresp.Data.getGraphObjectById?.lineage ?? "";
            if (lin.Contains('+'))
            {
                var lins = lin.Split('+');
                return await LookUpTypeWord(lins[0]) + " + " + await LookUpTypeWord(lins[1]);
            }
            return lin == null ? string.Empty : await LookUpTypeWord(lin);
        }

        public async Task<string> GetRecognitionLineage(string graphName, string id)
        {
            var rreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String!){getRecognitionObjectById(graphName: $name id: $id){id name lineage}}"
            };
            var rresp = await client.SendQueryAsync<GraphObjectResponse>(rreq);
            if (rresp.Errors != null && rresp.Errors.Count() > 0)
                throw new Exception(rresp.Errors[0].Message);
            return rresp.Data.getRecognitionObjectById?.lineage ?? "";
        }

        public async Task<string> GetRecognitionMarkDown(string graphName, string id)
        {
            var rreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String!){getRecognitionObjectById(graphName: $name id: $id){id name lineage externalId properties{lineage name value}}}"
            };
            var rresp = await client.SendQueryAsync<GraphObjectResponse>(rreq);
            if (rresp.Errors != null && rresp.Errors.Count() > 0)
                throw new Exception(rresp.Errors[0].Message);
            if(rresp.Data.getRecognitionObjectById?.properties != null)
            {
                var att = rresp.Data.getRecognitionObjectById?.properties.FirstOrDefault(a => a.type == GraphAttribute.DataType.MARKDOWN);
                if (att != null)
                    return att.value ?? "";
            }
            return string.Empty;
        }

        public async Task<string> GetTypeWord(string lineage)
        {
            return await LookUpTypeWord(lineage);
        }

        public Task<(string?, bool, bool)> GetUserSettings(string userId, string defaultKG)
        {
            return Task.FromResult((defaultKG, false, true));
        }

        public async Task<string> GetVirtualNodeAttributes(string graphName, string id)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String! $lineage: String!){getVirtualObjectByLineage(graphName: $name lineage: $id){id name lineage externalId properties {id lineage name value confidence properties {id lineage name value confidence}}}}"
            };
            var vresp = await client.SendQueryAsync<GraphObjectResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            var obj = vresp.Data.getVirtualObjectByLineage;
            if (obj != null)
            {
                if (obj.properties != null)
                {
                    var result = System.Text.Json.JsonSerializer.Serialize(obj.properties, options);
                    return result;
                }
            }
            return "[]";
        }

        public Task<string> GetVirtualNodeLineage(string graphName, string id)
        {
            return Task.FromResult(id);
        }

        public async Task<string> GetVirtualNodeName(string graphName, string id)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String! $lineage: String!){getVirtualObjectByLineage(graphName: $name lineage: $id){id name lineage externalId properties {id lineage name value confidence properties {id lineage name value confidence}}}}"
            };
            var vresp = await client.SendQueryAsync<GraphObjectResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            var obj = vresp.Data.getVirtualObjectByLineage;
            return obj?.name ?? string.Empty;
        }

        public Task<bool> IsDemo(string graphName)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> IsValidLineage(string lineage)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { lineage },
                Query = @"query ($lineage: String! ){isValidLineage(lineage: $lineage)}"
            };
            var vresp = await client.SendQueryAsync<BoolResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            return vresp.Data.isValidLineage;
        }

        public async Task<string> LintAsync(string darl)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { darl },
                Query = @"query ($darl: String! ){lintDarlMeta(darl: $lineage)}"
            };
            var vresp = await client.SendQueryAsync<LintErrorResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            return System.Text.Json.JsonSerializer.Serialize(vresp.Data.lintDarlMeta, options);
        }

        public async Task<string> RealKGraphData(string name)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name },
                Query = @"query ($name: String! ){getRealKGDisplay(graphName: $name){nodes{id name, lineage subLineage externalId parent hasCode} edges{id name source target lineage}}}"
            };
            var vresp = await client.SendQueryAsync<DisplayModelResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            var dm = vresp.Data.getRealKGDisplay;
            return System.Text.Json.JsonSerializer.Serialize(Convert(dm!), options);

        }

        public async Task<string> RecognitionKGraphData(string name)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name },
                Query = @"query ($name: String! ){getRecognitionKGDisplay(graphName: $name){nodes{id name, lineage subLineage externalId parent hasCode} edges{id name source target lineage}}}"
            };
            var vresp = await client.SendQueryAsync<DisplayModelResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            var dm = vresp.Data.getRecognitionKGDisplay;
            return System.Text.Json.JsonSerializer.Serialize(Convert(dm!), options);
        }

        public async Task SaveKGraph(string graphName)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName },
                Query = @"query ($name: String! ){saveKGraph(graphName: $name)}"
            };
            var vresp = await client.SendQueryAsync<object>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
        }

        /// <summary>
        /// Not needed for SaaS
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetActiveKGForBot(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public async Task SetDefaultTarget(string graphName, string id)
        {
            var k = await GetKGraphMetaData(graphName);
            k!.model!.defaultTarget = id;
            await UpdateKGraphMetaData(graphName, k);
        }

        public async Task UpdateKGraphMetaData(string name, KGraphDescription desc)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name = name, metadata = desc },
                Query = @"mutation ($name: String! $metadata:  modelMetaDataUpdate !){updateKGraphMetadata(name: $name update: $metadata)}"
            };
            var vresp = await client.SendQueryAsync<object>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);

        }

        public async Task<bool> UpdateNodeCode(string graphName, string currentNodeId, string editorText, IClientConnectivity.GraphSource src)
        {
            var att = new GraphAttributeInput { lineage = completedLineage, name = "complete", type = GraphAttribute.DataType.RULESET, confidence = 1.0, value = editorText };
            await UpdateAttribute(graphName, currentNodeId, src, att);
            return true;
        }

        public async Task UpdateNodeMarkDown(string graphName, string currentNodeId, string markDown, IClientConnectivity.GraphSource src)
        {
            var att = new GraphAttributeInput { name = "text", lineage = textLineage, type = GraphAttribute.DataType.MARKDOWN, value = markDown };
            await UpdateAttribute(graphName, currentNodeId, src, att);
        }

        public async Task UpdateRealConnectionName(string graphName, string id, string text)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String!){getGraphConnectionById(graphName: $name id: $id){id name lineage startId endId weight }}"
            };
            var sresp = await client.SendQueryAsync<GraphConnectionResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var conn = sresp.Data.getGraphConnectionById;
            var cUpdate = new GraphConnectionUpdate { id = conn.id, endId= conn.endId, startId = conn.startId, lineage = conn.lineage, name = text };
            var creq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, conn = cUpdate },
                Query = @"mutation ($name: String! $conn: graphConnectionUpdate!){updateGraphConnection(graphName: $name graphConnection: $conn ontology: BUILD)}"
            };
            await client.SendQueryAsync<object>(creq);

        }

        public async Task UpdateRealNodeAttribute(string graphName, string id, string newAtt)
        {
            var att = System.Text.Json.JsonSerializer.Deserialize<GraphAttributeInput>(newAtt, options);
            await UpdateAttribute(graphName, id, IClientConnectivity.GraphSource.real, att);
        }

        public async Task UpdateRealNodeExternalId(string graphName, string id, string newExternalId)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name id: $id){id name lineage externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var conn = sresp.Data.getGraphObjectById;
            var cUpdate = new GraphObjectUpdate { id = conn.id, lineage = conn.lineage, name = conn.name, externalId = newExternalId };
            var creq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, conn = cUpdate },
                Query = @"mutation ($name: String! $conn: graphObjectUpdate!){updateGraphObject(graphName: $name graphObject: $conn ontology: BUILD)}"
            };
            await client.SendQueryAsync<object>(creq);
        }

        public async Task UpdateRealNodeName(string graphName, string id, string newName)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getGraphObjectById(graphName: $name id: $id){id name lineage externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var conn = sresp.Data.getGraphObjectById;
            var cUpdate = new GraphObjectUpdate { id = conn.id, lineage = conn.lineage, name = newName, externalId = conn.externalId };
            var creq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, conn = cUpdate },
                Query = @"mutation ($name: String! $conn: graphObjectUpdate!){updateGraphObject(graphName: $name graphObject: $conn ontology: BUILD)}"
            };
            await client.SendQueryAsync<object>(creq);
        }

        public async Task UpdateRecognitionNode(string graphName, string id, string lineage, string word)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, id = id },
                Query = @"query ($name: String! $id: String){getRecognitionObjectById(graphName: $name id: $id){id name lineage externalId }}"
            };
            var sresp = await client.SendQueryAsync<GraphObjectResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            var conn = sresp.Data.getRecognitionObjectById;
            var cUpdate = new GraphObjectUpdate { id = id, lineage = lineage, name = word, externalId = conn.externalId };
            var creq = new GraphQLHttpRequest()
            {
                Variables = new { name = graphName, conn = cUpdate },
                Query = @"mutation ($name: String! $conn: graphObjectUpdate!){updateRecognitionObject(graphName: $name graphObject: $conn ontology: BUILD)}"
            };
            await client.SendQueryAsync<object>(creq);
        }

        public async Task UpdateVirtualAttribute(string graphName, string id, string newAtt)
        {
            var att = System.Text.Json.JsonSerializer.Deserialize<GraphAttributeInput>(newAtt, options);
            await UpdateAttribute(graphName, id, IClientConnectivity.GraphSource.virt, att!);
        }

        public async Task<string> VirtualKGraphData(string name)
        {
            var vreq = new GraphQLHttpRequest()
            {
                Variables = new { name },
                Query = @"query (name: String! ){getVirtualKGDisplay(graphName: name){nodes{id name, lineage subLineage externalId parent hasCode} edges{id name source target lineage}}}"
            };
            var vresp = await client.SendQueryAsync<DisplayModelResponse>(vreq);
            if (vresp.Errors != null && vresp.Errors.Count() > 0)
                throw new Exception(vresp.Errors[0].Message);
            var dm = vresp.Data.getVirtualKGDisplay;
            return System.Text.Json.JsonSerializer.Serialize(Convert(dm!), options);
        }

        #region private

        private async Task UpdateAttribute(string graphName, string currentNodeId, IClientConnectivity.GraphSource src, GraphAttributeInput att)
        {
            switch (src)
            {
                case IClientConnectivity.GraphSource.real:
                    var req = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = currentNodeId, att = att },
                        Query = @"mutation ($name: String! $id: String! $att: graphAttributeInput!){updateGraphObjectAttribute(name: $name id: $id att: $att)}"
                    };
                    var resp = await client.SendQueryAsync<object>(req);
                    if (resp.Errors != null && resp.Errors.Count() > 0)
                        throw new Exception(resp.Errors[0].Message);
                    break;
                case IClientConnectivity.GraphSource.virt:
                    var vreq = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = currentNodeId, att = att },
                        Query = @"mutation ($name: String! $id: String! $att: graphAttributeInput!){updateVirtualObjectAttribute(name: $name lineage: $id att: $att)}"
                    };
                    var vresp = await client.SendQueryAsync<GraphObjectResponse>(vreq);
                    if (vresp.Errors != null && vresp.Errors.Count() > 0)
                        throw new Exception(vresp.Errors[0].Message);
                    break;
                case IClientConnectivity.GraphSource.rec:
                    var rreq = new GraphQLHttpRequest()
                    {
                        Variables = new { name = graphName, id = currentNodeId, att = att },
                        Query = @"mutation ($name: String! $id: String! $att: graphAttributeInput!){updateRecognitionObjectAttribute(name: $name lineage: $id att: $att)}"
                    };
                    var rresp = await client.SendQueryAsync<GraphObjectResponse>(rreq);
                    if (rresp.Errors != null && rresp.Errors.Count() > 0)
                        throw new Exception(rresp.Errors[0].Message);
                    break;
            }
        }

        private async Task<string> LookUpTypeWord(string lineage)
        {
            var sreq = new GraphQLHttpRequest()
            {
                Variables = new { lineage },
                Query = @"query (lineage: String!){getTypeWordForLineage(lineage: $lineage)}"
            };
            var sresp = await client.SendQueryAsync<TypeWordResponse>(sreq);
            if (sresp.Errors != null && sresp.Errors.Count() > 0)
                throw new Exception(sresp.Errors[0].Message);
            return sresp.Data.getTypeWordForLineage ?? string.Empty;
        }
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

        private CytoDataModel Convert(DisplayModel model)
        {
            var om = new CytoDataModel();
            foreach (var n in model.nodes)
            {
                om.nodes.Add(new CytoDataNodeElement { data = new ColouredDisplayObject { externalId = n.externalId, hasCode = n.hasCode, id = n.id, lineage = n.lineage, name = n.name, parent = n.parent, subLineage = n.subLineage } });
            }
            foreach (var e in model.edges)
            {
                om.edges.Add(new CytoDataEdgeElement { data = e });
            }
            return om;
        }
        #endregion
    }

}
