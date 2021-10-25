using System;

namespace Darl.Lineage
{
    public class MatchedAnnotation : MatchedElement, IComparable<MatchedAnnotation>
    {
        public LineageAnnotationNode annotation { get; set; }

        public int CompareTo(MatchedAnnotation other)
        {
            var c = confidence.CompareTo(other.confidence);
            if (c != 0)
                return c;
            return depth.CompareTo(other.depth);
        }
    }
}
