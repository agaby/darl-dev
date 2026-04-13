/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace DarlLanguage.Processing
{
    /// Implements an operator of arity 1
    /// </summary>
    public class UnaryDarlNode : DarlNode
    {
        /// The single argument
        /// </summary>
        protected DarlNode Argument;

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
                Argument = (DarlNode)AddChild("-", nodes.Last());
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
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            Argument.WalkDependencies(dependencies, currentOutput, context);
        }

        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            Argument.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
        }
    }


}
