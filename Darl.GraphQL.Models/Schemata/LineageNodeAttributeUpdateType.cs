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
            Field<NonNullGraphType<StringGraphType>>("darl");
            Field<NonNullGraphType<BooleanGraphType>>("randomResponse");
            Field<NonNullGraphType<StringGraphType>>("response");
            Field<NonNullGraphType<StringGraphType>>("call");
            Field<NonNullGraphType<BooleanGraphType>>("present");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("accessRoles");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("implications");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("randomResponses");
        }
    }
}
