/// <summary>
/// NotNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class NotNode : UnaryDarlMetaNode
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
            Prologue(thread);
            DarlResult res = (DarlResult)await Argument.Evaluate(thread);
            var res2 = !res;
            Epilogue(thread, res2);
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
                return "not ";
            }
        }
    }
}