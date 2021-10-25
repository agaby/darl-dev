using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;

namespace DarlLanguage.Processing
{
    public class MinTimeNode : DarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("", DateTime.MinValue, DarlResult.DataType.temporal));
        }
    }
}
