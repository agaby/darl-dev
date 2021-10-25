using System.Collections.Generic;

namespace Darl.Lineage.Bot

{
    public class StoreState
    {
        public string name { get; set; }
        public Dictionary<string, string> states { get; set; } = new Dictionary<string, string>();
    }
}
