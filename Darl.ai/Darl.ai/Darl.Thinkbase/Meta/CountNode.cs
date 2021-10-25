using Darl.Thinkbase.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class CountNode : LineageMetaNode
    {
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            int count = 0;
            foreach (var o in grammar.currentModel.GetConnectedObjects(grammar.currentNode, connLineage, objLineage))
            {
                if (grammar.state.ContainsRecord(o.id))
                {
                    if (grammar.state.ContainsAttribute(o.id,attLineage))
                    {
                        count++;
                    }
                }
            }
            thread.CurrentNode = Parent;
            return Task.FromResult<object>(new DarlResult(count));
        }
    }
}
