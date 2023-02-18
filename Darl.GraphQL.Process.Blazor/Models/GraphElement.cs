using Darl.Thinkbase;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public abstract class GraphElement
    {
        public enum partitionType { dreaming, reality }
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;
        public List<DateTime>? existence { get; set; }
        public bool inferred { get; set; } = false;
        public bool? _virtual { get; set; }
        public List<StringStringPair>? properties { get; set; }
        public string partition { get { return _virtual ?? false ? partitionType.dreaming.ToString() : partitionType.reality.ToString(); } }
    }
}
