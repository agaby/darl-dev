/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ShiftParserAction.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;

namespace DarlCompiler.Parsing
{
    /// Class ShiftParserAction.
    /// </summary>
    public class ShiftParserAction : ParserAction
    {
        /// The term
        /// </summary>
        public readonly BnfTerm Term;
        /// The new state
        /// </summary>
        public readonly ParserState NewState;

        /// Initializes a new instance of the <see cref="ShiftParserAction"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public ShiftParserAction(Construction.LRItem item) : this(item.Core.Current, item.ShiftedItem.State) { }

        /// Initializes a new instance of the <see cref="ShiftParserAction"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="newState">The new state.</param>
        /// <exception cref="System.Exception">ParserShiftAction: newState may not be null. term:  + term.ToString()</exception>
        public ShiftParserAction(BnfTerm term, ParserState newState)
        {
            if (newState == null)
                throw new Exception("ParserShiftAction: newState may not be null. term: " + term.ToString());

            Term = term;
            NewState = newState;
        }

        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(ParsingContext context)
        {
            var currInput = context.CurrentParserInput;
            currInput.Term.OnShifting(context.SharedParsingEventArgs);
            context.ParserStack.Push(currInput, NewState);
            context.CurrentParserState = NewState;
            context.CurrentParserInput = null;
        }

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(Resources.LabelActionShift, NewState.Name);
        }

    }
}
