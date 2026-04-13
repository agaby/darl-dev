/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using Newtonsoft.Json;
using System;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public class NodaViewNodeProps : ILayoutNode
    {
        public enum NodaNodeShapes { Ball, Box, Tetra, Hourglass, Cylinder, Diamond, Plus, Star };
        public string uuid { get; set; } = Guid.NewGuid().ToString();
        public string title { get; set; } = String.Empty;
        public string color { get; set; } = "#FF0000";
        public double opacity { get; set; } = 1.0;// 0-1
        public NodaNodeShapes shape { get; set; } = NodaNodeShapes.Ball;
        public string imageUrl { get; set; } = String.Empty;
        public string notes { get; set; } = String.Empty;
        public string pageUrl { get; set; } = String.Empty;
        public int size { get; set; } = 5; //range 1 - 9
        public NodaViewNodeLocation location { get; set; } = new NodaViewNodeLocation { relativeTo = NodaViewNodeLocation.RelativeTo.Window, x = 0, y = 0, z = 0 };
        public bool selected { get; set; } = false;
        [JsonIgnore]
        public NodaPosition position { get { return location.position; } set { location.position = value; } }

    }
}
