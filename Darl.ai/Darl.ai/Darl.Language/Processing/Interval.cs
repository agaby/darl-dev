/// <summary>
/// </summary>

﻿using System;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Represents a numeric interval
    /// </summary>
    [Serializable]
    public class Interval
    {
        /// <summary>
        /// constructor	
        /// </summary>
        public Interval()
        {
            lower = 0.0;
            upper = 0.0;
        }
        /// <summary>
        /// constructs a singleton
        /// </summary>
        /// <param name="Value">A crisp value</param>
        public Interval(double Value)
        {
            lower = Value;
            upper = Value;
        }
        /// <summary>
        /// constructs an interval
        /// </summary>
        /// <param name="low">lower bound</param>
        /// <param name="up">upper bound</param>
        public Interval(double low, double up)
        {
            lower = low;
            upper = up;
            if (lower > upper)
            {
                throw new RuleException("Confidence Interval Error: Lower bound greater than upper bound");
            }
        }
        /// <summary>
        /// lower bound
        /// </summary>
        public double lower;
        /// <summary>
        /// upper bound
        /// </summary>
        public double upper;
        /// <summary>
        /// returns true if interval contains point
        /// </summary>
        /// <param name="Value">point</param>
        /// <returns>true if contained</returns>
        public bool Contains(double Value)
        {
            return Value >= lower && Value <= upper;
        }
        /// <summary>
        /// unary minus operator
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator -(Interval opnd1)
        {
            return new Interval(-opnd1.upper, -opnd1.lower);
        }
        /// <summary>
        /// binary plus operator with singleton
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator +(Interval opnd1, double opnd2)
        {
            return new Interval(opnd1.lower + opnd2, opnd1.upper + opnd2);
        }
        /// <summary>
        /// binary plus between two intervals
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator +(Interval opnd1, Interval opnd2)
        {
            return new Interval(opnd1.lower + opnd2.lower, opnd1.upper + opnd2.upper);
        }
        /// <summary>
        /// subtract a singleton from an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator -(Interval opnd1, double opnd2)
        {
            return new Interval(opnd1.lower - opnd2, opnd1.upper - opnd2);
        }
        /// <summary>
        /// binary minus between two intervals
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Interval operator -(Interval opnd1, Interval opnd2)
        {
            return new Interval(opnd1.lower - opnd2.upper, opnd1.upper - opnd2.lower);
        }
        /// <summary>
        /// Multiply an interval by a singleton
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator *(Interval opnd1, double opnd2)
        {
            return new Interval(opnd2 * ((opnd2 >= 0) ? opnd1.lower : opnd1.upper),
                opnd2 * ((opnd2 >= 0) ? opnd1.upper : opnd1.lower));
        }
        /// <summary>
        /// multiply an interval by an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator *(Interval opnd1, Interval opnd2)
        {
            double tmp1 = opnd1.lower * opnd2.lower;
            double tmp2 = opnd1.lower * opnd2.upper;
            double tmp3 = opnd1.upper * opnd2.lower;
            double tmp4 = opnd1.upper * opnd2.upper;

            return new Interval(min4(tmp1, tmp2, tmp3, tmp4),
                max4(tmp1, tmp2, tmp3, tmp4));
        }
        /// <summary>
        /// divide an interval by a singleton
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator /(Interval opnd1, double opnd2)
        {
            return opnd1 * (1 / opnd2);
        }
        /// <summary>
        /// divide an interval by an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator /(Interval opnd1, Interval opnd2)
        {
            return opnd1 * (1 / opnd2);
        }
        /// <summary>
        /// add a singleton to an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator +(double opnd1, Interval opnd2)
        {
            return opnd2 + opnd1;
        }
        /// <summary>
        /// subtract an interval from a singleton
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator -(double opnd1, Interval opnd2)
        {
            return -opnd2 + opnd1;
        }
        /// <summary>
        /// multiply a singleton by an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval operator *(double opnd1, Interval opnd2)
        {
            return opnd2 * opnd1;
        }
        /// <summary>
        /// divide a singleton by an interval
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        /// <exception cref="RuleException">Divide by zero error in Interval</exception>
        public static Interval operator /(double opnd1, Interval opnd2)
        {
            if ((opnd2.lower * opnd2.upper) <= 0)
            {
                throw new RuleException("Divide by zero error in Interval");
            }
            return opnd1 * new Interval(1 / opnd2.upper, 1 / opnd2.lower);
        }
        /// <summary>
        /// find the minimum of two intervals
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval min(Interval opnd1, Interval opnd2)
        {
            return new Interval((opnd1.lower <= opnd2.lower) ? opnd1.lower : opnd2.lower,
                (opnd1.upper <= opnd2.upper) ? opnd1.upper : opnd2.upper);

        }
        /// <summary>
        /// find the maximum of two intervals
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval max(Interval opnd1, Interval opnd2)
        {
            return new Interval((opnd1.lower >= opnd2.lower) ? opnd1.lower : opnd2.lower,
                (opnd1.upper >= opnd2.upper) ? opnd1.upper : opnd2.upper);
        }
        /// <summary>
        /// find the power of two intervals
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval power(Interval opnd1, Interval opnd2)
        {
            return new Interval(min4(Math.Pow(opnd1.lower, opnd2.lower), Math.Pow(opnd1.lower, opnd2.upper), Math.Pow(opnd1.upper, opnd2.lower), Math.Pow(opnd1.upper, opnd2.upper)),
                                max4(Math.Pow(opnd1.lower, opnd2.lower), Math.Pow(opnd1.lower, opnd2.upper), Math.Pow(opnd1.upper, opnd2.lower), Math.Pow(opnd1.upper, opnd2.upper)));
        }
        /// <summary>
        /// find the sine of an interval
        /// </summary>
        /// <param name="opnd">The opnd.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static Interval sin(Interval opnd)
        {
            if (opnd.upper - opnd.lower >= (2 * Math.PI))
                return new Interval(-1, 1);

            int offset = (int)(opnd.lower / (2 * Math.PI));
            double max = (Math.PI / 2) + offset * (2 * Math.PI);
            double min = (3 * Math.PI / 2) + offset * (2 * Math.PI);
            double low = Math.Sin(opnd.lower);
            double upp = Math.Sin(opnd.upper);

            double lower = ((opnd.lower <= min && opnd.upper >= min)
                || opnd.upper >= (min + (2 * Math.PI)))
                ? -1
                : (low < upp) ? low : upp;

            double upper = ((opnd.lower <= max && opnd.upper >= max)
                || opnd.upper >= (max + (2 * Math.PI)))
                ? 1
                : (low > upp) ? low : upp;

            return new Interval(lower, upper);
        }
        /// <summary>
        /// find the cosine of an interval
        /// </summary>
        /// <param name="opnd">the operand</param>
        /// <returns>The resulting interval</returns>
        public static Interval cos(Interval opnd)
        {
            if (opnd.upper - opnd.lower >= (2 * Math.PI))
                return new Interval(-1, 1);

            int offset = (int)(opnd.lower / (2 * Math.PI));
            double max = offset * (2 * Math.PI);
            double min = Math.PI + offset * (2 * Math.PI);
            double low = Math.Cos(opnd.lower);
            double upp = Math.Cos(opnd.upper);

            double lower = ((opnd.lower <= min && opnd.upper >= min)
                || opnd.upper >= (min + (2 * Math.PI)))
                ? -1
                : (low < upp) ? low : upp;

            double upper = ((opnd.lower <= max && opnd.upper >= max)
                || opnd.upper >= (max + (2 * Math.PI)))
                ? 1 : (low > upp) ? low : upp;

            return new Interval(lower, upper);
        }
        /// <summary>
        /// helper function
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        public static double ciDelta(Interval opnd1, Interval opnd2)
        {
            return (Math.Abs(opnd1.lower - opnd2.lower) +
                Math.Abs(opnd1.upper - opnd2.upper));
        }
        /// <summary>
        /// find the largest of 4 doubles
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <param name="opnd3">The opnd3.</param>
        /// <param name="opnd4">The opnd4.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        private static double max4(double opnd1, double opnd2, double opnd3, double opnd4)
        {
            double max = (opnd1 > opnd2) ? opnd1 : opnd2;
            if (opnd3 > max) max = opnd3;
            if (opnd4 > max) max = opnd4;
            return max;
        }
        /// <summary>
        /// find the smallest of 4 doubles
        /// </summary>
        /// <param name="opnd1">The opnd1.</param>
        /// <param name="opnd2">The opnd2.</param>
        /// <param name="opnd3">The opnd3.</param>
        /// <param name="opnd4">The opnd4.</param>
        /// <returns>
        /// The resulting interval
        /// </returns>
        private static double min4(double opnd1, double opnd2, double opnd3, double opnd4)
        {
            double min = (opnd1 < opnd2) ? opnd1 : opnd2;
            if (opnd3 < min) min = opnd3;
            if (opnd4 < min) min = opnd4;
            return min;
        }
        /// <summary>
        /// look for identical equality
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        /// true if identical
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Interval)
            {
                Interval i = obj as Interval;
                if (i.lower == lower && i.upper == upper)
                    return true;
                return false;
            }
            return false;
        }
        /// <summary>
        /// true if interval represents a single point
        /// </summary>
        /// <returns>true if singleton</returns>
        public bool IsSingleton()
        {
            return lower == upper;
        }
        /// <summary>
        /// represent the interval as a string
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            if (IsSingleton())
                return "singleton: " + lower.ToString("#.###");
            return "lower: " + lower.ToString("#.###") + " upper: " + upper.ToString("#.###");
        }
        /// <summary>
        /// gets a hash code
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// determines if two intervals overlap
        /// </summary>
        /// <param name="int1">First interval</param>
        /// <param name="int2">Second interval</param>
        /// <param name="gap">public or external gap</param>
        /// <returns>true if overlap</returns>
        public static bool Overlap(Interval int1, Interval int2, ref double gap)
        {
            bool overlap = true;
            if (int2.lower > int1.upper || int2.upper < int1.lower)
            {
                overlap = false;
            }
            gap = Interval.min4(Math.Abs(int1.lower - int2.lower), Math.Abs(int1.lower - int2.upper), Math.Abs(int1.upper - int2.lower), Math.Abs(int1.upper - int2.upper));
            return overlap;
        }

        /// <summary>
        /// Compares two intervals using the method of Dorohonceanu and Marin
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The Alpha value</returns>
        public static double GreaterThan(Interval a, Interval b)
        {
            double alpha = 0.0;
            if (a.length == 0.0 && b.length == 0.0)
            {
                if (a.upper > b.lower)
                    alpha = 1.0;
                else if (a.upper < b.lower)
                    alpha = 0.0;
                else // a.upper == b.lower
                    alpha = 0.5;
            }
            else
            {
                alpha = (a.upper - b.lower) / (a.length + b.length);
                if (alpha < 0.0)
                    alpha = 0.0;
                else if (alpha > 1.0)
                    alpha = 1.0;
            }
            return alpha;
        }
        /// <summary>
        /// the numeric length of the interval
        /// </summary>
        public double length
        {
            get
            {
                return upper - lower;
            }
        }

        /// <summary>
        /// returns the mid point of the interval
        /// </summary>
        /// <returns>The value</returns>
        public double Mean()
        {
            return (upper + lower) / 2.0;
        }

        /// <summary>
        /// returns the minimum bounding interval containing two intervals
        /// </summary>
        /// <param name="int1">The int1.</param>
        /// <param name="int2">The int2.</param>
        /// <returns>The resulting interval</returns>
        public static Interval Conjunction(Interval int1, Interval int2)
        {
            if (int1 == null)
                return int2;
            if (int2 == null)
                return int1;
            return new Interval(Math.Min(int1.lower, int2.lower), Math.Max(int1.upper, int2.upper));
        }
    }
}
