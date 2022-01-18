using Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Models
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
