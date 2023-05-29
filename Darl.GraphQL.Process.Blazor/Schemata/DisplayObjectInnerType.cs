using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DisplayObjectInnerType : ObjectGraphType<DisplayObject>
    {
        public DisplayObjectInnerType()
        {
            Field<StringGraphType>("id").Resolve(c => c.Source.id);
            Field<StringGraphType>("label").Resolve(c => c.Source.name);
            Field<StringGraphType>("lineage").Resolve(c => c.Source.lineage);
            Field<StringGraphType>("sublineage").Resolve(c => c.Source.subLineage);
            Field<StringGraphType>("externalId").Resolve(c => c.Source.externalId);
            Field<StringGraphType>("parent").Resolve(c => c.Source.parent);
            Field<BooleanGraphType>("hasCode").Resolve(c => c.Source.hasCode);
        }
    }
}