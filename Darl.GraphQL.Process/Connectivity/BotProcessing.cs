using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Process.Connectivity;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlCommon;
using DarlCompiler.Interpreter;
using GraphQL;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BotProcessing : IBotProcessing
    {
        readonly IConnectivity _conv;
        private readonly ILogger<BotProcessing> _logger;
        private readonly IConfiguration _config;
        readonly IGraphProcessing _graph;
        readonly IGraphHandler _ghandler;
        readonly IDistributedCache _cache;


        public BotProcessing(IConnectivity conv, ILogger<BotProcessing> logger, IConfiguration config, IGraphProcessing graph, IGraphHandler ghandler, IDistributedCache cache)
        {
            _conv = conv;
            _logger = logger;
            _config = config;
            _graph = graph;
            _ghandler = ghandler;
            _cache = cache;
        }

        public async Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId)
        {
            return await _ghandler.Discover(userId, KnowledgeGraphName, subjectId, null, new System.Text.StringBuilder(), null);
        }

        public async Task<List<InteractTestResponse>> InteractKGAsync(string userId, string KnowledgeGraphName, string conversationId, DarlVar conversationData)
        {
            _logger.LogWarning($"{nameof(InteractKGAsync)}: {userId}, {KnowledgeGraphName}, {conversationId}, {conversationData.Value}");
            var resp = new List<InteractTestResponse>();
            var bs = await GetBotState(conversationId);
            if (bs == null)//first call for this conversation
            {
                _logger.LogInformation($"new conversation, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new StoredBotData(), conversationData = new StoredBotData(), privateConversationData = new StoredBotData(), values = new List<DarlVar>(), updated = DateTime.UtcNow };
            }
            if (bs.kGraphData == null) // top level conversation
            {
                var responses = await _ghandler.InterpretText(userId, KnowledgeGraphName, "default:", conversationData);
                _logger.LogInformation($"top level conversation, text = {conversationData.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                if (responses.Any())
                {
                    var r = responses.Last();
                    if (r.response.dataType == DarlVar.DataType.seek)
                    {
                        //emit any preceding messages before starting seek.
                        foreach (var c in responses)
                        {
                            if (c == r)
                                break;
                            if (c.response.dataType != DarlVar.DataType.seek)
                            {
                                _logger.LogInformation($"Emitting text before seek: {c.response.Value}, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                                resp.Add(c);
                            }
                        }
                        bs.kGraphData = r.response.sequence;
                        bs.pending = null;
                        var res = await _ghandler.GraphPass(userId, KnowledgeGraphName, conversationId, r.response.sequence[0][0], r.response.sequence[1], r.response.sequence[2][0], bs.values, bs.pending, GraphProcess.seek);
                        if (!res.Item1.Any())
                        {
                            //no connection found
                        }
                        resp.Add(res.Item1.First());
                        bs.pending = res.Item2;
                    }
                    else if (r.response.dataType == DarlVar.DataType.discover)
                    {
                        //emit any preceding messages before starting seek.
                        foreach (var c in responses)
                        {
                            if (c == r)
                                break;
                            if (c.response.dataType != DarlVar.DataType.discover)
                            {
                                _logger.LogInformation($"Emitting text before discover: {c.response.Value}, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                                resp.Add(c);
                            }
                        }
                        bs.kGraphData = r.response.sequence;
                        bs.pending = null;
                        var discoverResp = await _ghandler.GraphPass(userId, KnowledgeGraphName, conversationId, r.response.sequence[0][0], r.response.sequence[1], r.response.sequence[2][0], bs.values, bs.pending, GraphProcess.discover);
                        if (discoverResp.Item1.Any())
                        {
                            resp.AddRange(discoverResp.Item1);
                        }
                        else
                        {
                            resp.Add(new InteractTestResponse { activeNodes = new List<string>(), darl = "", matches = new List<MatchedElement>(), response = new DarlVar { dataType = DarlVar.DataType.textual, Value = "nothing discovered" } });
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"top level response, text = {r.response.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                        resp.Add(r);
                    }
                    if (r.response.approximate)
                    {
                        _logger.LogInformation($"top level default response, text = {r.response.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                    }
                }
                else
                {
                    _logger.LogInformation($"No response found, text = {conversationData.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                    resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Internal error", dataType = DarlVar.DataType.textual, name = "response" } });
                }
            }
            else
            {
                //pass text through the navigation recognition tree checking for help, quit etc.
                var responses = await _ghandler.InterpretText(userId, KnowledgeGraphName, "navigation:", conversationData);
                if (responses.Any())
                {
                    var r = responses.Last();
                    if (r.response.name == "response")
                    {
                        resp.Add(r);
                    }
                    else if (r.response.name == "terminate")
                    {
                        bs.kGraphData = null;
                        bs.pending = null;
                        resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Quitting...", dataType = DarlVar.DataType.textual, name = "response" } });
                    }
                }
                else //continue processing the KGraph
                {
                    var request = new DarlVar { Value = conversationData.Value, name = RuleSetHandler.questionIdentifier, dataType = DarlVar.DataType.textual };
                    var existing = bs.values.Where(a => a.name == RuleSetHandler.questionIdentifier).FirstOrDefault();
                    if (existing != null)
                        bs.values.Remove(existing);
                    bs.values.Add(request);
                    try
                    {
                        var res = await _ghandler.GraphPass(userId, KnowledgeGraphName, conversationId, bs.kGraphData[0][0], bs.kGraphData[1], bs.kGraphData[2][0], bs.values, bs.pending, GraphProcess.seek);
                        resp.Add(res.Item1.Last());
                        bs.pending = res.Item2;
                        if (res.Item1.Count > 1)
                        {//check for completion response
                            foreach (var r in res.Item1)
                            {
                                if (r.response.dataType == DarlVar.DataType.complete)
                                {
                                    bs.kGraphData = null;
                                    bs.pending = null;
                                    break;
                                }
                            }
                        }
                    }
                    catch (ScriptException ex)
                    {
                        resp.Add(new InteractTestResponse { darl = "", response = new DarlVar { dataType = DarlVar.DataType.textual, Value = $"_Rule error: {ex.Message} location: {ex.Location}._ " } });
                    }
                    catch (Exception ex)
                    {
                        throw new ExecutionError($"Internal Error in GraphPass", ex);
                    }
                }
            }
            await SetBotState(conversationId, bs);
            return resp;

        }

        public async Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId,  List<string> paths, string completionLineage)
        {
            return await _ghandler.Seek(ks, targetId,  paths, completionLineage);
        }

        private async Task<BotState?> GetBotState(string conversationId)
        {
            var blob = await _cache.GetAsync(conversationId);
            if (blob != null)
            {
                using (var ms = new MemoryStream(blob))
                {
                    ms.Position = 0;
                    return Serializer.Deserialize<BotState>(ms);
                }
            }
            return null;
        }

        private async Task SetBotState(string conversationId, BotState state)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<BotState>(ms, state);
                ms.Position = 0;
                await _cache.SetAsync(conversationId, ms.ToArray());
            }
        }
    }
}
