using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage
{
    public class DefaultAnnotation : IComparable<DefaultAnnotation>
    {
        public int depth { get; set; }

        public LineageAnnotationNode node { get; set; }


        public int CompareTo(DefaultAnnotation other)
        {
            return this.depth.CompareTo(other.depth);
        }
    }
}
