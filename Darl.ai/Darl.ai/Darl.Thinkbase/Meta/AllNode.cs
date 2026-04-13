/// </summary>

﻿using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AllNode : LineageMetaNode
    {
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            var res = new DarlResult(1.0, false);
            foreach (var o in grammar!.currentModel.GetConnectedObjects(grammar.currentNode, this.connLineage, this.objLineage))
            {
                if (grammar.state.ContainsRecord(o.id!))
                {
                    if (grammar.state.ContainsAttribute(o.id!, this.attLineage))
                    {
                        continue;
                    }
                    else
                    {
                        res = new DarlResult(0.0, true);
                        break;
                    }
                }
                else
                {
                    res = new DarlResult(0.0, true);
                    break;
                }
            }
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }

        public override string preamble
        {
            get
            {
                return "all( ";
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