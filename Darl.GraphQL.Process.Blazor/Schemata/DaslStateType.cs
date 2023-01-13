using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DaslStateType : ObjectGraphType<DaslState>
    {

        public DaslStateType()
        {
            Name = "daslState";
            Description = "The state of a system at a particular time";
            Field(c => c.timeStamp).Description("The point in time of this state");
            Field<ListGraphType<DarlVarType>>("values").Description("The set of values changed at this state").Resolve(context => context.Source.values);
        }
    }
}
