/// <summary>
/// ConfidenceNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Holds the confidence associated with a rule
    /// </summary>
    public class ConfidenceNode : DarlNode
    {
        public double weight = 1.0;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            //attach constant node
            base.Init(context, treeNode);
            if (treeNode.ChildNodes.Count > 0)
                weight = Convert.ToDouble(treeNode.ChildNodes[0].Token.Value);
        }

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            DarlResult conf = new DarlResult();
            //get value if constant node set.
            conf.SetWeight(weight);
            return Task.FromResult<object>(conf);
        }

        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return weight == 1.0 ? "" : "confidence " + weight.ToString("0.00");
            }
        }
    }
}
