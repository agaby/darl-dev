using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ConversationType : ObjectGraphType<Conversation>
    {
        public ConversationType()
        {
            Name = "conversation";
            Description = "General details about a bot conversation";
            Field(c => c.appId).Description("The id of the bot");
            Field(c => c.city).Description("The city of the correspondent");
            Field(c => c.conversationId).Description("The conversation id");
            Field(c => c.count).Description("The number of exchanges");
            Field(c => c.countryOrRegion).Description("Country or region of the correspondent");
            Field(c => c.stateOrProvince).Description("State or province of the correspondent");
            Field(c => c.timestamp).Description("The time of the conversation");
        }
    }
}
