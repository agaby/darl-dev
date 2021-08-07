using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaNodeShapes { Ball, Box, Hourglass};
    public class NodaNode : NodaElement
    {        public NodaFacing facing { get; set; } = new NodaFacing();
        public NodaPosition position { get; set; } = NodaPosition.Random();
        public NodaNodeShapes shape { get; set; }
        public bool collapsed { get; set; } = false;

    }
}
