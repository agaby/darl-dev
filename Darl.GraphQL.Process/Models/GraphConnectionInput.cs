using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphConnectionInput
    {
        public string name { get; set; }
        public string lineage { get; set; }
        public List<DateTime> existence { get; set; }
        public bool inferred { get; set; } = false;
        public double weight { get; set; } = 1.0;
        public string startId { get; set; }
        public string endId { get; set; }
        public List<StringStringPair> properties { get; set; }
    }
}
