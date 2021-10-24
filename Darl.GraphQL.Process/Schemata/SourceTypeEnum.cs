using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SourceTypeEnum : EnumerationGraphType
    {
        public SourceTypeEnum()
        {
            Name = "sourceTypes";
            AddValue("RESULTS", "Get the value from a result in the ruleset output", 0);
            AddValue("FIXEDVALUE", "The values is determined at design time", 1);
        }
    }
}
