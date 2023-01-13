using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DisplayConnectionOuterType : ObjectGraphType<DisplayConnection>
    {
        public DisplayConnectionOuterType()
        {
            Name = "displayConnection";
            Description = "A display representation of a knowledge graph connection";
            Field<DisplayConnectionInnerType>("data").Resolve(context => context.Source);
            Field<BooleanGraphType>("selectable").Resolve(context => true);
            Field<BooleanGraphType>("grabbable").Resolve(context => true);
        }
    }
}