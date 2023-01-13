using Darl.Lineage.Bot;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class InteractResponseType : ObjectGraphType<InteractTestResponse>
    {
        public InteractResponseType()
        {
            Name = "interactionResponse";
            Description = "The bot's response to your input";
            Field(c => c.darl, true);
            Field(c => c.reference, true);
            Field<DarlVarType>("response").Resolve(c => c.Source.response);
            Field<ListGraphType<MatchedAnnotationType>>("matches").Resolve(c => c.Source.matches);
            Field<ListGraphType<StringGraphType>>("activeNodes").Resolve(c => c.Source.activeNodes);
            Field<DarlMetaActivityType>("codeActivity").Resolve(c => c.Source.codeActivity);
        }
    }
}
