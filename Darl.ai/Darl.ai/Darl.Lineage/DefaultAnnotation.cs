/// </summary>

﻿using System;

namespace Darl.Lineage
{
    public class DefaultAnnotation : IComparable<DefaultAnnotation>
    {
        public int Depth { get; set; }

        public LineageAnnotationNode Node { get; set; }

        public string path { get; set; }


        public int CompareTo(DefaultAnnotation other)
        {
            return Depth.CompareTo(other.Depth);
        }
    }
}
