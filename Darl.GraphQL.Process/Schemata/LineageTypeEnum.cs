using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageTypeEnum : EnumerationGraphType
    {
        public LineageTypeEnum()
        {
            Name = "lineageTypes";
            AddValue("CONCEPT", "A concept", 0);
            AddValue("REFERENCE", "A reference", 1);
            AddValue("VALUE", "A value", 2);
            AddValue("LITERAL", "A literal", 3);
            AddValue("DEFAULT", "A default", 4);
            AddValue("COMPOSITE", "A composite type", 5);
        }
    }
}
