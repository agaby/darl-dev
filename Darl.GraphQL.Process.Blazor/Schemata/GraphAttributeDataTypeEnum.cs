using GraphQL.Types;
using static Darl.Thinkbase.GraphAttribute;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphAttributeDataTypeEnum : EnumerationGraphType<DataType>
    {
        public GraphAttributeDataTypeEnum()
        {
            Name = "dataTypes";
            Description = "The data types of an attribute.";
        }
    }
}
