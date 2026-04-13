/// <summary>
/// GreaterEqualNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the comparison for equality or greater of two numeric nodes
    /// </summary>
    public class GreaterEqualNode : UnaryDarlNode
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
            DarlResult res2 = (DarlResult)await Argument.Evaluate(thread);
            res2.Normalise(true);
            return ((DarlResult)thread.CurrentScope.Parameters[0]) >= res2;
        }

        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return ">= ";
            }
        }
    }
}
