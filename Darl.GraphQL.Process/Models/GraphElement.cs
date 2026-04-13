/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public abstract class GraphElement
    {
        public enum partitionType { dreaming, reality }
        public string id { get; set; }
        public string name { get; set; }
        public string lineage { get; set; }
        public List<DateTime> existence { get; set; }
        public bool inferred { get; set; } = false;
        public bool? _virtual { get; set; }
        public List<StringStringPair> properties { get; set; }
        public string partition { get { return _virtual ?? false ? partitionType.dreaming.ToString() : partitionType.reality.ToString(); } }
    }
}
