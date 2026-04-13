/// </summary>

﻿namespace Darl.Thinkbase
{
    public class DisplayObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string lineage { get; set; }
        public string subLineage { get; set; }
        public string externalId { get; set; }
        public string parent { get; set; }
        public bool hasCode { get; set; } = false;
    }
}
