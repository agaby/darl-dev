using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AfterNode : UnaryDarlMetaNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {//simple behavior 
            Prologue(thread);
            DarlResult res2 = (DarlResult)await Argument.Evaluate(thread);
            res2.Normalise(true);
            var res = DarlResult.After((DarlResult)thread.CurrentScope.Parameters[0], res2);
            Epilogue(thread, res);
            return res;
        }

        public override string midamble => "after ";
    }
}