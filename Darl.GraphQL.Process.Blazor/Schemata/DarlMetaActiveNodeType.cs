using Darl.Thinkbase.Meta;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DarlMetaActiveNodeType : ObjectGraphType<DarlMetaActiveNode>
    {
        public DarlMetaActiveNodeType()
        {
            Name = "darlMetaActivityNode";
            Description = "The location and weight associated with a ruleset node";
            Field(c => c.weight);
            Field(c => c.name);
            Field<SourceSpanType>("span").Resolve(c => c.Source.location);
        }
    }
}
