using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class FinishesNode : BinaryDarlNode
    {

        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(base.DoEvaluate(thread));
        }

        public override string midamble => "finishes ";
    }
}