// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParserDataBuilder_HelperClasses.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;


namespace DarlCompiler.Parsing.Construction
{

    /// Class ParserStateData.
    /// </summary>
    public class ParserStateData
    {
        /// The state
        /// </summary>
        public readonly ParserState State;
        /// All items
        /// </summary>
        public readonly LRItemSet AllItems = new LRItemSet();
        /// The shift items
        /// </summary>
        public readonly LRItemSet ShiftItems = new LRItemSet();
        /// The reduce items
        /// </summary>
        public readonly LRItemSet ReduceItems = new LRItemSet();
        /// The initial items
        /// </summary>
        public readonly LRItemSet InitialItems = new LRItemSet();
        /// The shift terms
        /// </summary>
        public readonly BnfTermSet ShiftTerms = new BnfTermSet();
        /// The shift terminals
        /// </summary>
        public readonly TerminalSet ShiftTerminals = new TerminalSet();
        /// The conflicts
        /// </summary>
        public readonly TerminalSet Conflicts = new TerminalSet();
        /// The is inadequate
        /// </summary>
        public readonly bool IsInadequate;
        /// All cores
        /// </summary>
        public LR0ItemSet AllCores = new LR0ItemSet();

        //used for creating canonical states from core set
        /// Initializes a new instance of the <see cref="ParserStateData"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="kernelCores">The kernel cores.</param>
        public ParserStateData(ParserState state, LR0ItemSet kernelCores)
        {
            State = state;
            foreach (var core in kernelCores)
                AddItem(core);
            IsInadequate = ReduceItems.Count > 1 || ReduceItems.Count == 1 && ShiftItems.Count > 0;
        }

        /// Adds the item.
        /// </summary>
        /// <param name="core">The core.</param>
        public void AddItem(LR0Item core)
        {
            //Check if a core had been already added. If yes, simply return
            if (!AllCores.Add(core)) return;
            //Create new item, add it to AllItems, InitialItems, ReduceItems or ShiftItems
            var item = new LRItem(State, core);
            AllItems.Add(item);
            if (item.Core.IsFinal)
                ReduceItems.Add(item);
            else
                ShiftItems.Add(item);
            if (item.Core.IsInitial)
                InitialItems.Add(item);
            if (core.IsFinal) return;
            //Add current term to ShiftTerms
            if (!ShiftTerms.Add(core.Current)) return;
            if (core.Current is Terminal)
                ShiftTerminals.Add(core.Current as Terminal);
            //If current term (core.Current) is a new non-terminal, expand it
            var currNt = core.Current as NonTerminal;
            if (currNt == null) return;
            foreach (var prod in currNt.Productions)
                AddItem(prod.LR0Items[0]);
        }

        /// Gets the transitions.
        /// </summary>
        /// <value>The transitions.</value>
        public TransitionTable Transitions
        {
            get
            {
                if (_transitions == null)
                    _transitions = new TransitionTable();
                return _transitions;
            }
        }
        TransitionTable _transitions;

        //A set of states reachable through shifts over nullable non-terminals. Computed on demand
        /// Gets the read state set.
        /// </summary>
        /// <value>The read state set.</value>
        public ParserStateSet ReadStateSet
        {
            get
            {
                if (_readStateSet == null)
                {
                    _readStateSet = new ParserStateSet();
                    foreach (var shiftTerm in State.BuilderData.ShiftTerms)
                        if (shiftTerm.Flags.IsSet(TermFlags.IsNullable))
                        {
                            var shift = State.Actions[shiftTerm] as ShiftParserAction;
                            var targetState = shift.NewState;
                            _readStateSet.Add(targetState);
                            _readStateSet.UnionWith(targetState.BuilderData.ReadStateSet); //we shouldn't get into loop here, the chain of reads is finite
                        }
                }
                return _readStateSet;
            }
        }
        ParserStateSet _readStateSet;

        /// Gets the state of the next.
        /// </summary>
        /// <param name="shiftTerm">The shift term.</param>
        /// <returns>ParserState.</returns>
        public ParserState GetNextState(BnfTerm shiftTerm)
        {
            var shift = ShiftItems.FirstOrDefault(item => item.Core.Current == shiftTerm);
            if (shift == null) return null;
            return shift.ShiftedItem.State;
        }

