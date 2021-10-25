using System.Collections.Generic;

namespace Darl.Lineage.Bot
{
    public class BotFragment
    {
        public string Response { get; set; }

        public List<string> RandomResponses { get; set; } = new List<string>();

        public string CallRuleset { get; set; }
    }
}
