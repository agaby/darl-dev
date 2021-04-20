using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Process.Connectivity;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlCommon;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BotProcessing : IBotProcessing
    {
        IConnectivity _conv;
        IFormApi _form;
        IRuleFormInterface _rfi;
        IHttpContextAccessor _context;
        ITrigger _trigger;
        private ILogger<BotProcessing> _logger;
        private IConfiguration _config;
        IGraphProcessing _graph;
        IGraphHandler _ghandler;


        public BotProcessing(IConnectivity conv, IFormApi form, IRuleFormInterface rfi, ITrigger trigger, ILogger<BotProcessing> logger, IConfiguration config, IHttpContextAccessor context, IGraphProcessing graph, IGraphHandler ghandler)
        {
            _conv = conv;
            _form = form;
            _rfi = rfi;
            _context = context;
            _trigger = trigger;
            _logger = logger;
            _config = config;
            _graph = graph;
            _ghandler = ghandler;
        }

        public async Task<List<InteractTestResponse>> InteractAsync(string userId, string botModelName, string conversationId, DarlVar conversationData)
        {
            _logger.LogWarning($"{nameof(InteractAsync)}: {userId}, {botModelName}, {conversationId}, {conversationData.Value}");
            List<InteractTestResponse> resp = new List<InteractTestResponse>();
            //cache these?
            var bmt = await _conv.GetBotModel(userId, botModelName);
            if (bmt == null)
                throw new ExecutionError($"{botModelName} not found in this account.");
            LineageModel bm = null;
            using (var ms = new MemoryStream(bmt.Model))
            {
                ms.Position = 0;
                bm = LineageModel.Load(ms);
            }
            BotState bs = await _conv.GetBotState(userId, conversationId);
            if(bs == null)//first call for this conversation
            {
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new StoredBotData(), conversationData = new StoredBotData(), privateConversationData = new StoredBotData(), values = new List<DarlVar>(), ruleProcessing = new Stack<RuleSetHandler>(), updated = DateTime.UtcNow }; 
            }
            var stores = bm.CreateStores(userId, _rfi, bs.values, bs.userData, bs.conversationData, bs.privateConversationData );
            //add extra stores defined locally
            var botFormat = JsonConvert.DeserializeObject<BotFormat>(bm.form);
            if(botFormat.Stores.Contains("Graph"))
            {
                stores.Add("Graph", new GraphLocalStore(_config, _logger as ILogger<GraphLocalStore>, _context, _graph));
            }
            if (bs.ruleProcessing.Count == 0) // conversational processing
            {
                var responses = await bm.InteractTest(conversationData, bs.values, stores, true);
                if(responses.Any())
                {
                    var r = responses.Last();
                    if(r.response.dataType == DarlVar.DataType.ruleset)
                    {
                        //call the ruleset and stack it
                        var newRF = ((CallStore)stores["Call"]).currentRF;
                        if(newRF != null)
                            bs.ruleProcessing.Push(new RuleSetHandler { user = userId, rf = newRF, modelId = conversationId });
                    }
                    else
                    {
                        resp.Add(r);
                    }
                    if (r.response.approximate)
                    {
                        string version = "unknown";
                        if(bm.modelSettings.ContainsKey("version"))
                        {
                            var v = JsonConvert.DeserializeObject<DarlVar>(bm.modelSettings["version"]);
                            version = v.Value;
                        }
                        await _conv.CreateDefaultResponse(new DefaultResponse { date = DateTime.UtcNow, model = botModelName, message = conversationData.Value, response = r.response.Value, userId = userId, version = version });
                    }
                }
                else
                {
                    resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Internal error", dataType = DarlVar.DataType.textual } });
                }
            }
            bool recursive = false;
            DarlVar recursiveRuleSet = null;
            if (bs.ruleProcessing.Count > 0) //ruleset processing
            {
                var rsh = bs.ruleProcessing.Peek();
                rsh.Trigger = _trigger;
                //handle simple navigation
                var c = LineageModelBotExtensions.HandleRuleSetCommands(conversationData.Value);
                switch(c)
                {
                    case LineageModelBotExtensions.Commands.quit:
                        bs.ruleProcessing.Clear();
                        resp.Add(new InteractTestResponse { response = new DarlVar { Value = "I'm quitting that sequence of questions.", dataType = DarlVar.DataType.textual } });
                        await _conv.SaveBotState(bs);
                        return resp;
                    case LineageModelBotExtensions.Commands.back:
                        if (rsh.CanGoBack())
                            rsh.Back(bs.values); //removes last value from values
                        break;
                    default:
                        var request = new DarlVar { Value = conversationData.Value, name = RuleSetHandler.questionIdentifier, dataType = DarlVar.DataType.textual };
                        var existing = bs.values.Where(a => a.name == RuleSetHandler.questionIdentifier).FirstOrDefault();
                        if (existing != null)
                            bs.values.Remove(existing);
                        bs.values.Add(request); //should be hashset throughout
                        break;
                }
                //pass on to ruleset
                List<InteractTestResponse> responses = null;
                try 
                {
                     responses = await rsh.RuleSetPass(bs.values, stores, bmt.serviceConnectivity);
                }
                catch(Exception ex)
                {
                    throw new ExecutionError("Error in rule set processing", ex);
                }
                //add to resp;
                foreach (var r in responses)
                {
                    switch (r.response.dataType)
                    {
                        case DarlVar.DataType.ruleset:
                            //                           var newRF = ((CallStore)stores["Call"]).currentRF; 
                            //                           if (newRF != null)
                            //                                bs.ruleProcessing.Push(new RuleSetHandler { user = userId, rf = newRF, modelId = conversationId });
                            recursive = true;
                            recursiveRuleSet = r.response;
                            bs.values.Clear();
                            bs.ruleProcessing.Pop();
                            var newRF = ((CallStore)stores["Call"]).currentRF;
                            if (newRF != null)
                                bs.ruleProcessing.Push(new RuleSetHandler { user = userId, rf = newRF, modelId = conversationId });
                            break;
                        case DarlVar.DataType.complete:
                            //clear out values etc here
                            bs.values.Clear();
                            bs.ruleProcessing.Pop();
                            break;
                        case DarlVar.DataType.categorical:
                            //consider dynamic
                            resp.Add(r);
                            break;
                        default:
                            resp.Add(r);
                            break;
                    }
                }
            }
            await _conv.SaveBotState(bs);          
            if(recursive)
            {
                resp.AddRange(await InteractAsync(userId, botModelName, conversationId, recursiveRuleSet));
            }

            return resp;
        }

        public async Task<BotTestView> InteractTestAsync(string userId, string botModelName, string conversationId, string text, bool reset)
        {
            List<InteractTestResponse> resp = new List<InteractTestResponse>();
            //cache these?
            var bmt = await _conv.GetBotModel(userId, botModelName);
            LineageModel bm = null;
            using (var ms = new MemoryStream(bmt.Model))
            {
                ms.Position = 0;
                bm = LineageModel.Load(ms);
            }
            BotState bs = await _conv.GetBotState(userId, conversationId);
            if (bs == null)//first call for this conversation
            {
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new StoredBotData(), conversationData = new StoredBotData(), privateConversationData = new StoredBotData(), values = new List<DarlVar>(), ruleProcessing = new Stack<RuleSetHandler>() };
            }
            var stores = bm.CreateStores(userId, _rfi, bs.values, bs.userData, bs.conversationData, bs.privateConversationData);
            var btv = new BotTestView() { conversationID = bs.conversationId, conversation = new List<string>(), darl = "" };
            var responses = await bm.InteractTest(new DarlVar { Value = text, name = "text", dataType = DarlVar.DataType.textual }, bs.values, stores);
            if (responses.Any())
            {

            }
            else
            {
                resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Internal error", dataType = DarlVar.DataType.textual } });
            }

            await _conv.SaveBotState(bs);
            return btv;
        }

        public async Task<List<InteractTestResponse>> InteractKGAsync(string userId, string KnowledgeGraphName, string conversationId, DarlVar conversationData)
        {
            _logger.LogWarning($"{nameof(InteractKGAsync)}: {userId}, {KnowledgeGraphName}, {conversationId}, {conversationData.Value}");
            List<InteractTestResponse> resp = new List<InteractTestResponse>();
            BotState bs = await _conv.GetBotState(userId, conversationId);
            if (bs == null)//first call for this conversation
            {
                _logger.LogInformation($"new conversation, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new StoredBotData(), conversationData = new StoredBotData(), privateConversationData = new StoredBotData(), values = new List<DarlVar>(), ruleProcessing = new Stack<RuleSetHandler>(), updated = DateTime.UtcNow };
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
                        foreach(var c in responses)
                        {
                            if (c == r)
                                break;
                            if(c.response.dataType != DarlVar.DataType.seek)
                            {
                                _logger.LogInformation($"Emitting text before seek: {c.response.Value}, id= {conversationId}, KGName= {KnowledgeGraphName}, userId = {userId}");
                                resp.Add(c);
                            }
                        }
                        bs.kGraphData = r.response.sequence;
                        bs.pending = null;
                        var res = await _ghandler.GraphPass(userId, KnowledgeGraphName, conversationId, r.response.sequence[0][0], r.response.sequence[1], r.response.sequence[2][0], bs.values, bs.pending);
                        if(!res.Item1.Any())
                        {
                            //no connection found
                        }
                        resp.Add(res.Item1.First());
                        bs.pending = res.Item2;
                    }
                    else
                    {
                        _logger.LogInformation($"top level response, text = {r.response.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                        resp.Add(r);
                    }
                    if (r.response.approximate)
                    {
                        string version = "unknown";
                        _logger.LogInformation($"top level default response, text = {r.response.Value}, KGName= {KnowledgeGraphName}, userId = {userId}");
                        await _conv.CreateDefaultResponse(new DefaultResponse { date = DateTime.UtcNow, model = KnowledgeGraphName, message = conversationData.Value, response = r.response.Value, userId = userId, version = version });
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
                if(responses.Any())
                {
                    var r = responses.Last();
                    if(r.response.name == "response")
                    {
                        resp.Add(r);
                    }
                    else if(r.response.name == "terminate")
                    {
                        bs.kGraphData = null;
                        bs.pending = null;
                        resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Quitting...", dataType = DarlVar.DataType.textual, name = "response"} });
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
                        var res = await _ghandler.GraphPass(userId, KnowledgeGraphName, conversationId, bs.kGraphData[0][0], bs.kGraphData[1], bs.kGraphData[2][0], bs.values, bs.pending);
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
                    catch(Exception ex)
                    {
                        throw new ExecutionError($"Internal Error in GraphPass", ex);
                    }
                }
            }
            await _conv.SaveBotState(bs);
            return resp;

        }
    }
}
