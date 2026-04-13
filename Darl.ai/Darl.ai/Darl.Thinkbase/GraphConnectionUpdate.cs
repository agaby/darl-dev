/// <summary>
/// GraphConnectionUpdate.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.Thinkbase
{
    public class GraphConnectionUpdate : GraphConnectionInput
    {
        public string id { get; set; } = string.Empty;

        public bool? inferred { get; set; }

    }
}
