using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class LineageElementUnionType : UnionGraphType
    {
        public LineageElementUnionType()
        {
            Type<LineageElementType>();
            Type<LineageRecordType>();
        }
    }
}
