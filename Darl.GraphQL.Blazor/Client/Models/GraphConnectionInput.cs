using System.Collections.Generic;

namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphConnectionInput
    {
        public List<DarlTimeInput>? existence { get; set; }
        public string lineage { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string startId { get; set; } = string.Empty;
        public string endId { get; set; } = string.Empty;
        double weight { get; set; } = 1.0;
    }
}

