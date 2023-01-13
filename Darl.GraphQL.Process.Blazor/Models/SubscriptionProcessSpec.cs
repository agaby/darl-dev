using Darl.Thinkbase;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class SubscriptionProcessSpec
    {
        public IObservable<KnowledgeState> ks { get; set; }
        public string graphName { get; set; } = string.Empty;
        public string target { get; set; } = string.Empty;
        public GraphProcess process { get; set; } = GraphProcess.seek;
        public string userId { get; set; } = string.Empty;
        public string compositeName { get { return userId + "_" + graphName; } }
    }
}
