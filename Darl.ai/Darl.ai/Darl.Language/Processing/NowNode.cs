using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class NowNode : DarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("Now", DateTime.UtcNow, DarlResult.DataType.temporal));
        }
    }
}
