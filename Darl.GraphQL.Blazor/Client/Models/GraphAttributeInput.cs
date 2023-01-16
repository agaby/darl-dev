using System;
using System.Collections.Generic;
using static Darl.GraphQL.Blazor.Client.Models.GraphAttribute;

namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphAttributeInput
    {
        public string value { get; set; } = string.Empty;
        public double? confidence { get; set; }
        public DataType type { get; set; }
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;
        public string subLineage { get; set; } = string.Empty;
        public List<DarlTime>? existence { get; set; }//existence
        public bool? inferred { get; set; }
        public List<GraphAttributeInput>? properties { get; set; }

    }
}
