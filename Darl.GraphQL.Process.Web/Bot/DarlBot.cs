/// <summary>
/// DarlBot.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage.Bot;
using DarlCommon;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Bot
{
    public class DarlBot : ActivityHandler
    {
        private readonly IConfiguration _config;
        private readonly IBotProcessing _botProcessing;
        private readonly string botModelName;
        private readonly string botModelUser;

        public DarlBot(IConfiguration config, IBotProcessing botProcessing)
        {
            _config = config;
            _botProcessing = botProcessing;
            botModelName = _config["DarlBot:botModelName"];
            botModelUser = _config["DarlBot:botModelUser"];
        }


        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var responses = await _botProcessing.InteractKGAsync(botModelUser, botModelName, turnContext.Activity.Conversation.Id, new DarlVar { name = "", Value = turnContext.Activity.Text, dataType = DarlVar.DataType.textual });//resp.GetDataFieldAs<List<InteractResponse>>(graph ? "interactKnowledgeGraph" : "interact");

                foreach (var r in responses)
                {
                    Activity m = MessageFactory.Text("internal error");
                    switch (r.response.dataType)
                    {
                        case DarlVar.DataType.categorical:
                            m = MessageFactory.SuggestedActions(r.response.categories.Keys, r.response.Value) as Activity;
                            break;

                        case DarlVar.DataType.link:
                            m = MessageFactory.Text($"[{r.response.Value}]({r.response.Value})");
                            break;

                        default:
                            if (!string.IsNullOrEmpty(r.response.Value))
                            {
                                m = MessageFactory.Text(r.response.Value);
                            }
                            break;
                    }
                    await turnContext.SendActivityAsync(m, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(ex.Message), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(_config["DarlBot:initialText"]), cancellationToken);
                }
            }
        }
    }
}
