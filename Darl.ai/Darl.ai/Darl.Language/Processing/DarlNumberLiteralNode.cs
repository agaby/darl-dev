/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Identifies a number literal
    /// </summary>
    public class DarlNumberLiteralNode : DarlNode
    {

        /// <summary>
        /// The fixed result
        /// </summary>
        public DarlResult FixedResult;

        /// <summary>
        /// Determines whether this instance is a constant.
        /// </summary>
        /// <returns>True if a constant</returns>
        public override bool IsConstant()
        {
            return true;
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.Token.Value is int)
                FixedResult = new DarlResult((int)treeNode.Token.Value);
            else if (treeNode.Token.Value is double)
                FixedResult = new DarlResult((double)treeNode.Token.Value);
            else if (treeNode.Token.Value is string)
                FixedResult = new DarlResult("", (string)treeNode.Token.Value, DarlResult.DataType.textual);
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>The result</returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            return Task.FromResult<object>(FixedResult);
        }

        public override string GetName()
        {
            if (!FixedResult.IsUnknown())
                return FixedResult.Value.ToString();
            return base.GetName();
        }

        public override string preamble
        {
            get
            {
                return FixedResult.dataType == DarlResult.DataType.textual ? $"\"{FixedResult.Value.ToString()}\" " : $"{FixedResult.Value.ToString()} ";
            }
        }

    }
}
