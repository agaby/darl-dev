namespace Darl.GraphQL.Blazor.Client.Models
{
    public class DisplayObject
    {
        public string id { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string lineage { get; set; } = string.Empty;

        public string subLineage { get; set; } = string.Empty;

        public string externalId { get; set; } = string.Empty;

        public string parent { get; set; } = string.Empty;

        public bool hasCode { get; set; }
    }
}
