/// <summary>
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public class VRDisplayNode
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;
        public string subLineage { get; set; } = string.Empty;
        public string externalId { get; set; } = string.Empty;
        public string parent { get; set; } = string.Empty;

        public List<VRDisplayAtt> attributes { get; set; } = new List<VRDisplayAtt>();

        public string compositeLineage
        {
            get { return string.IsNullOrEmpty(subLineage) ? lineage : string.IsNullOrEmpty(lineage) ? "" : $"{lineage}_{subLineage}"; }
        }
    }
}
