using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using GraphQL.Types;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageMatchNodeType : ObjectGraphType<LineageMatchNode>
    {
        public LineageMatchNodeType()
        {
            Name = "LineageMatchNode";
            Description = "A node in a text recognition tree";
            Field<LineageAnnotationNodeType>("annotation", resolve: context => context.Source.annotation); //
            Field<ListGraphType<LineageMatchNodePairType>>("children", resolve: context => GetChildrenAsPairs(context.Source.children));//
            Field<LineageElementUnionType>("element", resolve: context => context.Source.element); // nullable
        }

        private List<LineageMatchNodePair> GetChildrenAsPairs(SortedList<string, LineageMatchNode> children)
        {
            var list = new List<LineageMatchNodePair>();
            foreach (var k in children.Keys)
            {
                list.Add(new LineageMatchNodePair(k, children[k]));
            }
            return list;
        }
    }
}
