using DarlCompiler.Interpreter;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class MaxTimeNode : DarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("", DateTime.MaxValue, DarlResult.DataType.temporal));
        }
    }
}
