/// <summary>
/// </summary>

﻿namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public class Spring
    {
        public Spring(Point iPoint1, Point iPoint2, double iLength, double iK)
        {
            point1 = iPoint1;
            point2 = iPoint2;
            Length = iLength;
            K = iK;
        }

        public Point point1
        {
            get;
            private set;
        }
        public Point point2
        {
            get;
            private set;
        }

        public double Length
        {
            get;
            private set;
        }

        public double K
        {
            get;
            private set;
        }
    }
}
