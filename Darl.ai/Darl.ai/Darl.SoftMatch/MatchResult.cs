/// <summary>
/// MatchResult.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.SoftMatch
{
    public class MatchResult
    {
        public string index { get; set; }
        public string sourceText { get; set; }
        public string referenceText { get; set; }
        public double confidence { get; set; } = 0.0;
        public double distance { get; set; } = 0;
        public double matchedWords { get; set; } = 0;

        public Dictionary<string, double> alternatives = new Dictionary<string, double>();

        public int tieCount { get; set; } = 0;
    }
}
