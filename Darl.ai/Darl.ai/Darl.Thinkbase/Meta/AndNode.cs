using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AndNode : BinaryDarlMetaNode
    {

        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                return "and ";
            }
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>The fuzzy logic and of the two child nodes</returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            DarlResult res1 = (DarlResult)await Left.Evaluate(thread);
            if (res1.IsUnknown() || (double)res1.values[0] == 0.0)
                return res1;
            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
            thread.CurrentNode = Parent;
            var res = res1 & res2;
            Epilogue(thread,res);
            return res;
        }
    }
}