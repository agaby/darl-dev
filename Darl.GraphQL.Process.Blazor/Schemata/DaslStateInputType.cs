using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DaslStateInputType : InputObjectGraphType<DaslState>
    {

        public DaslStateInputType()
        {
            Name = "daslStateInput";
            Description = "The state of a system at a particular time";
            Field<DateTimeGraphType>("timeStamp").Description("The point in time of this state");
            Field<ListGraphType<DarlVarInputType>>("values").Description("The set of values changed at this state").Resolve(context => context.Source.values);

        }
    }
}
