using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageRecordType : ObjectGraphType<LineageRecord>
    {
        public LineageRecordType()
        {
            Name = "lineageRecord";
            Description = "A concept and it's associated typeWord";
            Field(c => c.description);
            Field(c => c.lineage);
            Field<LineageTypeEnum>("lineageType").Resolve(c => c.Source.type);
            Field(c => c.typeWord);
            Field<ListGraphType<LineageAssociationType>>("follows").Resolve(c => c.Source.follows);
            Field<ListGraphType<LineageAssociationType>>("precedes").Resolve(c => c.Source.precedes);
        }
    }
}
