using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class QuestionSetInputType : InputObjectGraphType<QuestionSetInput>
    {
        public QuestionSetInputType()
        {
            Name = "QuestionSetInput";
            Description = "A set of questionnaire responses";
            Field(c => c.ieToken);
            Field<ListGraphType<QuestionInputType>>("questions");
        }
    }
}
