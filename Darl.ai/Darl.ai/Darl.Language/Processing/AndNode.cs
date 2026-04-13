/// <summary>
/// AndNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the AND operator
    /// </summary>
    public class AndNode : BinaryDarlNode
    {

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>The fuzzy logic and of the two child nodes</returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlResult res1 = (DarlResult)await Left.Evaluate(thread);
            if (res1.IsUnknown() || (double)res1.values[0] == 0.0)
                return res1;
            DarlResult res2 = (DarlResult)await Right.Evaluate(thread);
            thread.CurrentNode = Parent;
            return res1 & res2;
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
                return "and ";
            }
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            Left.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
            Right.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
        }

    }
}
