using Darl.Lineage.Bot;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class InteractResponseType : ObjectGraphType<InteractTestResponse>
    {
        public InteractResponseType()
        {
            Name = "interactionResponse";
            Description = "The bot's response to your input";
            Field(c => c.darl);
            Field<DarlVarType>("response", resolve:  c => c.Source.response);
            Field<ListGraphType<MatchedAnnotationType>>("matches", resolve: c => c.Source.matches);
        }
    }
}
