/// <summary>
/// </summary>

﻿using Darl.Common;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public abstract class GraphElementInput
    {
        public string name { get; set; } = string.Empty;
        public string lineage { get; set; } = string.Empty;
        public List<DarlTime?>? existence { get; set; }//existence
        public List<GraphAttributeInput>? properties { get; set; }

    }
}
