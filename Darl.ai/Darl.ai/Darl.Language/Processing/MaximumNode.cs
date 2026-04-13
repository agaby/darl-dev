/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// Implements the maximum of a set of numeric values
    /// </summary>
    public class MaximumNode : MultipleDarlNode
    {
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            int nIndex = 0;
            DarlResult res2 = new DarlResult(0.0, true);
            foreach (DarlNode child in arguments)
            {
                if (child != null)
                {
                    DarlResult res1 = (DarlResult)await child.Evaluate(thread);
                    if (nIndex == 0)
                        res2 = res1;
                    else
                        res2 = DarlResult.Maximum(res2, res1);
                    nIndex++;
                }
                else break;
            }
            return res2;
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
                return "maximum( ";
            }
        }

        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                return ", ";
            }
        }

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
