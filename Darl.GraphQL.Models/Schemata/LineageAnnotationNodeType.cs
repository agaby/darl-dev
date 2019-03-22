using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageAnnotationNodeType : ObjectGraphType<LineageAnnotationNode>
    {
        public LineageAnnotationNodeType()
        {
            Field<ListGraphType<StringGraphType>>("accessRoles", resolve: context => context.Source.accessRoles);
            Field<ListGraphType<StringGraphType>>("darl", resolve: context => context.Source.darl);
            Field<ListGraphType<StringGraphType>>("implications", resolve: context => context.Source.implications);
        }

    }
}
