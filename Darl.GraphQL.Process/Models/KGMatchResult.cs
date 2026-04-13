/// <summary>
/// KGMatchResult.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.SoftMatch;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class KGMatchResult
    {
        public bool index { get; set; } = false;

        public List<string> valueProperty { get; set; } = new List<string>();

        public List<List<MatchResult>> results { get; set; } = new List<List<MatchResult>>();
    }
}
