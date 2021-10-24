using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlVarDataTypeEnum : EnumerationGraphType
    {
        public DarlVarDataTypeEnum()
        {
            Name = "darlVarDataTypes";
            Description = "The different types of data represented by a darlVar";
            AddValue("numeric", "numeric data", 0);
            AddValue("categorical", "categorical data", 1);
            AddValue("textual", "textual data", 2);
            AddValue("sequence", "sequence data", 3);
            AddValue("date", "date data", 4);
            AddValue("time", "time data", 5);
            AddValue("duration", "duration data", 6);
            AddValue("location", "location data", 7);
            AddValue("link", "link data", 8);
            AddValue("image", "image data", 9);
            AddValue("video", "numeric data", 10);
            AddValue("credentials", "credentials data", 11);
            AddValue("name", "name data", 12);
            AddValue("organization", "organization data", 13);
            AddValue("payment", "payment data", 14);
            AddValue("ruleset", "ruleset data", 15);
            AddValue("complete", "completion data", 16);
        }
    }
}
