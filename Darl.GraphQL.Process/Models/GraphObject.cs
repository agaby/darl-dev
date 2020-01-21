using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string lineage { get; set; }
        public string userId { get; set; } //also used for sharding
        public string firstname { get; set; }//optional
        public string secondname { get; set; }//optional
        public List<DateTime> existence { get; set; }//existence
        public bool inferred { get; set; } = false;
        public List<GraphConnection> connections { get; set; } = new List<GraphConnection>();
        public List<StringStringPair> properties { get; set; } = new List<StringStringPair>();
    }
}