        /// Gets the shift reduce conflicts.
        /// </summary>
        /// <returns>TerminalSet.</returns>
        public TerminalSet GetShiftReduceConflicts()
        {
            var result = new TerminalSet();
            result.UnionWith(Conflicts);
            result.IntersectWith(ShiftTerminals);
            return result;
        }
        /// Gets the reduce reduce conflicts.
        /// </summary>
        /// <returns>TerminalSet.</returns>
        public TerminalSet GetReduceReduceConflicts()
        {
            var result = new TerminalSet();
            result.UnionWith(Conflicts);
            result.ExceptWith(ShiftTerminals);
            return result;
        }

    }

    //An object representing inter-state transitions. Defines Includes, IncludedBy that are used for efficient lookahead computation 
    /// Class Transition.
    /// </summary>
    public class Transition
    {
        /// From state
        /// </summary>
        public readonly ParserState FromState;
        /// To state
        /// </summary>
        public readonly ParserState ToState;
        /// The over non terminal
        /// </summary>
        public readonly NonTerminal OverNonTerminal;
        /// The items
        /// </summary>
        public readonly LRItemSet Items;
        /// The includes
        /// </summary>
        public readonly TransitionSet Includes = new TransitionSet();
        /// The included by
        /// </summary>
        public readonly TransitionSet IncludedBy = new TransitionSet();

        /// The _hash code
        /// </summary>
        readonly int _hashCode;

        /// Initializes a new instance of the <see cref="Transition"/> class.
        /// </summary>
        /// <param name="fromState">From state.</param>
        /// <param name="overNonTerminal">The over non terminal.</param>
        public Transition(ParserState fromState, NonTerminal overNonTerminal)
        {
            FromState = fromState;
            OverNonTerminal = overNonTerminal;
            ToState = FromState.BuilderData.GetNextState(overNonTerminal);
            _hashCode = unchecked(FromState.GetHashCode() - overNonTerminal.GetHashCode());
            FromState.BuilderData.Transitions.Add(overNonTerminal, this);
            Items = FromState.BuilderData.ShiftItems.SelectByCurrent(overNonTerminal);
            foreach (var item in Items)
            {
                item.Transition = this;
            }

        }

        /// Includes the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        public void Include(Transition other)
        {
            if (other == this) return;
            if (!IncludeTransition(other)) return;
            //include children
            foreach (var child in other.Includes)
            {
                IncludeTransition(child);
            }
        }
        /// Includes the transition.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool IncludeTransition(Transition other)
        {
            if (!Includes.Add(other)) return false;
            other.IncludedBy.Add(this);
            //propagate "up"
            foreach (var incBy in IncludedBy)
                incBy.IncludeTransition(other);
            return true;
        }

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return FromState.Name + " -> (over " + OverNonTerminal.Name + ") -> " + ToState.Name;
        }
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

    /// Class TransitionSet.
    /// </summary>
    [Serializable]
    public class TransitionSet : HashSet<Transition> { }
    /// Class TransitionList.
    /// </summary>
    public class TransitionList : List<Transition> { }
    /// Class TransitionTable.
    /// </summary>
    [Serializable]
    public class TransitionTable : Dictionary<NonTerminal, Transition> { }

    /// Class LRItem.
    /// </summary>
    public class LRItem
    {
        /// The state
        /// </summary>
        public readonly ParserState State;
        /// The core
        /// </summary>
        public readonly LR0Item Core;
        //these properties are used in lookahead computations
        /// The shifted item
        /// </summary>
        public LRItem ShiftedItem;
        /// The transition
        /// </summary>
        public Transition Transition;

        /// The _hash code
        /// </summary>
        readonly int _hashCode;

        //Lookahead info for reduce items
        /// The lookbacks
        /// </summary>
        public TransitionSet Lookbacks = new TransitionSet();
        /// The lookaheads
        /// </summary>
        public TerminalSet Lookaheads = new TerminalSet();

