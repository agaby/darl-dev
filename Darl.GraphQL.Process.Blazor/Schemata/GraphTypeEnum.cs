using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphTypeEnum : EnumerationGraphType<GraphElementType>
    {
        public GraphTypeEnum()
        {
            Name = "graphElementTypes";
            Description = "The kinds of graph elements";
        }
    }
}
