/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// Implements an interpreter functional node with arity 2.
    /// </summary>
    public class BinaryDarlNode : DarlNode
    {
        /// The left child
        /// </summary>
        protected DarlNode Left;
        /// The right child
        /// </summary>
        protected DarlNode Right;

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlNode)AddChild("-", nodes[0]);
            Right = (DarlNode)AddChild("-", nodes[1]);
        }

        /// Establishes dependencies and initialises constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            Left.WalkDependencies(dependencies, currentOutput, context);
            Right.WalkDependencies(dependencies, currentOutput, context);
        }

        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            Left.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
            Right.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
        }

    }
}
