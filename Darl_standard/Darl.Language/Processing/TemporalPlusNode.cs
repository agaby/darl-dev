using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements temporal addition including fuzzy arithmetic.
    /// </summary>
    public class TemporalPlusNode : BinaryDarlNode
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
            thread.CurrentNode = this;  //standard prologue
            DarlResult res1 = (DarlResult)await Left.Evaluate(thread);
            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
            thread.CurrentNode = Parent;
            return res1 + res2; //n.b. one must be period, so temporal + period or period + temporal
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
