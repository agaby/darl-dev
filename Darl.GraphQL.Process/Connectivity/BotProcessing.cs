using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using GraphQL;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
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
        ITrigger _trigger;
        private TelemetryClient _telemetry;
        private IConfiguration _config;


        public BotProcessing(IConnectivity conv, IFormApi form, IRuleFormInterface rfi, ITrigger trigger, TelemetryClient telemetry, IConfiguration config)
        {
            _conv = conv;
            _form = form;
            _rfi = rfi;
            _trigger = trigger;
            _telemetry = telemetry;
            _config = config;
        }

        public async Task<List<InteractTestResponse>> InteractAsync(string userId, string botModelName, string conversationId, DarlVar conversationData)
        {
            _telemetry.TrackEvent($"InteractAsync", new Dictionary<string, string> { { nameof(userId), userId }, { nameof(botModelName), botModelName },{nameof(conversationId),conversationId} , {nameof(conversationData), conversationData.Value } });
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
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new LocalBotData(new Dictionary<string, string>()), conversationData = new LocalBotData(new Dictionary<string, string>()), privateConversationData = new LocalBotData(new Dictionary<string, string>()), values = new List<DarlVar>(), ruleProcessing = new Stack<RuleSetHandler>() }; 
            }
            var stores = bm.CreateStores(userId, _rfi, bs.values, bs.userData, bs.conversationData, bs.privateConversationData );
            //add extra stores defined locally
            var botFormat = JsonConvert.DeserializeObject<BotFormat>(bm.form);
            if(botFormat.Stores.Contains("Graph"))
            {
                stores.Add("Graph", new GraphProcessing(_config, _telemetry));
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
            if (bs.ruleProcessing.Count > 0) //ruleset processing
            {
                var rsh = bs.ruleProcessing.Peek();
                rsh.trigger = _trigger;
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
                var responses = await rsh.RuleSetPass(bs.values, stores, bmt.serviceConnectivity);
                //add to resp;
                foreach (var r in responses)
                {
                    switch (r.response.dataType)
                    {
                        case DarlVar.DataType.ruleset:
                            resp.Add(r);
                            var newRF = ((CallStore)stores["Call"]).currentRF; if (newRF != null)
                                bs.ruleProcessing.Push(new RuleSetHandler { user = userId, rf = newRF, modelId = conversationId });
                            break;
                        case DarlVar.DataType.complete:
                            //clear out values etc here
                            bs.values.Clear();
                            bs.ruleProcessing.Pop();
                            break;
                        default:
                            resp.Add(r);
                            break;
                    }
                }
            }
            await _conv.SaveBotState(bs);
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
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new LocalBotData(new Dictionary<string, string>()), conversationData = new LocalBotData(new Dictionary<string, string>()), privateConversationData = new LocalBotData(new Dictionary<string, string>()), values = new List<DarlVar>(), ruleProcessing = new Stack<RuleSetHandler>() };
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
    }
}
