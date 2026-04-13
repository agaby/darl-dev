/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Thinkbase.Meta
{
    public class UnaryDarlMetaNode : DarlMetaNode
    {
        /// The single argument
        /// </summary>
        protected DarlMetaNode Argument;

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            try
            {
                base.Init(context, treeNode);
                var nodes = treeNode.GetMappedChildNodes();
                if (nodes.Any())
                    Argument = (DarlMetaNode)AddChild("-", nodes.Last());
            }
            catch
            {

            }
        }

        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            if (Argument != null)
                Argument.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
        }

        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            if (Argument != null)
                Argument.WalkSaliences(saliency, root);
        }
    }
}
