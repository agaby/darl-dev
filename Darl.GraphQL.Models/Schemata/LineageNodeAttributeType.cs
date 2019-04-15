using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageNodeAttributeType : ObjectGraphType<LineageNodeAttributes>
    {
        public LineageNodeAttributeType()
        {
            Name = "lineageNodeAttributes";
            Field(c => c.darl);
            Field(c => c.call);
//            Field(c => c.path);
            Field(c => c.randomResponse);
            Field(c => c.response);
            Field<ListGraphType<StringGraphType>>("accessRoles", resolve: context => context.Source.accessRoles);
            Field<ListGraphType<StringGraphType>>("implications", resolve: context => context.Source.implications);
            Field<ListGraphType<StringGraphType>>("randomResponses", resolve: context => context.Source.randomResponses);
        }
    }
}
