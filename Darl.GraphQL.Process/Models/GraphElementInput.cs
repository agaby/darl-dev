using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public abstract class GraphElementInput
    {
        public string name { get; set; }
        public string lineage { get; set; }
        public List<DateTime> existence { get; set; }//existence
        public bool inferred { get; set; } = false;
        public List<StringStringPair> properties { get; set; }
    }
}
