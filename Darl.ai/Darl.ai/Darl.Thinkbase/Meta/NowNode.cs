using Darl.Common;
using DarlCompiler.Interpreter;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Returns either the crisp current UTC time or a fuzzy time set in the grammar object
    /// </summary>
    public class NowNode : DarlMetaNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (grammar!.now == null || !grammar.now.Any())
            {
                var res1 = new DarlResult("Now", DarlTime.UtcNow, DarlResult.DataType.temporal);
                Epilogue(thread, res1);
                return Task.FromResult<object>(res1);
            }
            var res = new DarlResult("Now", grammar.now, DarlResult.DataType.temporal);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }

        public override string preamble
        {
            get
            {
                return "now";
            }
        }
    }
}