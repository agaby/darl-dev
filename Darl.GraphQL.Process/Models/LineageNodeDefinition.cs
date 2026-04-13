/// <summary>
/// LineageNodeDefinition.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{
    //Definition returned from GetChildren search in tree editing
    public class LineageNodeDefinition
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool children { get; set; }
        public string icon { get; set; }
        public string type { get; set; }
        public LineageNodeAttributes attributes { get; set; }

    }
}
