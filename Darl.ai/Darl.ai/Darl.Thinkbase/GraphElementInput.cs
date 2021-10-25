using Darl.Common;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public abstract class GraphElementInput
    {
        public string name { get; set; }
        public string lineage { get; set; }
        public List<DarlTime?> existence { get; set; }//existence

    }
}
