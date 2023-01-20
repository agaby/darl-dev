namespace Darl.GraphQL.Blazor.Client.Models
{
    public class GraphModel
    {
        public enum DateDisplay { RECENT, HISTORIC}
        public enum InferenceTime { RECENT, HISTORIC }
        public string? author { get; set; }
        public string? copyright { get; set; }
        public DateDisplay? dateDisplay { get; set; }
        public string? description { get; set; }
        public DarlTime? fixedTime { get; set; }
        public InferenceTime? inferenceTime { get; set; }
        public string? initialText { get; set; }
        public string? licenseUrl { get; set; }
        public string? defaultTarget { get; set;}
        public bool? transient { get; set; }

    }
}
