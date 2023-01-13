using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class QuestionTypeEnum : EnumerationGraphType
    {
        public QuestionTypeEnum()
        {
            Name = "QuestionType";
            Description = "The data type sought.";
            Add("numeric", 0, "a number");
            Add("categorical", 1, "text from a range of texts");
            Add("textual", 2, "Free form text");
            Add("temporal", 3, "Free form text");
        }
    }
}
