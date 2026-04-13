/// <summary>
/// CategoryLiteralNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class CategoryLiteralNode : DarlMetaNode
    {
        string category { get; set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            category = (string)treeNode.Token.Value;
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>The category as a Result</returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var res = new DarlResult("", category); ;
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }
    }
}