using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Thinkbase
{
    public class VRDisplayNode
    {
        public string id { get; set; }
        public string name { get; set; }
        public string lineage { get; set; }
        public string subLineage { get; set; }
        public string externalId { get; set; }
        public string parent { get; set; }

        public string compositeLineage
        {
            get { return string.IsNullOrEmpty(subLineage) ? lineage : string.IsNullOrEmpty(lineage) ? "" : $"{lineage}_{subLineage}"; }
        }
    }
}
