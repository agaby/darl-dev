/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using Darl.GraphQL.Process.Web.Models.Noda;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public class Point
    {

        public double mass = 1.0;
        public Point(NodaPosition iPosition, NodaPosition iVelocity, NodaPosition iAcceleration, ILayoutNode iNode)
        {
            position = iPosition;
            node = iNode;
            velocity = iVelocity;
            acceleration = iAcceleration;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Point p = obj as Point;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return position == p.position;
        }

        public bool Equals(Point p)
        {
            // If parameter is null return false:
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return position == p.position;
        }

        public void ApplyForce(NodaPosition force)
        {
            acceleration.Add(force / mass);
        }

        public NodaPosition position { get; set; }
        public ILayoutNode node { get; private set; }

        public NodaPosition velocity { get; private set; }
        public NodaPosition acceleration { get; private set; }
    }
}
