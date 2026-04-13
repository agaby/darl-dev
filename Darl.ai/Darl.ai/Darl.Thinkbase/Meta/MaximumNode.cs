/// </summary>

﻿using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class MaximumNode : AggregateDarlMetaNode
    {
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            int nIndex = 0;
            DarlResult res2 = new DarlResult(0.0, true);
            if (arguments.Count == 1 && arguments[0] is AttributesNode)
            {
                var aggregate = (DarlResult)await arguments[0].Evaluate(thread);
                foreach (DarlResult res1 in aggregate.values)
                {
                    if (nIndex == 0)
                        res2 = res1;
                    else
                        res2 = DarlResult.Maximum(res2, res1);
                    nIndex++;
                }
            }
            else
            {
                foreach (DarlMetaNode child in arguments)
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
            }
            Epilogue(thread, res2);
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