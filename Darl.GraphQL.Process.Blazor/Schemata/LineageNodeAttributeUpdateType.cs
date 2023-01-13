using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageNodeAttributeUpdateType : InputObjectGraphType<LineageNodeAttributeUpdate>
    {
        public LineageNodeAttributeUpdateType()
        {
            Name = "lineageNodeAttributeUpdate";
            Field<StringGraphType>("darl").Description("The darl code fragment to execute");
            Field<BooleanGraphType>("randomResponse").Description("If true one of the random response is selected as an answer, otherwise response is selected");
            Field<StringGraphType>("response").Description("The default response");
            Field<StringGraphType>("call").Description("the ruleset name to call if non-null");
            Field<ListGraphType<StringGraphType>>("accessRoles").Description("Named roles for this response, general access if null");
            Field<ListGraphType<StringGraphType>>("randomResponses").Description("A list of responses, one of which will be selected at random.");
        }
    }
}
