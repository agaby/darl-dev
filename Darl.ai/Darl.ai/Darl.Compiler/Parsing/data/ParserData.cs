/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParserData.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;



namespace DarlCompiler.Parsing
{
    // ParserData is a container for all information used by CoreParser in input processing.
    // ParserData is a field in LanguageData structure and is used by CoreParser when parsing input. 
    // The state graph entry is InitialState state; the state graph encodes information usually contained 
    // in what is known in literature as transition/goto tables.
    // The graph is built from the language grammar by ParserDataBuilder. 
    using DarlCompiler.Parsing.Construction;
    /// Class ParserData.
    /// </summary>
    public class ParserData
    {
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// The initial state
        /// </summary>
        public ParserState InitialState; //main initial state
        /// The initial states
        /// </summary>
        public ParserStateTable InitialStates = new ParserStateTable(); // Lookup table: AugmRoot => InitialState
        /// The states
        /// </summary>
        public readonly ParserStateList States = new ParserStateList();
        /// The error action
        /// </summary>
        public ParserAction ErrorAction;
        /// Initializes a new instance of the <see cref="ParserData"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public ParserData(LanguageData language)
        {
            Language = language;
        }
    }

    /// Class ParserState.
    /// </summary>
    public partial class ParserState
    {
        /// The name
        /// </summary>
        public readonly string Name;
        /// The actions
        /// </summary>
        public readonly ParserActionTable Actions = new ParserActionTable();
        //Defined for states with a single reduce item; Parser.GetAction returns this action if it is not null.
        /// The default action
        /// </summary>
        public ParserAction DefaultAction;
        //Expected terms contains terminals is to be used in 
        //Parser-advise-to-Scanner facility would use it to filter current terminals when Scanner has more than one terminal for current char,
        //   it can ask Parser to filter the list using the ExpectedTerminals in current Parser state. 
        /// The expected terminals
        /// </summary>
        public readonly TerminalSet ExpectedTerminals = new TerminalSet();
        //Used for error reporting, we would use it to include list of expected terms in error message 
        // It is reduced compared to ExpectedTerms - some terms are "merged" into other non-terminals (with non-empty DisplayName)
        //   to make message shorter and cleaner. It is computed on-demand in CoreParser
        /// The reported expected set
        /// </summary>
        public StringSet ReportedExpectedSet;
        /// The builder data
        /// </summary>
        internal ParserStateData BuilderData; //transient, used only during automaton construction and may be cleared after that

        //Custom flags available for use by language/parser authors, to "mark" states in some way
        // Darl reserves the highest order byte for internal use
        /// The custom flags
        /// </summary>
        public int CustomFlags;

        /// Initializes a new instance of the <see cref="ParserState"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ParserState(string name)
        {
            Name = name;
        }
        /// Clears the data.
        /// </summary>
        public void ClearData()
        {
            BuilderData = null;
        }
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// Customs the flag is set.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CustomFlagIsSet(int flag)
        {
            return (CustomFlags & flag) != 0;
        }
    }

    /// Class ParserStateList.
    /// </summary>
    public class ParserStateList : List<ParserState> { }
    /// Class ParserStateSet.
    /// </summary>
    [Serializable]
    public class ParserStateSet : HashSet<ParserState> { }
    /// Class ParserStateHash.
    /// </summary>
    [Serializable]
    public class ParserStateHash : Dictionary<string, ParserState> { }
    /// Class ParserStateTable.
    /// </summary>
    [Serializable]
    public class ParserStateTable : Dictionary<NonTerminal, ParserState> { }

    /// Enum ProductionFlags
    /// </summary>
    [Flags]
    public enum ProductionFlags
    {
        /// The none
        /// </summary>
        None = 0,
        /// The has terminals
        /// </summary>
        HasTerminals = 0x02, //contains terminal
        /// The is error
        /// </summary>
        IsError = 0x04,      //contains Error terminal
        /// The is empty
        /// </summary>
        IsEmpty = 0x08,
    }

    /// Class Production.
    /// </summary>
    public partial class Production
    {
        /// The flags
        /// </summary>
        public ProductionFlags Flags;
        /// The l value
        /// </summary>
        public readonly NonTerminal LValue;                              // left-side element
        /// The r values
        /// </summary>
        public readonly BnfTermList RValues = new BnfTermList();         //the right-side elements sequence
        /// The l r0 items
        /// </summary>
        internal readonly Construction.LR0ItemList LR0Items = new Construction.LR0ItemList();        //LR0 items based on this production 

        /// Initializes a new instance of the <see cref="Production"/> class.
        /// </summary>
        /// <param name="lvalue">The lvalue.</param>
        public Production(NonTerminal lvalue)
        {
            LValue = lvalue;
        }

        /// To the string quoted.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToStringQuoted()
        {
            return "'" + ToString() + "'";
        }
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ProductionToString(this, -1); //no dot
        }
        /// Productions to string.
        /// </summary>
        /// <param name="production">The production.</param>
        /// <param name="dotPosition">The dot position.</param>
        /// <returns>System.String.</returns>
        public static string ProductionToString(Production production, int dotPosition)
        {
            char dotChar = '\u00B7'; //dot in the middle of the line
            StringBuilder bld = new StringBuilder();
            bld.Append(production.LValue.Name);
            bld.Append(" -> ");
            for (int i = 0; i < production.RValues.Count; i++)
            {
                if (i == dotPosition)
                    bld.Append(dotChar);
                bld.Append(production.RValues[i].Name);
                bld.Append(" ");
            }//for i
            if (dotPosition == production.RValues.Count)
                bld.Append(dotChar);
            return bld.ToString();
        }

    }

    /// Class ProductionList.
    /// </summary>
    public class ProductionList : List<Production> { }


}
