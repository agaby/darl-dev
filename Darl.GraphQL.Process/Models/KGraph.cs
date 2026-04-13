/// <summary>
/// KGraph.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{

    public class KGraph
    {
        public string Name { get; set; }

        public string userId { get; set; }

        public bool Shared { get; set; } = false;

        public string OwnerId { get; set; }

        public bool? ReadOnly { get; set; } = false;

        public bool? hidden { get; set; } = false;

    }
}
