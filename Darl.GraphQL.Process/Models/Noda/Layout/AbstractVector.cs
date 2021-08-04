using Darl.GraphQL.Models.Models.Noda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public abstract class AbstractVector: IVector
    {

        public double x
        {
            get;
            set;
        }

        public double y
        {
            get;
            set;
        }

        public double z
        {
            get;
            set;
        }

        public AbstractVector()
        {
        }

        public abstract AbstractVector Add(AbstractVector v2);
        public abstract AbstractVector Subtract(AbstractVector v2);
        public abstract AbstractVector Multiply(double n);
        public abstract AbstractVector Divide(double n);
        public abstract double Magnitude();
        //public abstract public abstract AbstractVector Normal();
        public abstract NodaPosition Normalize();
        public abstract NodaPosition SetZero();
        public abstract NodaPosition SetIdentity();

        public static AbstractVector operator +(AbstractVector a, AbstractVector b)
        {
            if (a is NodaPosition && b is NodaPosition)
                return (a as NodaPosition) + (b as NodaPosition);
            return null;
        }
        public static AbstractVector operator - (AbstractVector a, AbstractVector b)
        {
            if (a is NodaPosition && b is NodaPosition)
                return (a as NodaPosition) - (b as NodaPosition);
            return null;
        }
        public static AbstractVector operator *(AbstractVector a, double b)
        {
            if (a is NodaPosition)
                return (a as NodaPosition) * b;
            return null;
        }
        public static AbstractVector operator *(double a, AbstractVector b)
        {
            if (b is NodaPosition)
                return a * (b as NodaPosition);
            return null;
        }

        public static AbstractVector operator /(AbstractVector a, double b)
        {
           if (a is NodaPosition)
                return (a as NodaPosition) / b;
            return null;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(System.Object obj)
        {
            return this==(obj as AbstractVector);
        }
        public static bool operator == (AbstractVector a, AbstractVector b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            if (a is NodaPosition && b is NodaPosition)
                return (a as NodaPosition) == (b as NodaPosition);
            return false;

        }

        public static bool operator !=(AbstractVector a, AbstractVector b)
        {
            return !(a == b);
        }



    }


}
