using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class MaxTimeNode : DarlMetaNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("", DateTime.MaxValue, DarlResult.DataType.temporal));
        }
    }
}