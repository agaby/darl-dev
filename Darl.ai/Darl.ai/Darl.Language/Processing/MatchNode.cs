/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// Evaluates a regular expression match
    /// </summary>
    public class MatchNode : UnaryDarlNode
    {
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            DarlResult r1 = (DarlResult)await Argument.Evaluate(thread);
            var inp = ((DarlResult)thread.CurrentScope.Parameters[0]);
            //Use implicit comparison. Result does the work internally
            return new DarlResult(r1 == inp ? 1.0 : 0.0, false);
        }

        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "match ";
            }
        }



    }
}
