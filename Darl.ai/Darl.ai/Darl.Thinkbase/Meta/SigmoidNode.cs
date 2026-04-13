/// <summary>
/// </summary>

﻿using System.Threading.Tasks;

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
            Prologue(thread);
            DarlResult res = (DarlResult)await Argument.Evaluate(thread);
            var res2 =  res.Sigmoid();
            Epilogue(thread, res2);
            return res2;
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