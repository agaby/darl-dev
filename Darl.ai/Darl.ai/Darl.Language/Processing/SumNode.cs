/// <summary>
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements numeric product of as set including fuzzy arithmetic.
    /// </summary>
    public class SumNode : MultipleDarlNode
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
                        res2 = res2 + res1;
                    nIndex++;
                }
                else break;
            }
            thread.CurrentNode = Parent;
            return res2;
        }

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
                return "sum( ";
            }
        }

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
                return ", ";
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
