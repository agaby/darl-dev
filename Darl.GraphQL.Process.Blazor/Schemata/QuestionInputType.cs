using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class QuestionInputType : InputObjectGraphType<QuestionInput>
    {
        public QuestionInputType()
        {
            Name = "QuestionInput";
            Description = "One response from a questionnaire user";
            Field(c => c.dResponse, true).Description("The value if numeric");
            Field(c => c.reference).Description("The reference of the question asked");
            Field(c => c.sResponse, true).Description("The value if not numeric");
            Field<QuestionTypeEnum>("qType").Description("The type of the question").Resolve(c => c.Source.qType);
        }
    }
}
