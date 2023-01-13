using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DarlVarDataTypeEnum : EnumerationGraphType
    {
        public DarlVarDataTypeEnum()
        {
            Name = "darlVarDataTypes";
            Description = "The different types of data represented by a darlVar";
            Add("numeric", 0, "numeric data");
            Add("categorical", 1, "categorical data");
            Add("textual", 2, "textual data");
            Add("sequence", 3, "sequence data");
            Add("date", 4, "date data");
            Add("time", 5, "time data");
            Add("duration", 6, "duration data");
            Add("location", 7, "location data");
            Add("link", 8, "link data");
            Add("image", 9, "image data");
            Add("video", 10, "numeric data");
            Add("credentials", 11, "credentials data");
            Add("name", 12, "name data");
            Add("organization", 13, "organization data");
            Add("payment", 14, "payment data");
            Add("ruleset", 15, "ruleset data");
            Add("complete", 16, "completion data");
        }
    }
}
