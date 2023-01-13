using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class PostTypeEnum : EnumerationGraphType
    {
        public PostTypeEnum()
        {
            Name = "postTypes";
            Add("DARLVARLIST", 0, "pass a list of DarlVar objects");
            Add("FORM", 1, "Use a POST form of name value pairs");
        }
    }
}
