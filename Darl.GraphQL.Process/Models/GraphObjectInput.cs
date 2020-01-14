using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphObjectInput
    {
        public string name { get; set; }
        public string lineage { get; set; }
        public string firstname { get; set; }//optional
        public string secondname { get; set; }//optional
        public List<DateTime> existence { get; set; }//existence
        public bool inferred { get; set; } = false;
        public List<StringStringPair> properties { get; set; }
    }
}
