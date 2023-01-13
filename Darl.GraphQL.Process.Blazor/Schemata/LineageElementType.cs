using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageElementType : ObjectGraphType<LineageElement>
    {
        public LineageElementType()
        {
            Name = "LineageElement";
            Description = "A node of a hypernymy tree";
            Field(c => c.description);
            Field(c => c.lineage);
            Field<LineageTypeEnum>("lineageType").Resolve(c => c.Source.type);
        }
    }
}
