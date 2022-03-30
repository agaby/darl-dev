using Darl.GraphQL.Models.Models.Noda;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public abstract class AbstractVector
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

        public abstract NodaPosition Add(AbstractVector v2);
        public abstract NodaPosition Subtract(AbstractVector v2);
        public abstract NodaPosition Multiply(double n);
        public abstract NodaPosition Divide(double n);
        public abstract double Magnitude();
        //public abstract public abstract AbstractVector Normal();
        public abstract NodaPosition Normalize();
        public abstract NodaPosition SetZero();
        public abstract NodaPosition SetIdentity();

        public static NodaPosition operator +(AbstractVector a, AbstractVector b)
        {
            if (a is NodaPosition && b is NodaPosition)
                return (a as NodaPosition) + (b as NodaPosition);
            return null;
        }
        public static NodaPosition operator -(AbstractVector a, AbstractVector b)
        {
            if (a is NodaPosition && b is NodaPosition)
                return (a as NodaPosition) - (b as NodaPosition);
            return null;
        }
        public static NodaPosition operator *(AbstractVector a, double b)
        {
            if (a is NodaPosition)
                return (a as NodaPosition) * b;
            return null;
        }
        public static NodaPosition operator *(double a, AbstractVector b)
        {
            if (b is NodaPosition)
                return a * (b as NodaPosition);
            return null;
        }

        public static NodaPosition operator /(AbstractVector a, double b)
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
            return this == (obj as NodaPosition);
        }



    }


}
