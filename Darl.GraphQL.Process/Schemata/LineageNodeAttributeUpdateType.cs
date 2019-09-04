using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageNodeAttributeUpdateType : InputObjectGraphType<LineageNodeAttributeUpdate>
    {
        public LineageNodeAttributeUpdateType()
        {
            Name = "lineageNodeAttributeUpdate";
            Field<StringGraphType>("darl", "The darl code fragment to execute");
            Field<BooleanGraphType>("randomResponse","If true one of the random response is selected as an answer, otherwise response is selected");
            Field<StringGraphType>("response","The default response");
            Field<StringGraphType>("call", "the ruleset name to call if non-null");
            Field<ListGraphType<StringGraphType>>("accessRoles", "Named roles for this response, general access if null");
            Field<ListGraphType<StringGraphType>>("randomResponses","A list of responses, one of which will be selected at random.");
        }
    }
}
