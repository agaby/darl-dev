/// <summary>
/// CompIoNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Holds a composite IO reference
    /// </summary>
    public class CompIoNode : DarlNode
    {

        int writeIndex = 0;
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                var childAst = AddChild("-", child);
            }
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>A True result</returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult(1.0, false));
        }

        public override string midamble
        {
            get
            {
                switch (writeIndex)
                {
                    case 0:
                        writeIndex++;
                        return "";
                    case 1:
                        writeIndex++;
                        return ".";
                    default:
                        return "";
                }
            }
        }
    }
}
