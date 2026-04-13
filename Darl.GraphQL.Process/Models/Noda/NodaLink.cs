/// <summary>
/// NodaLink.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Process.Web.Models.Noda;
using System.Text.Json.Serialization;

namespace Darl.GraphQL.Models.Models.Noda
{
    public enum NodaLinkShapes { Solid, Dash }
    public class NodaLink : NodaElement, ILayoutLink
    {
        public NodaNodeId fromNode { get; set; }
        public NodaNodeId toNode { get; set; }
        public NodaLinkShapes shape { get; set; }
        #region Layout
        [JsonIgnore]
        public double length { get; set; } = 0.0;

        public string FromNode()
        {
            return fromNode.Uuid;
        }

        public string ToNode()
        {
            return toNode.Uuid;
        }

        #endregion
    }
}
