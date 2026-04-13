/// <summary>
/// </summary>

﻿using System.Threading.Tasks;

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
            Prologue(thread);
            DarlResult res = (DarlResult)await Argument.Evaluate(thread);
            Epilogue(thread, res);
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