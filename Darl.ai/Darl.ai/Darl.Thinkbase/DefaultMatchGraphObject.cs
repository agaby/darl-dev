/// <summary>
/// DefaultMatchGraphObject.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;

namespace Darl.Thinkbase
{
    public class DefaultMatchGraphAttribute : IComparable<DefaultMatchGraphAttribute>
    {
        public int Depth { get; set; }

        public GraphAttribute Att { get; set; }

        public string path { get; set; }


        public int CompareTo(DefaultMatchGraphAttribute other)
        {
            return Depth.CompareTo(other.Depth);
        }
    }
}
