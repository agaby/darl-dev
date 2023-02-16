namespace Darl.GraphQL.Process.Blazor.Models
{
    public class ModelDetails
    {
        public string version { get; set; } = string.Empty;
        public string author { get; set; } = string.Empty;
        public string copyright { get; set; } = string.Empty;
        public string license { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public double? price { get; set; }
        public string currency { get; set; } = string.Empty;

    }
}
