/// <summary>
/// TriggerEvent.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace DarlCommon
{
    public class TriggerEvent
    {
        public string tenant { get; set; }
        public string sourceId { get; set; }
        public List<DarlVar> data { get; set; }
        public int darlPoints { get; set; }
    }
}
