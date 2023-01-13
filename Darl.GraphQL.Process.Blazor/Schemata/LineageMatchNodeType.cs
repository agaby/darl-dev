using Darl.GraphQL.Process.Blazor.Models;
using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageMatchNodeType : ObjectGraphType<LineageMatchNode>
    {
        public LineageMatchNodeType()
        {
            Name = "LineageMatchNode";
            Description = "A node in a text recognition tree";
            Field<LineageAnnotationNodeType>("annotation").Resolve(context => context.Source.annotation); //
            Field<ListGraphType<LineageMatchNodePairType>>("children").Resolve(context => GetChildrenAsPairs(context.Source.children));//
            Field<LineageElementUnionType>("element").Resolve(context => context.Source.element); // nullable
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
