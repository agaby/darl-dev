/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public interface IVector
    {
        double x
        {
            get;
            set;
        }

        double y
        {
            get;
            set;
        }

        double z
        {
            get;
            set;
        }

        AbstractVector Add(AbstractVector v2);
        AbstractVector Subtract(AbstractVector v2);
        AbstractVector Multiply(double n);
        AbstractVector Divide(double n);
        double Magnitude();
        //public abstract AbstractVector Normal();
        NodaPosition Normalize();
        NodaPosition SetZero();
        NodaPosition SetIdentity();
    }
}
