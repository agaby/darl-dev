using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ConversationInputType : InputObjectGraphType<Conversation>
    {
        public ConversationInputType()
        {
            Name = "conversationInput";
            Description = "General details about a bot conversation";
            Field<StringGraphType>("appId");
            Field<StringGraphType>("city");
            Field<StringGraphType>("conversationId");
            Field<StringGraphType>("countryOrRegion");
            Field<StringGraphType>("stateOrProvince");
            Field<IntGraphType>("count");
            Field<DateTimeGraphType>("timestamp");
        }
    }
}
