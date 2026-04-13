/// <summary>
/// </summary>

﻿using DarlCompiler.Interpreter.Ast;
using System;
using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements an interpreter functional node
    /// </summary>
    public class DarlNode : AstNode

    {
        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public virtual void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {

        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public virtual void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
        }


        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public virtual string preamble { get { return string.Empty; } }
        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public virtual string midamble { get { return string.Empty; } }
        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public virtual string postamble { get { return string.Empty; } }

        /// <summary>
        /// Gets the HTML preamble.
        /// </summary>
        /// <value>
        /// The HTML preamble.
        /// </value>
        public virtual string htmlPreamble { get { return preamble; } }

        /// <summary>
        /// Gets the HTML midamble.
        /// </summary>
        /// <value>
        /// The HTML midamble.
        /// </value>
        public virtual string htmlMidamble { get { return "<span class=\"text-info\">" + midamble + "</span>"; } }

        /// <summary>
        /// Gets the HTML postamble.
        /// </summary>
        /// <value>
        /// The HTML postamble.
        /// </value>
        public virtual string htmlPostamble { get { return postamble; } }

        /// <summary>
        /// Apply a mutation to each element of the tree with the given probability
        /// </summary>
        /// <param name="rate">mutation rate, greater or equal to zero, less than 1.</param>
        public virtual void Mutate(double rate)
        {

        }

        /// <summary>
        /// Crossover operator
        /// </summary>
        /// <param name="other">Other node to cross with</param>
        public virtual void Mate(DarlNode other)
        {

        }

        public virtual string lineage { get; }

        public virtual string GetName()
        {
            return String.Empty;
        }


    }
}
