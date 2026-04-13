// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParserStack.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// Class ParserStack.
    /// </summary>
    public class ParserStack : List<ParseTreeNode>
    {
        /// Initializes a new instance of the <see cref="ParserStack"/> class.
        /// </summary>
        public ParserStack() : base(200) { }
        /// Pushes the specified node information.
        /// </summary>
        /// <param name="nodeInfo">The node information.</param>
        public void Push(ParseTreeNode nodeInfo)
        {
            base.Add(nodeInfo);
        }
        /// Pushes the specified node information.
        /// </summary>
        /// <param name="nodeInfo">The node information.</param>
        /// <param name="state">The state.</param>
        public void Push(ParseTreeNode nodeInfo, ParserState state)
        {
            nodeInfo.State = state;
            base.Add(nodeInfo);
        }
        /// Pops this instance.
        /// </summary>
        /// <returns>ParseTreeNode.</returns>
        public ParseTreeNode Pop()
        {
            var top = Top;
            base.RemoveAt(Count - 1);
            return top;
        }
        /// Pops the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        public void Pop(int count)
        {
            base.RemoveRange(Count - count, count);
        }
        /// Pops the until.
        /// </summary>
        /// <param name="finalCount">The final count.</param>
        public void PopUntil(int finalCount)
        {
            if (finalCount < Count)
                Pop(Count - finalCount);
        }
        /// Gets the top.
        /// </summary>
        /// <value>The top.</value>
        public ParseTreeNode Top
        {
            get
            {
                if (Count == 0) return null;
                return base[Count - 1];
            }
        }
    }
}