        /// Initializes a new instance of the <see cref="LRItem"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="core">The core.</param>
        public LRItem(ParserState state, LR0Item core)
        {
            State = state;
            Core = core;
            _hashCode = unchecked(state.GetHashCode() + core.GetHashCode());
        }
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Core.ToString();
        }
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// Gets the lookaheads in conflict.
        /// </summary>
        /// <returns>TerminalSet.</returns>
        public TerminalSet GetLookaheadsInConflict()
        {
            var lkhc = new TerminalSet();
            lkhc.UnionWith(Lookaheads);
            lkhc.IntersectWith(State.BuilderData.Conflicts);
            return lkhc;
        }

    }//LRItem class

    /// Class LRItemList.
    /// </summary>
    public class LRItemList : List<LRItem> { }

    /// Class LRItemSet.
    /// </summary>
    [Serializable]
    public class LRItemSet : HashSet<LRItem>
    {

        /// Finds the by core.
        /// </summary>
        /// <param name="core">The core.</param>
        /// <returns>LRItem.</returns>
        public LRItem FindByCore(LR0Item core)
        {
            foreach (LRItem item in this)
                if (item.Core == core) return item;
            return null;
        }
        /// Selects the by current.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <returns>LRItemSet.</returns>
        public LRItemSet SelectByCurrent(BnfTerm current)
        {
            var result = new LRItemSet();
            foreach (var item in this)
                if (item.Core.Current == current)
                    result.Add(item);
            return result;
        }

        /// Gets the shifted cores.
        /// </summary>
        /// <returns>LR0ItemSet.</returns>
        public LR0ItemSet GetShiftedCores()
        {
            var result = new LR0ItemSet();
            foreach (var item in this)
                if (item.Core.ShiftedItem != null)
                    result.Add(item.Core.ShiftedItem);
            return result;
        }
        /// Selects the by lookahead.
        /// </summary>
        /// <param name="lookahead">The lookahead.</param>
        /// <returns>LRItemSet.</returns>
        public LRItemSet SelectByLookahead(Terminal lookahead)
        {
            var result = new LRItemSet();
            foreach (var item in this)
                if (item.Lookaheads.Contains(lookahead))
                    result.Add(item);
            return result;
        }

    }

    /// Class LR0Item.
    /// </summary>
    public partial class LR0Item
    {
        /// The production
        /// </summary>
        public readonly Production Production;
        /// The position
        /// </summary>
        public readonly int Position;
        /// The current
        /// </summary>
        public readonly BnfTerm Current;
        /// The tail is nullable
        /// </summary>
        public bool TailIsNullable;
        /// The hints
        /// </summary>
        public GrammarHintList Hints = new GrammarHintList();

        //automatically generated IDs - used for building keys for lists of kernel LR0Items
        // which in turn are used to quickly lookup parser states in hash
        /// The identifier
        /// </summary>
        internal readonly int ID;

        /// Initializes a new instance of the <see cref="LR0Item"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="production">The production.</param>
        /// <param name="position">The position.</param>
        /// <param name="hints">The hints.</param>
        public LR0Item(int id, Production production, int position, GrammarHintList hints)
        {
            ID = id;
            Production = production;
            Position = position;
            Current = (Position < Production.RValues.Count) ? Production.RValues[Position] : null;
            if (hints != null)
                Hints.AddRange(hints);
            _hashCode = ID.ToString().GetHashCode();
        }

        /// Gets the shifted item.
        /// </summary>
        /// <value>The shifted item.</value>
        public LR0Item ShiftedItem
        {
            get
            {
                if (Position >= Production.LR0Items.Count - 1)
                    return null;
                else
                    return Production.LR0Items[Position + 1];
            }
        }
        /// Gets a value indicating whether this instance is kernel.
        /// </summary>
        /// <value><c>true</c> if this instance is kernel; otherwise, <c>false</c>.</value>
        public bool IsKernel
        {
            get { return Position > 0; }
        }
        /// Gets a value indicating whether this instance is initial.
        /// </summary>
        /// <value><c>true</c> if this instance is initial; otherwise, <c>false</c>.</value>
        public bool IsInitial
        {
            get { return Position == 0; }
        }
        /// Gets a value indicating whether this instance is final.
        /// </summary>
        /// <value><c>true</c> if this instance is final; otherwise, <c>false</c>.</value>
        public bool IsFinal
        {
            get { return Position == Production.RValues.Count; }
        }
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Production.ProductionToString(Production, Position);
        }
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
            /// The _hash code
            /// </summary>
        }

        readonly int _hashCode;

    }

    /// Class LR0ItemList.
    /// </summary>
    public class LR0ItemList : List<LR0Item> { }
    /// Class LR0ItemSet.
    /// </summary>
    [Serializable]
    public class LR0ItemSet : HashSet<LR0Item> { }



}
