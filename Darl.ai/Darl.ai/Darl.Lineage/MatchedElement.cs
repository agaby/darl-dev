/// </summary>

﻿using DarlCommon;
using System.Collections.Generic;

namespace Darl.Lineage
{
    public abstract class MatchedElement
    {
        public string path { get; set; }

        public List<DarlVar> values { get; set; }

        public int depth { get; set; }

        public double confidence { get; set; } = 1.0;

    }
}
