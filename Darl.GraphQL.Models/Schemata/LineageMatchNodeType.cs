using Darl.GraphQL.Models.Services;
using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageMatchNodeType : ObjectGraphType<LineageMatchNode>
    {
        public LineageMatchNodeType(ILineageMatchNodePairService pairs
            )
        {
            Field<LineageAnnotationNodeType>("annotation", resolve: context => context.Source.annotation); //
            Field<ListGraphType<LineageMatchNodePairType>>("children", resolve: context => pairs.GetChildrenAsPairs(context.Source.children));//
            Field<LineageElementType>("element", resolve: context => context.Source.element); // nullable
        }
    }
}
