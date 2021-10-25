using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
