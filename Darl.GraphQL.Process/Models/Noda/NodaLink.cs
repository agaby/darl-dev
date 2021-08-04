using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaLinkShapes {Solid, Dash }
    public class NodaLink : NodaElement
    {
        public string title { get; set; }
        public NodaNodeId fromNode { get; set; }
        public NodaNodeId toNode { get; set; }
        public NodaLinkShapes shape { get; set; }
        #region Layout

        public double length { get; set; } = 0.0;

        #endregion
    }
}
