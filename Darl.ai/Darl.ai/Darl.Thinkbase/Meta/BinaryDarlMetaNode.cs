/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace Darl.Thinkbase.Meta
{
    public class BinaryDarlMetaNode : DarlMetaNode
    {
        protected DarlMetaNode Left;
        /// The right child
        /// </summary>
        protected DarlMetaNode Right;

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlMetaNode)AddChild("-", nodes[0]);
            Right = (DarlMetaNode)AddChild("-", nodes[1]);
        }

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            Left.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
            Right.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
        }

        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            Left.WalkSaliences(saliency, root);
            Right.WalkSaliences(saliency, root);
        }
    }
}
