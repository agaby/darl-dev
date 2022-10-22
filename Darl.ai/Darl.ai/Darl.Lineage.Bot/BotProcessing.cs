using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler.Interpreter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static Darl.Lineage.Bot.IBotProcessing;

namespace Darl.Lineage.Bot
{
    public class BotProcessing : IBotProcessing
    {
        private readonly ILogger<BotProcessing> _logger;
        private readonly IConfiguration _config;
        readonly IGraphProcessing _graph;
        readonly IGraphHandler _ghandler;
        IBotStateStorage _stateStorage;

        private readonly ISubject<KnowledgeState> _knowledgeStateStream = new ReplaySubject<KnowledgeState>(1);


        public BotProcessing(ILogger<BotProcessing> logger, IConfiguration config, IGraphProcessing graph, IGraphHandler ghandler, IBotStateStorage stateStorage)
        {
            _logger = logger;
            _config = config;
            _graph = graph;
            _ghandler = ghandler;
            _stateStorage = stateStorage;
        }

        public async Task<DarlMineReport> Build(string userId, string name, string data, string patternPath, List<DataMap> dataMaps, LoadType ltype = LoadType.xml, LearningForm form = LearningForm.supervised)
        {
            return await _ghandler.Build(userId, name, data, patternPath, dataMaps, ltype);
        }

        public async Task<KnowledgeState> Discover(string userId, string KnowledgeGraphName, string subjectId)
        {
            return await _ghandler.Discover(userId, KnowledgeGraphName, subjectId, null, new System.Text.StringBuilder(), null);
        }

        public IObservable<KnowledgeState> ObservableKStates()
        {
            return _knowledgeStateStream.AsObservable();
        }

        public async Task<List<InteractTestResponse>> InteractKGAsync(string userId, string KnowledgeGraphName, string conversationId, DarlVar conversationData)
        {
            _logger.LogInformation($"{nameof(InteractKGAsync)}: {userId}, {KnowledgeGraphName}, {conversationId}, {conversationData.Value}");
            var resp = new List<InteractTestResponse>();
            var bs = await _stateStorage.GetBotState(userId, conversationId);
            if (bs == null)//first call for this conversation
            {
                _logger.LogInformation($"new conversation, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                bs = new BotState { conversationId = conversationId, userId = userId, values = new List<DarlVar>(), updated = DateTime.UtcNow };
            }
            if(!bs.states.ContainsKey(KnowledgeGraphName))
            {
                bs.states.Add(KnowledgeGraphName, new KnowledgeState { userId = userId, knowledgeGraphName = KnowledgeGraphName, subjectId = conversationId });
            }
            var model = await _graph.GetModel(userId, KnowledgeGraphName);
            if(model == null)
            {
                resp.Add(new InteractTestResponse { response = new DarlVar { Value = $"{KnowledgeGraphName} not found for user {userId}", dataType = DarlVar.DataType.textual, name = "response" } });
                return resp;
            }
            if (bs.kGraphData == null) // top level conversation
            {
                var responses = await _ghandler.InterpretText(model, "default:", conversationData);
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
                        var res = await _ghandler.GraphPass(bs.states[KnowledgeGraphName], model, conversationId, r.response.sequence[0][0], r.response.sequence[1], r.response.sequence[2][0], bs.values, bs.pending, GraphProcess.seek);
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
                        var discoverResp = await _ghandler.GraphPass(bs.states[KnowledgeGraphName], model, conversationId, r.response.sequence[0][0], r.response.sequence[1], r.response.sequence[2][0], bs.values, bs.pending, GraphProcess.discover);
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
                var responses = await _ghandler.InterpretText(model, "navigation:", conversationData);
                if (responses.Any())
                {
                    var r = responses.Last();
                    if (r.response.name == "response")
                    {
                        resp.Add(r);
                    }
                    else if (r.response.name == "terminate")
                    {
                        bs.ClearBotState(KnowledgeGraphName);
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
                        var res = await _ghandler.GraphPass(bs.states[KnowledgeGraphName], model, conversationId, bs.kGraphData[0][0], bs.kGraphData[1], bs.kGraphData[2][0], bs.values, bs.pending, GraphProcess.seek);
                        resp.Add(res.Item1.Last());
                        bs.pending = res.Item2;
                        if (res.Item1.Count > 1)
                        {//check for completion response
                            foreach (var r in res.Item1)
                            {
                                if (r.response.dataType == DarlVar.DataType.complete)
                                {
                                    _knowledgeStateStream.OnNext(bs.states[KnowledgeGraphName]);
                                    bs.kGraphData = null;
                                    bs.pending = null;
                                    break;
                                }
                            }
                        }
                    }
                    catch(StructureException ex)
                    {
                        _logger.LogInformation(ex, "Structure exception in GraphPass");
                        resp.Add(new InteractTestResponse { darl = "", response = new DarlVar { dataType = DarlVar.DataType.textual, Value = ex.Message } });
                    }
                    catch (ScriptException ex)
                    {
                        _logger.LogInformation(ex, "Script exception in GraphPass");
                        resp.Add(new InteractTestResponse { darl = "", response = new DarlVar { dataType = DarlVar.DataType.textual, Value = $"_Rule error: {ex.Message} location: {ex.Location}._ " } });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Internal exception in GraphPass");
                        throw new Exception($"Internal Error in GraphPass: {ex.Message}", ex);
                    }
                }
            }
            await _stateStorage.SetBotState(userId, conversationId, bs);
            return resp;

        }

        public async Task<DarlMineReport> Learn(string userId, string graphName, string target, IBotProcessing.LearningForm form, string targetLineage, string valueLineage, int percentTrain, IGraphHandler.SetChoices sets)
        {
            return await _ghandler.Learn(userId, graphName, target, form, targetLineage, valueLineage,percentTrain, sets);
        }

        public async Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId, List<string> paths, string completionLineage)
        {
            return await _ghandler.Seek(ks, targetId, paths, completionLineage);
        }

        public async Task<KnowledgeState?> GetInteractKnowledgeState(string id, string userId, string graphName, bool external = false)
        {
            var bs = await _stateStorage.GetBotState(userId, id);
            if (bs == null)
                return null;
            if (!bs.states.ContainsKey(graphName))
                return null;
            if (!external)
                return bs.states[graphName];
            return await _graph.ConvertKSIDs(bs.states[graphName]);
        }
    }
}
