/// <summary>
/// CountNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class CountNode : LineageMetaNode
    {
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            int count = 0;
            foreach (var o in grammar!.currentModel.GetConnectedObjects(grammar.currentNode, connLineage, objLineage))
            {
                if (grammar.state.ContainsRecord(o.id))
                {
                    if (grammar.state.ContainsAttribute(o.id, attLineage))
                    {
                        count++;
                    }
                }
            }
            var res = new DarlResult(count);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }

        public override string preamble
        {
            get
            {
                return "count( ";
            }
        }
        public override string midamble
        {
            get
            {
                return ", ";
            }
        }
        public override string postamble
        {
            get
            {
                return ")";
            }
        }
    }
}
