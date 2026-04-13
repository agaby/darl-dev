/// <summary>
/// DarlMetaActivity.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DarlMetaActivity
    {
        public List<DarlMetaActiveNode> activeNodes {get; set;} = new List<DarlMetaActiveNode>();
    }

    public class DarlMetaActiveNode
    {
        public double weight { get; set; } = 1.0;
        public SourceSpan location { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
