/// <summary>
/// ImpliedPrecedenceHint.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ImpliedPrecedenceHint.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DarlCompiler.Parsing
{

    //Note: This in incomplete implementation. 
    // this implementation sets precedence only on operator symbols that are already "shifted" into the parser stack, 
    // ie those on the "left" of precedence comparison. It does not set precedence when operator symbol first appears in parser
    // input. This works OK for unary operator but might break some advanced scenarios.

    /// <summary>
    /// Class ImpliedPrecedenceHint.
    /// </summary>
    public class ImpliedPrecedenceHint : GrammarHint
    {
        /// <summary>
        /// The implied precedence custom flag
        /// </summary>
        public const int ImpliedPrecedenceCustomFlag = 0x01000000; // a flag to mark a state for setting implied precedence

        //GrammarHint inherits Precedence and Associativity members from BnfTerm; we'll use them to store implied values for this hint

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpliedPrecedenceHint"/> class.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="associativity">The associativity.</param>
        public ImpliedPrecedenceHint(int precedence, Associativity associativity)
        {
            Precedence = precedence;
            Associativity = associativity;
        }

        /// <summary>
        /// Gives a chance to a custom code in hint to interfere in parser automaton construction.
        /// </summary>
        /// <param name="language">The LanguageData instance.</param>
        /// <param name="owner">The LRItem that "owns" the hint.</param>
        /// <remarks>The most common purpose of this method (it's overrides) is to resolve the conflicts
        /// by adding specific actions into State.Actions dictionary.
        /// The owner parameter represents the position in the grammar expression where the hint
        /// is found. The parser state is available through owner.State property.</remarks>
        public override void Apply(LanguageData language, Construction.LRItem owner)
        {
            //Check that owner is not final - we can imply precedence only in shift context
            var curr = owner.Core.Current;
            if (curr == null)
                return;
            //mark the state, to make sure we do stuff in Term_Shifting event handler only in appropriate states
            owner.State.CustomFlags |= ImpliedPrecedenceCustomFlag;
            curr.Shifting += Term_Shifting;
        }

        /// <summary>
        /// Handles the Shifting event of the Term control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ParsingEventArgs"/> instance containing the event data.</param>
        void Term_Shifting(object sender, ParsingEventArgs e)
        {
            //Set the values only if we are in the marked state
            if (!e.Context.CurrentParserState.CustomFlagIsSet(ImpliedPrecedenceCustomFlag))
                return;
            e.Context.CurrentParserInput.Associativity = Associativity;
            e.Context.CurrentParserInput.Precedence = Precedence;
        }

    }
}
