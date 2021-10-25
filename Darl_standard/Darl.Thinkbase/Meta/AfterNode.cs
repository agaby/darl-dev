using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AfterNode : UnaryDarlMetaNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {//simple behavior 
            DarlResult res2 = (DarlResult)await Argument.Evaluate(thread);
            res2.Normalise(true);
            return DarlResult.After((DarlResult)thread.CurrentScope.Parameters[0], res2);
        }

        public override string midamble => "after ";
    }
}