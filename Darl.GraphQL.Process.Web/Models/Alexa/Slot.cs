/// <summary>
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.GraphQL.Process.Models.Alexa
{
    public class Slot
    {
        public string name { get; set; }

        public string type { get; set; }

        public List<string> samples { get; set; } = new List<string>();
    }
}
