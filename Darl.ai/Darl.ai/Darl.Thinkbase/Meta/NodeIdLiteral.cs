using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
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
            return Task.FromResult<object>(new DarlResult("", literal, DarlResult.DataType.textual));
        }
    }
}
