/// <summary>
/// NotNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the NOT operator
    /// </summary>
    public class NotNode : UnaryDarlNode
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
            return !res;
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
                return "not ";
            }
        }
    }
}
