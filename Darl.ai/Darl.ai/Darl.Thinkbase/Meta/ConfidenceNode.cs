/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class ConfidenceNode : DarlMetaNode
    {
        public double weight = 1.0;

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
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            DarlResult conf = new DarlResult();
            //get value if constant node set.
            conf.SetWeight(weight);
            Epilogue(thread, conf);
            return Task.FromResult<object>(conf);
        }

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