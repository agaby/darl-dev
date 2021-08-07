using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaNodeShapes { Ball, Box, Hourglass};
    public class NodaNode : NodaElement
    {
        public string name { get; set; } = "";
        public NodaFacing facing { get; set; } = new NodaFacing();
        public NodaPosition position { get; set; } = NodaPosition.Random();
        public NodaNodeShapes shape { get; set; }
        public bool collapsed { get; set; } = false;


        #region Layout
        [JsonIgnore]
        public bool Pinned { get; set; } = false;
        [JsonIgnore]
        public double mass { get; set; } = 1.0;

        #endregion

    }
}
