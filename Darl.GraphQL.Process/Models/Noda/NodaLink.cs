using System.Text.Json.Serialization;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaLinkShapes { Solid, Dash }
    public class NodaLink : NodaElement
    {
        public NodaNodeId fromNode { get; set; }
        public NodaNodeId toNode { get; set; }
        public NodaLinkShapes shape { get; set; }
        #region Layout
        [JsonIgnore]
        public double length { get; set; } = 0.0;

        #endregion
    }
}
