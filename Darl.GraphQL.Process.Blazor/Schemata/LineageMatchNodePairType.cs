using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageMatchNodePairType : ObjectGraphType<LineageMatchNodePair>
    {
        public LineageMatchNodePairType()
        {
            Name = "LineageMatchNodePair";
            Description = "Pair of text and LineageMatchNode";
            Field(c => c.Text);
            Field<LineageMatchNodeType>("Match").Resolve(c => c.Source.Match);
        }
    }
}
