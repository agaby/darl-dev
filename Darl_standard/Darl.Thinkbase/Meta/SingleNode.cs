using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    //returns just the first matching attribute found
    public class SingleNode : AttributesNode
    {
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var res = await base.DoEvaluate(thread) as DarlResult;
            thread.CurrentNode = Parent;
            if (res.IsUnknown())
                return res;
            if (!res.values.Any())
                return new DarlResult(0.0, true);
            return res.values.First();
        }

        public override string preamble
        {
            get
            {
                return "single( ";
            }
        }
    }
}
