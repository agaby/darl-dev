/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public class InferenceRecord
    {
        public double confidence { get; set; } = 0.0;
        public bool unknown = true;
        public GraphObject source { get; set; }
        public List<StringStringPair> recommendations { get; set; }
    }
}