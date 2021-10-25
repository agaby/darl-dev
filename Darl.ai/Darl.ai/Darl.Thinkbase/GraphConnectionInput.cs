using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public class GraphConnectionInput : GraphElementInput
    {
        public double? weight { get; set; }
        public string startId { get; set; }
        public string endId { get; set; }
        public string id { get; set; } //Cytoscape presets ids of new connections...
        public List<GraphAttribute> properties { get; set; } = new List<GraphAttribute>();


    }
}
