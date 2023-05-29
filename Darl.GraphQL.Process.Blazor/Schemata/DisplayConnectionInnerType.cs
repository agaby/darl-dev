using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DisplayConnectionInnerType : ObjectGraphType<DisplayConnection>
    {
        public DisplayConnectionInnerType()
        {
            Field<StringGraphType>("id").Resolve(c => c.Source.id);
            Field<StringGraphType>("label").Resolve(c => c.Source.name);
            Field<StringGraphType>("source").Resolve(c => c.Source.source);
            Field<StringGraphType>("target").Resolve(c => c.Source.target);
            Field<StringGraphType>("lineage").Resolve(c => c.Source.lineage);
        }
    }
}