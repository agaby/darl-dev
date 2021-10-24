namespace Darl.GraphQL.Models.Models
{
    public class InputFormatUpdate
    {
        public double? Increment { get; set; }
        public int? MaxLength { get; set; }
        public double? NumericMax { get; set; }
        public double? NumericMin { get; set; }
        public string Regex { get; set; }
        public bool? ShowSets { get; set; }
        public bool? EnforceCrisp { get; set; }
        public string path { get; set; }
    }
}
