/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class LifetimeNode : DarlMetaNode
    {
        protected DarlMetaNode Argument;

        protected DarlResult lifetime = new DarlResult("lifetime", DarlResult.DataType.temporal, 1.0);

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Argument = (DarlMetaNode)AddChild("-", nodes.Last());
        }

        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
            var nowNode = new NowNode();
            var now = await nowNode.Evaluate(thread) as DarlResult;
            var duration = await Argument.Evaluate(thread) as DarlResult;
            lifetime = now! + duration!;
            Epilogue(thread, lifetime);
            return lifetime;
        }

        public override string preamble => base.preamble;
    }
}
