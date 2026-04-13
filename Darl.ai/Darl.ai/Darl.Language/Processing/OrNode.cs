/// <summary>
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the OR operator
    /// </summary>
    public class OrNode : BinaryDarlNode
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
            DarlResult res1 = (DarlResult)await Left.Evaluate(thread);
            if (!res1.IsUnknown() && (double)res1.values[0] == 1.0)
                return res1;
            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
            thread.CurrentNode = Parent;
            return res1 | res2;
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
                return "or ";
            }
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The saliency.</param>
        /// <param name="root">The root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            Left.WalkSaliences(saliency / 2, root, currentRuleSet, currentOutput);
            Left.WalkSaliences(saliency / 2, root, currentRuleSet, currentOutput);
        }
    }
}
