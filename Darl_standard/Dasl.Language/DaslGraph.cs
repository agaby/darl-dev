// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslGraph.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using QuickGraph;

namespace DaslLanguage
{
    /// <summary>
    /// Class DaslGraph.
    /// </summary>
    public class DaslGraph : BidirectionalGraph<DaslVertex, DaslEdge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaslGraph"/> class.
        /// </summary>
        public DaslGraph() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaslGraph"/> class.
        /// </summary>
        /// <param name="allowParallelEdges">if set to <c>true</c> [allow parallel edges].</param>
        public DaslGraph(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaslGraph"/> class.
        /// </summary>
        /// <param name="allowParallelEdges">if set to <c>true</c> [allow parallel edges].</param>
        /// <param name="vertexCapacity">The vertex capacity.</param>
        public DaslGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }
    }
}
