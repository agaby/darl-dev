
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlCommon
{
    public class TriggerEvent
    {
        public string tenant { get; set; }
        public string sourceId { get; set; }
        public List<DarlVar> data { get; set; }
        public int darlPoints { get; set; }
    }
}
