using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AllNode : LineageMetaNode
    {
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            var res = new DarlResult(1.0, false);
            foreach(var o in grammar.currentModel.GetConnectedObjects(grammar.currentNode, this.connLineage, this.objLineage))
            {
                if(grammar.state.ContainsRecord(o.id))
                {
                    if (grammar.state.ContainsAttribute(o.id, this.attLineage))
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
            thread.CurrentNode = Parent;
            return Task.FromResult<object>(res);
        }
    }
}