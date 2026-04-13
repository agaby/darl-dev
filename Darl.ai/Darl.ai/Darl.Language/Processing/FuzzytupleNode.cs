/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// Implements a fuzzy number.
    /// </summary>
    public class FuzzytupleNode : MultipleDarlNode
    {
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            if (arguments[0] == null)
            {
                return new DarlResult(0.0f, true);
            }
            DarlResult res2 = new DarlResult(true, 1.0);
            foreach (DarlNode child in arguments)
            {
                if (child != null)
                {
                    DarlResult res1 = (DarlResult)await child.Evaluate(thread);
                    if (res1.IsUnknown())//if any children unknown, whole thing unknown.
                        return new DarlResult(0.0f, true);
                    //if the constituents of a fuzzytuple are themselves fuzzy we're into
                    //type 2 fuzzy sets, but since we don't support those yet we'll summarize them
                    //with CofG
                    res2.values.Add(res1.CofG());
                }
                else
                    break; // quit on first null
            }
            res2.values.Sort();
            res2.Normalise(false);
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
                return "fuzzytuple( ";
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
                return ") ";
            }
        }
    }
}
