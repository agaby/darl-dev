using Darl.GraphQL.Models.Models.Noda;
using System.Collections.Generic;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public class NearestPoint
    {
        public NearestPoint()
        {
            node = null;
            point = null;
            distance = null;
        }
        public NodaNode? node;
        public Point? point;
        public double? distance;
    }

    public class BoundingBox
    {
        public static double defaultBB = 2.0;
        public static double defaultPadding = 0.07; // ~5% padding

        public BoundingBox()
        {
            topRightBack = null;
            bottomLeftFront = null;
        }
        public NodaPosition? topRightBack;
        public NodaPosition? bottomLeftFront;
    }

    public class ForceDirected3D
    {

        public double Stiffness
        {
            get;
            set;
        }

        public double Repulsion
        {
            get;
            set;
        }

        public double Damping
        {
            get;
            set;
        }

        public double Threshold
        {
            get;
            set;
        }

        public bool WithinThreshold
        {
            get;
            private set;
        }

        protected Dictionary<string, Point> m_nodePoints;
        protected Dictionary<string, Spring> m_NodaLinkSprings;

        public NodaDocument graph
        {
            get;
            protected set;
        }
        public void Clear()
        {
            m_nodePoints.Clear();
            m_NodaLinkSprings.Clear();
        }


        public delegate void EdgeAction(NodaLink edge, Spring spring);
        public delegate void NodeAction(NodaNode edge, Point point);
        public ForceDirected3D(NodaDocument iGraph, double iStiffness, double iRepulsion, double iDamping)
        {
            graph = iGraph;
            Stiffness = iStiffness;
            Repulsion = iRepulsion;
            Damping = iDamping;
            m_nodePoints = new Dictionary<string, Point>();
            m_NodaLinkSprings = new Dictionary<string, Spring>();
            Threshold = 0.01f;
        }

        public Point GetPoint(NodaNode iNodaNode)
        {
            if (!(m_nodePoints.ContainsKey(iNodaNode.uuid)))
            {
                NodaPosition iniPosition = iNodaNode.position;
                if (iniPosition == null)
                    iniPosition = NodaPosition.Random();
                m_nodePoints[iNodaNode.uuid] = new Point(iniPosition, NodaPosition.Zero(), NodaPosition.Zero(), iNodaNode);
            }
            return m_nodePoints[iNodaNode.uuid];
        }

        public BoundingBox GetBoundingBox()
        {
            BoundingBox boundingBox = new BoundingBox();
            NodaPosition bottomLeft = NodaPosition.Identity().Multiply(BoundingBox.defaultBB * -1.0f);
            NodaPosition topRight = NodaPosition.Identity().Multiply(BoundingBox.defaultBB);
            foreach (NodaNode n in graph.nodes)
            {
                NodaPosition position = GetPoint(n).position;
                if (position.x < bottomLeft.x)
                    bottomLeft.x = position.x;
                if (position.y < bottomLeft.y)
                    bottomLeft.y = position.y;
                if (position.z < bottomLeft.z)
                    bottomLeft.z = position.z;
                if (position.x > topRight.x)
                    topRight.x = position.x;
                if (position.y > topRight.y)
                    topRight.y = position.y;
                if (position.z > topRight.z)
                    topRight.z = position.z;
            }
            NodaPosition padding = (topRight - bottomLeft).Multiply(BoundingBox.defaultPadding);
            boundingBox.bottomLeftFront = bottomLeft.Subtract(padding);
            boundingBox.topRightBack = topRight.Add(padding);
            return boundingBox;

        }

        public Spring GetSpring(NodaLink iNodaLink)
        {
            if (!(m_NodaLinkSprings.ContainsKey(iNodaLink.uuid)))
            {
                double length = iNodaLink.length;
                Spring? existingSpring = null;

                var fromNodaLink = graph.GetEdge(iNodaLink.fromNode, iNodaLink.toNode);
                if (fromNodaLink != null)
                {
                    if (existingSpring == null && m_NodaLinkSprings.ContainsKey(fromNodaLink.uuid))
                    {
                        existingSpring = m_NodaLinkSprings[fromNodaLink.uuid];
                    }
                }
                if (existingSpring != null)
                {
                    return new Spring(existingSpring.point1, existingSpring.point2, 0.0, 0.0);
                }

                var toNodaLink = graph.GetEdge(iNodaLink.toNode, iNodaLink.fromNode);
                if (toNodaLink != null)
                {
                    if (existingSpring == null && m_NodaLinkSprings.ContainsKey(toNodaLink.uuid))
                    {
                        existingSpring = m_NodaLinkSprings[toNodaLink.uuid];
                    }
                }

                if (existingSpring != null)
                {
                    return new Spring(existingSpring.point2, existingSpring.point1, 0.0, 0.0);
                }
                m_NodaLinkSprings[iNodaLink.uuid] = new Spring(GetPoint(graph.GetNode(iNodaLink.fromNode.Uuid)), GetPoint(graph.GetNode(iNodaLink.toNode.Uuid)), length, Stiffness);

            }
            return m_NodaLinkSprings[iNodaLink.uuid];
        }

        protected void applyCoulombsLaw()
        {
            foreach (NodaNode n1 in graph.nodes)
            {
                Point point1 = GetPoint(n1);
                foreach (NodaNode n2 in graph.nodes)
                {
                    Point point2 = GetPoint(n2);
                    if (!point1.Equals(point2))
                    {
                        NodaPosition d = point1.position - point2.position;
                        double distance = d.Magnitude() + 0.1;
                        NodaPosition direction = d.Normalize();
                        if (n1.Pinned && n2.Pinned)
                        {
                            point1.ApplyForce(direction * 0.0);
                            point2.ApplyForce(direction * 0.0);
                        }
                        else if (n1.Pinned)
                        {
                            point1.ApplyForce(direction * 0.0);
                            point2.ApplyForce((direction * Repulsion) / (distance * -1.0));
                        }
                        else if (n2.Pinned)
                        {
                            point1.ApplyForce((direction * Repulsion) / (distance));
                            point2.ApplyForce(direction * 0.0f);
                        }
                        else
                        {

                            point1.ApplyForce((direction * Repulsion) / (distance * 0.5));
                            point2.ApplyForce((direction * Repulsion) / (distance * -0.5));
                        }

                    }
                }
            }
        }

        protected void applyHookesLaw()
        {
            foreach (NodaLink e in graph.links)
            {
                Spring spring = GetSpring(e);
                NodaPosition d = spring.point2.position - spring.point1.position;
                double displacement = spring.Length - d.Magnitude();
                NodaPosition direction = d.Normalize();

                if (spring.point1.node.Pinned && spring.point2.node.Pinned)
                {
                    spring.point1.ApplyForce(direction * 0.0f);
                    spring.point2.ApplyForce(direction * 0.0f);
                }
                else if (spring.point1.node.Pinned)
                {
                    spring.point1.ApplyForce(direction * 0.0f);
                    spring.point2.ApplyForce(direction * (spring.K * displacement));
                }
                else if (spring.point2.node.Pinned)
                {
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -1.0f));
                    spring.point2.ApplyForce(direction * 0.0f);
                }
                else
                {
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -0.5f));
                    spring.point2.ApplyForce(direction * (spring.K * displacement * 0.5f));
                }


            }
        }

        protected void attractToCentre()
        {
            foreach (NodaNode n in graph.nodes)
            {
                Point point = GetPoint(n);
                if (!point.node.Pinned)
                {
                    NodaPosition direction = point.position * -1.0;
                    //point.ApplyForce(direction * ((float)Math.Sqrt((double)(Repulsion / 100.0f))));


                    double displacement = direction.Magnitude();
                    direction = direction.Normalize();
                    point.ApplyForce(direction * (Stiffness * displacement * 0.4));
                }
            }
        }

        protected void updateVelocity(double iTimeStep)
        {
            foreach (NodaNode n in graph.nodes)
            {
                Point point = GetPoint(n);
                point.velocity.Add(point.acceleration * iTimeStep);
                point.velocity.Multiply(Damping);
                point.acceleration.SetZero();
            }
        }

        protected void updatePosition(double iTimeStep)
        {
            foreach (NodaNode n in graph.nodes)
            {
                Point point = GetPoint(n);
                point.position.Add(point.velocity * iTimeStep);
            }
        }

        protected double getTotalEnergy()
        {
            double energy = 0.0;
            foreach (NodaNode n in graph.nodes)
            {
                Point point = GetPoint(n);
                double speed = point.velocity.Magnitude();
                energy += 0.5f * point.mass * speed * speed;
            }
            return energy;
        }

        public void Calculate(double iTimeStep) // time in second
        {
            applyCoulombsLaw();
            applyHookesLaw();
            attractToCentre();
            updateVelocity(iTimeStep);
            updatePosition(iTimeStep);
            if (getTotalEnergy() < Threshold)
            {
                WithinThreshold = true;
            }
            else
                WithinThreshold = false;
        }

        public NearestPoint Nearest(NodaPosition position)
        {
            NearestPoint min = new NearestPoint();
            foreach (NodaNode n in graph.nodes)
            {
                Point point = GetPoint(n);
                double distance = (point.position - position).Magnitude();
                if (min.distance == null || distance < min.distance)
                {
                    min.node = n;
                    min.point = point;
                    min.distance = distance;
                }
            }
            return min;
        }
    }
}
