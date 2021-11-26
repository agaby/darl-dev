using Darl.Common;
using DarlCompiler.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// returns the age of the temporal input relative to the current time.
    /// requires one parameter, a variable or function  returning a temporal value.
    /// </summary>
    public class AgeNode : UnaryDarlMetaNode
    {

        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlResult? res = Argument != null ? (DarlResult)await Argument.Evaluate(thread) : null;
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (res.Exists())
            {
                var nowNode = new NowNode();
                var now = await nowNode.Evaluate(thread) as DarlResult;
                thread.CurrentNode = Parent;
                return DarlResult.Age(res, now);
 
            }
            return new DarlResult(-1.0, true);
        }

        public override string preamble
        {
            get
            {
                return "age( ";
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
