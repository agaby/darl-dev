using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputTypeEnum : EnumerationGraphType
    {
        public OutputTypeEnum()
        {
            Name = "outputTypes";
            AddValue("NUMERIC", "Output is numeric", 0);
            AddValue("CATEGORICAL", "Output is categorical", 1);
            AddValue("TEXTUAL", "Output is textual", 2);
            AddValue("TEMPORAL", "Output is temporal", 3);
        }
    }
}
