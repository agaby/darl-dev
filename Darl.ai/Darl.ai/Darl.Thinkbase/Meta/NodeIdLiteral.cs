/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class NodeIdLiteral : DarlMetaNode
    {
        public string literal { get; set; }

        public GraphObject node { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            literal = (string)treeNode.Token.Value;
        }

        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var res = new DarlResult("", literal, DarlResult.DataType.textual);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }
    }
}
