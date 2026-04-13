/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="PrecedenceHint.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing.Construction;
using System.Collections.Generic;
using System.Linq;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// A hint to use precedence.
    /// </summary>
    /// <remarks>Not used directly in grammars; injected automatically by system in states having conflicts on operator symbols.
    /// The purpose of the hint is make handling precedence similar to other conflict resolution methods - through hints
    /// activated during parser construction. The hint code analyzes the conflict and resolves it by adding custom or general action
    /// for a conflicting input.</remarks>
    public class PrecedenceHint : GrammarHint
    {
        /// <summary>
        /// Gives a chance to a custom code in hint to interfere in parser automaton construction.
        /// </summary>
        /// <param name="language">The LanguageData instance.</param>
        /// <param name="owner">The LRItem that "owns" the hint.</param>
        /// <remarks>The most common purpose of this method (it's overrides) is to resolve the conflicts
        /// by adding specific actions into State.Actions dictionary.
        /// The owner parameter represents the position in the grammar expression where the hint
        /// is found. The parser state is available through owner.State property.</remarks>
        public override void Apply(LanguageData language, LRItem owner)
        {
            var state = owner.State;
            var allConflicts = state.BuilderData.Conflicts;
            if (allConflicts.Count == 0)
                return;
            //Find all conflicts that can be resolved by operator precedence
            // SL does not support Find extension, so we do it with explicit loop
            var operConflicts = new List<Terminal>();
            foreach (var c in allConflicts)
                if (c.Flags.IsSet(TermFlags.IsOperator))
                    operConflicts.Add(c);
            foreach (var conflict in operConflicts)
            {
                var newState = state.BuilderData.GetNextState(conflict);
                var reduceItems = state.BuilderData.ReduceItems.SelectByLookahead(conflict).ToList();
                if (newState == null || reduceItems.Count != 1)
                    continue; // this cannot be fixed by precedence
                state.Actions[conflict] = new PrecedenceBasedParserAction(conflict, newState, reduceItems[0].Core.Production);
                allConflicts.Remove(conflict);
            }//foreach conflict
        }

    }



}
