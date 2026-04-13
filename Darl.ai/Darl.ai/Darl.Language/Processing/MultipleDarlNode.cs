/// <summary>
/// MultipleDarlNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a node with multiple children
    /// </summary>
    public class MultipleDarlNode : DarlNode
    {
        /// <summary>
        /// The list of arguments
        /// </summary>
        protected List<DarlNode> arguments = new List<DarlNode>();

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            if (nodes.Count == 1)
            {
                foreach (var node in nodes[0].ChildNodes)
                {
                    arguments.Add((DarlNode)AddChild("-", node));
                }
            }
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            foreach (var node in arguments)
                node.WalkDependencies(dependencies, currentOutput, context);
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            foreach (var node in arguments)
                node.WalkSaliences(saliency, root, currentRuleSet, currentOutput);

        }
    }
}
