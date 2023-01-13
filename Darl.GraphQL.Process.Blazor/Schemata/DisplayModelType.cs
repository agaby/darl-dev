using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DisplayModelType : ObjectGraphType<DisplayModel>
    {
        public DisplayModelType()
        {
            Name = "displayModel";
            Description = "A simplified version of a knowledge graph for display purposes";
            Field<ListGraphType<DisplayObjectOuterType>>("nodes").Resolve(context => context.Source.nodes);
            Field<ListGraphType<DisplayConnectionOuterType>>("edges").Resolve(context => context.Source.edges);
        }
    }
}
