/// <summary>
/// SalienceRecord.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;

namespace Darl.Thinkbase
{
    public class SalienceRecord : IComparable<SalienceRecord>, IEquatable<SalienceRecord>
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
            if (obj is SalienceRecord)
            {
                var o = obj as SalienceRecord;
                if (o.gobj is GraphObject && gobj is GraphObject)
                    return (((GraphObject)o.gobj).id == ((GraphObject)gobj).id && o.att.lineage == att.lineage);
                else
                    return (o.gobj == gobj && o.att.lineage == att.lineage);
            }
            return false;
        }

        public bool Equals(SalienceRecord? other)
        {
            if (other?.gobj is GraphObject && gobj is GraphObject)
                return (((GraphObject)other.gobj).id == ((GraphObject)gobj).id && other.att.lineage == att.lineage);
            else
                return (other?.gobj == gobj && other.att.lineage == att.lineage);
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
