using Darl.GraphQL.Models.Models.Noda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public delegate void EdgeAction(NodaLink edge, Spring spring);
    public delegate void NodeAction(NodaNode edge, Point point);

    public interface IForceDirected
    {
        NodaDocument graph
        {
            get;
        }

        double Stiffness
        {
            get;
        }

        double Repulsion
        {
            get;
        }

        double Damping
        {
            get;
        }

        double Threshold // NOT Using
        {
            get;
            set;
        }
        bool WithinThreshold
        {
            get;
        }
        void Clear();
        void Calculate(double iTimeStep);
        void EachEdge(EdgeAction del);
        void EachNode(NodeAction del);
        NearestPoint Nearest(AbstractVector position);
        BoundingBox GetBoundingBox();
    }
}
