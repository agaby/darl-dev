using Darl.Thinkbase;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class InferenceRecord
    {
        public double confidence { get; set; } = 0.0;
        public bool unknown = true;
        public GraphObject source { get; set; }
        public List<StringStringPair> recommendations { get; set; }
    }
}