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
// <copyright file="PreferredActionHint.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing.Construction;


namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class PreferredActionHint.
    /// </summary>
    public class PreferredActionHint : GrammarHint
    {
        /// <summary>
        /// The action type
        /// </summary>
        readonly PreferredActionType ActionType;
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferredActionHint"/> class.
        /// </summary>
        /// <param name="actionType">Type of the action.</param>
        public PreferredActionHint(PreferredActionType actionType)
        {
            ActionType = actionType;
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
        public override void Apply(LanguageData language, LRItem owner)
        {
            var state = owner.State;
            var conflicts = state.BuilderData.Conflicts;
            if (conflicts.Count == 0) return;
            switch (ActionType)
            {
                case PreferredActionType.Shift:
                    var currTerm = owner.Core.Current as Terminal;
                    if (currTerm == null || !conflicts.Contains(currTerm)) return; //nothing to do
                    //Current term for shift item (hint owner) is a conflict - resolve it with shift action
                    var shiftAction = new ShiftParserAction(owner);
                    state.Actions[currTerm] = shiftAction;
                    conflicts.Remove(currTerm);
                    return;
                case PreferredActionType.Reduce:
                    if (!owner.Core.IsFinal) return; //we take care of reduce items only here
                    //we have a reduce item with "Reduce" hint. Check if any of lookaheads are in conflict
                    ReduceParserAction reduceAction = null;
                    foreach (var lkhead in owner.Lookaheads)
                    {
                        if (conflicts.Contains(lkhead))
                        {
                            if (reduceAction == null)
                                reduceAction = new ReduceParserAction(owner.Core.Production);
                            state.Actions[lkhead] = reduceAction;
                            conflicts.Remove(lkhead);
                        }
                    }
                    return;
            }
        }
    }


}
