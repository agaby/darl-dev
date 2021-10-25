using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class SigmoidNode : UnaryDarlMetaNode
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
            DarlResult res = (DarlResult)await Argument.Evaluate(thread);
            thread.CurrentNode = Parent;
            return res.Sigmoid();
        }

        public override string preamble
        {
            get
            {
                return "sigmoid( ";
            }
        }


        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public override string postamble
        {
            get
            {
                return ")";
            }
        }
    }
}