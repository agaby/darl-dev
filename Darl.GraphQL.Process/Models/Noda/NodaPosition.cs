/// </summary>

﻿using Darl.GraphQL.Process.Models.Noda.Layout;
using System;

namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaPosition : AbstractVector
    {

        public NodaPosition() : base()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        public NodaPosition(double iX, double iY, double iZ)
            : base()
        {
            x = iX;
            y = iY;
            z = iZ;

        }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public NodaPosition Add(NodaPosition v2)
        {
            NodaPosition v32 = v2;
            x = x + v32.x;
            y = y + v32.y;
            z = z + v32.z;
            return this;
        }

        public NodaPosition Subtract(NodaPosition v2)
        {
            NodaPosition v32 = v2;
            x = x - v32.x;
            y = y - v32.y;
            z = z - v32.z;
            return this;
        }

        public override NodaPosition Multiply(double n)
        {
            x = x * n;
            y = y * n;
            z = z * n;
            return this;
        }

        public override NodaPosition Divide(double n)
        {
            if (n == 0.0f)
            {
                x = 0.0f;
                y = 0.0f;
                z = 0.0f;
            }
            else
            {
                x = x / n;
                y = y / n;
                z = z / n;
            }
            return this;
        }

        public override double Magnitude()
        {
            return Math.Sqrt((double)(x * x) + (double)(y * y) + (double)(z * z));
        }


        public override NodaPosition Normalize()
        {
            return this / Magnitude();
        }

        public override NodaPosition SetZero()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
            return this;
        }
        public override NodaPosition SetIdentity()
        {
            x = 1.0f;
            y = 1.0f;
            z = 1.0f;
            return this;
        }
        public static NodaPosition Zero()
        {
            return new NodaPosition(0.0f, 0.0f, 0.0f);
        }

        public static NodaPosition Identity()
        {
            return new NodaPosition(1.0f, 1.0f, 1.0f);
        }

        public static NodaPosition Random()
        {
            return new NodaPosition(10.0f * (Util.Random() - 0.5f), 10.0f * (Util.Random() - 0.5f), 10.0f * (Util.Random() - 0.5f));
        }

        public override NodaPosition Add(AbstractVector v2)
        {
            return Add(v2 as NodaPosition);
        }

        public override NodaPosition Subtract(AbstractVector v2)
        {
            return Subtract(v2 as NodaPosition);
        }

        public static NodaPosition operator +(NodaPosition a, NodaPosition b)
        {
            NodaPosition temp = new NodaPosition(a.x, a.y, a.z);
            temp.Add(b);
            return temp;
        }
        public static NodaPosition operator -(NodaPosition a, NodaPosition b)
        {
            NodaPosition temp = new NodaPosition(a.x, a.y, a.z);
            temp.Subtract(b);
            return temp;
        }
        public static NodaPosition operator *(NodaPosition a, double b)
        {
            NodaPosition temp = new NodaPosition(a.x, a.y, a.z);
            temp.Multiply(b);
            return temp;
        }
        public static NodaPosition operator *(float a, NodaPosition b)
        {
            NodaPosition temp = new NodaPosition(b.x, b.y, b.z);
            temp.Multiply(a);
            return temp;
        }

        public static NodaPosition operator /(NodaPosition a, double b)
        {
            NodaPosition temp = new NodaPosition(a.x, a.y, a.z);
            temp.Divide(b);
            return temp;
        }
    }
}
