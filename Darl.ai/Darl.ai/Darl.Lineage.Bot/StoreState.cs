/// <summary>
/// StoreState.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Lineage.Bot

{
    public class StoreState
    {
        public string name { get; set; }
        public Dictionary<string, string> states { get; set; } = new Dictionary<string, string>();
    }
}
