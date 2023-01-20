namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphObjectResponse
    {
        public GraphObject? getGraphObjectByExternalId { get; set; }
        public GraphObject? createGraphObject { get; set; }
        public GraphObject? updateGraphObject { get; set; }
        public GraphObject? getVirtualObjectByLineage { get; set; }
        public GraphObject? getRecognitionObjectById { get; set; }
        public GraphObject? getGraphObjectById { get; set; }

    }
}
