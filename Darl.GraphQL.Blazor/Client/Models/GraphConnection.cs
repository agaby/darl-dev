using System;

namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphConnection : GraphElement
    {
        public double weight { get; set; }
        public string startId { get; set; } = string.Empty;
        public string endId { get; set; } = string.Empty;
    }
}
