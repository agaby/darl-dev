using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class PlusNode : BinaryDarlMetaNode
    {
        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            DarlResult res1 = (DarlResult)await Left.Evaluate(thread);
            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
            thread.CurrentNode = Parent;
            var res =  res1 + res2;
            Epilogue(thread, res);
            return res;
        }

        public override string midamble
        {
            get
            {
                return "+ ";
            }
        }
    }
}