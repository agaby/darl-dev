using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Thinkbase
{
    public class SalienceRecord : IComparable<SalienceRecord>
    {
        public GraphAbstraction gobj { get; set; }

        public GraphAttribute att { get; set; }

        public double salience { get; set; } = 0.0;

        public int CompareTo(SalienceRecord other)
        {
            return this.salience.CompareTo(other.salience);
        }

        public override bool Equals(object obj)
        {
            if(obj is SalienceRecord)
            {
                var o = obj as SalienceRecord;
                return (o.gobj == gobj && o.att == att);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"";
        }
    }
}
