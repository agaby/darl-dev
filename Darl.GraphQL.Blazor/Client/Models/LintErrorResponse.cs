namespace Darl.GraphQL.Blazor.Client.Models
{
    public class LintErrorResponse
    {
        public List<DarlLintError> lintDarlMeta { get; set; }   = new List<DarlLintError>();
    }
}
