/// <summary>
/// LineageLiteral.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Holds a lineage with validation checks
    /// </summary>
    public class LineageLiteral : DarlMetaNode
    {

        public string literal { get; set; }

        public string typeword { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            literal = (string)treeNode.Token.Value;
            var validity = LineageLibrary.CheckLineageWithTypeWord(literal);
            if (!validity.Item1)
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.Token.Location, $"'{literal}' is not a valid lineage.");
            else
                typeword = validity.Item2;
        }

        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var res = new DarlResult("", literal, DarlResult.DataType.textual);
            Epilogue(thread,res);
            return Task.FromResult<object>(res);
        }

    }
}
