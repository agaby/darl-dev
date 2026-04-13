/// <summary>
/// NodaNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Process.Web.Models.Noda;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaNodeShapes { Ball, Box, Tetra, Hourglass, Cylinder, Diamond, Plus, Star };
    public class NodaNode : NodaElement, ILayoutNode
    {
        public NodaFacing facing { get; set; } = new NodaFacing();
        public NodaPosition position { get; set; } = NodaPosition.Random();
        public NodaNodeShapes shape { get; set; }
        public bool collapsed { get; set; } = false;

    }
}
