using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DisplayObjectOuterType : ObjectGraphType<DisplayObject>
    {
        public DisplayObjectOuterType()
        {
            Name = "displayObject";
            Description = "A display representation of a knowledge graph object";
            Field<DisplayObjectInnerType>("data").Resolve(context => context.Source);
            Field<BooleanGraphType>("selectable").Resolve(context => true);
            Field<BooleanGraphType>("grabbable").Resolve(context => true);
        }
    }
}