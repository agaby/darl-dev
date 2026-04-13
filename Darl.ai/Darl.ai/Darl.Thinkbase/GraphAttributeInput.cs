/// <summary>
/// </summary>

﻿using static Darl.Thinkbase.GraphAttribute;

namespace Darl.Thinkbase
{
    public class GraphAttributeInput : GraphElementInput
    {
        public string value { get; set; }

        public double? confidence { get; set; }

        public DataType type { get; set; }

        public string subLineage { get; set; }

        public bool? inferred { get; set; }

    }
}
