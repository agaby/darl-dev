/// <summary>
/// SigmoidNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a sigmoid function including fuzzy arithmetic.
    /// </summary>
    public class SigmoidNode : UnaryDarlNode
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
