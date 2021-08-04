using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaNodeShapes { Ball, Box};
    public class NodaNode : NodaElement
    {
        public string name { get; set; }
        public NodaFacing facing { get; set; }
        public NodaPosition position { get; set; }
        public NodaNodeShapes shape { get; set; }
        public bool collapsed { get; set; } = false;


        #region Layout
        public bool Pinned { get; set; } = false;

        public double mass { get; set; } = 1.0;

        #endregion

    }
}
