namespace Darl.GraphQL.Process.Blazor.Models
{
    public class QuestionSetInput
    {
        public List<QuestionInput> questions { get; set; } = new List<QuestionInput>();

        public string ieToken { get; set; } = string.Empty;

    }
}
