using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageMatchNodePairType : ObjectGraphType<LineageMatchNodePair>
    {
        public LineageMatchNodePairType()
        {
            Name = "LineageMatchNodePair";
            Description = "Pair of text and LineageMatchNode";
            Field(c => c.Text);
            Field<LineageMatchNodeType>("Match", resolve: c => c.Source.Match);
        }
    }
}
