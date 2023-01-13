using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageAssociationType : ObjectGraphType<LineageAssociation>
    {
        public LineageAssociationType()
        {
            Name = "lineageAssociation";
            Description = "The weighted association between two lineages";
            Field(c => c.weight);
            Field<LineageRecordType>("start").Resolve(c => c.Source.start);
            Field<LineageRecordType>("end").Resolve(c => c.Source.end);
        }
    }
}
