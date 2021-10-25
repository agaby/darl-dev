using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;

namespace DarlLanguage.Processing
{
    public class DuringNode : UnaryDarlNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            DarlResult res2 = (DarlResult)await Argument.Evaluate(thread);
            res2.Normalise(true);
            return DarlResult.During((DarlResult)thread.CurrentScope.Parameters[0], res2);
        }

        public override string midamble => "during ";
    }
}