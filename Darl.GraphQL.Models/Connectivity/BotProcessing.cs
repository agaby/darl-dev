using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BotProcessing : IBotProcessing
    {
        IConnectivity _conv;
        IFormApi _form;
        IRuleFormInterface _rfi;

        public BotProcessing(IConnectivity conv, IFormApi form, IRuleFormInterface rfi)
        {
            _conv = conv;
            _form = form;
            _rfi = rfi;
        }

        public async Task<List<InteractTestResponse>> InteractAsync(string userId, string botModelName, string conversationId, DarlVar conversationData)
        {
            List<InteractTestResponse> resp = new List<InteractTestResponse>();
            var bm = await _conv.GetLineageModel(userId, botModelName);
            BotState bs = await _conv.GetBotState(userId, conversationId);
            if(bs == null)//first call for this conversation
            {
                bs = new BotState { conversationId = conversationId, userId = userId, userData = new LocalBotData(new Dictionary<string, string>()), conversationData = new LocalBotData(new Dictionary<string, string>()), privateConversationData = new LocalBotData(new Dictionary<string, string>()), values = new List<DarlVar>(), ruleProcessing = new Stack<QuestionSetProxy>() }; 
            }
            if (bs.ruleProcessing.Count == 0) // conversational processing
            {
                var stores = bm.CreateStores(userId, _rfi, bs.values, bs.userData, bs.conversationData, bs.privateConversationData );
                var responses = await bm.InteractTest(conversationData, bs.values, stores);
                if(responses.Any())
                {
                    var r = responses.Last();
                    if(r.response.dataType == DarlVar.DataType.ruleset)
                    {
                        //call the ruleset and stack it
                        var newRF = ((CallStore)stores["Call"]).currentRF;
                        bs.ruleProcessing.Push(await _form.Get(new RuleSet { Contents = newRF, userId = userId }));
                    }
                    else
                    {
                        resp.Add(r);
                    }
                }
                else
                {
                    resp.Add(new InteractTestResponse { response = new DarlVar { Value = "Internal error", dataType = DarlVar.DataType.textual } });
                }
            }
            if (bs.ruleProcessing.Count > 0) //ruleset processing
            {
                //handle simple navigation
                var c = LineageModelBotExtensions.HandleRuleSetCommands(conversationData.Value);
                switch(c)
                {
                    case LineageModelBotExtensions.Commands.quit:
                        break;
                    case LineageModelBotExtensions.Commands.back:
                        break;
                }
                //pass on to ruleset
                //add to resp;
            }
            await _conv.SaveBotState(bs);
            return resp;
        }
    }
}
