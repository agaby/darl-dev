/// <summary>
/// GraphObjectInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public class GraphObjectInput : GraphElementInput
    {
        public string externalId { get; set; }//optional

        public string subLineage { get; set; }

        public List<GraphAttributeInput> properties { get; set; } = new List<GraphAttributeInput>();

    }
}
