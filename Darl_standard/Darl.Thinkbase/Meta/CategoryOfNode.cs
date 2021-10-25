using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class CategoryOfNode : UnaryDarlMetaNode
    {
        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "categoryof( ";
            }
        }

        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlResult res = (DarlResult)await Argument.Evaluate(thread);
            thread.CurrentNode = Parent;
            return res;
        }

        public override string postamble
        {
            get
            {
                return ")";
            }
        }
    }
}