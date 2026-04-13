/// <summary>
/// MatchedGraphAttribute.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using System;

namespace Darl.Thinkbase
{
    public class MatchedGraphAttribute : MatchedElement, IComparable<MatchedGraphAttribute>
    {
        public GraphAttribute terminus { get; set; }

        public int CompareTo(MatchedGraphAttribute other)
        {
            var c = confidence.CompareTo(other.confidence);
            if (c != 0)
                return c;
            return depth.CompareTo(other.depth);
        }
    }
}
