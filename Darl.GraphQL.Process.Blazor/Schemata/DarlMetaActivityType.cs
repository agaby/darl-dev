using Darl.Thinkbase.Meta;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DarlMetaActivityType : ObjectGraphType<DarlMetaActivity>
    {
        public DarlMetaActivityType()
        {
            Name = "darlMetaActivity";
            Description = "Holds the active ruleset information for the last evaluation.";
            Field<ListGraphType<DarlMetaActiveNodeType>>("activeNodes").Resolve(c => c.Source.activeNodes);
        }
    }
}
