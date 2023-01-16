using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphElement
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;

        public List<DarlTime>? existence { get; set; }
        public bool inferred { get; set; }
        public bool? _virtual { get; set; }
        public List<GraphAttribute>? properties { get; set; }
        public string dynamicSource { get; set; } = string.Empty;


    }
}
