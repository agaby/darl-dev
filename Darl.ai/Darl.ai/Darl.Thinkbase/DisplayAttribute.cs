using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    public class DisplayAttribute
    {
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public double confidence { get; set; } = 1.0;
    }
}
