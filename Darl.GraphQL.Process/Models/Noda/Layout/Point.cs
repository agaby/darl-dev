using Darl.GraphQL.Models.Models.Noda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public class Point
    {
        public Point(NodaPosition iPosition, NodaPosition iVelocity, NodaPosition iAcceleration, NodaNode iNode)
        {
            position=iPosition;
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
            return position==p.position;
        }

        public bool Equals(Point p)
        {
            // If parameter is null return false:
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return position==p.position;
        }

        public void ApplyForce(NodaPosition force)
        {
            acceleration.Add(force/mass);
        }

        public NodaPosition position { get; set; }
        public NodaNode node { get; private set; }
        public double mass
        {
            get
            {
                return node.mass;
            }
            private set
            {
                node.mass = value;
            }
        }
        public NodaPosition velocity { get; private set; }
        public NodaPosition acceleration { get; private set; }
     }
}
