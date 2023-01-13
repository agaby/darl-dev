using Darl.SoftMatch;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class KGMatchResult
    {
        public bool index { get; set; } = false;

        public List<string> valueProperty { get; set; } = new List<string>();

        public List<List<MatchResult>> results { get; set; } = new List<List<MatchResult>>();
    }
}
