/// <summary>
/// ParserData.cs - Core module for the Darl.dev project.
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
    /// <summary>
    /// Class ParserData.
    /// </summary>
    public class ParserData
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// <summary>
        /// The initial state
        /// </summary>
        public ParserState InitialState; //main initial state
        /// <summary>
        /// The initial states
        /// </summary>
        public ParserStateTable InitialStates = new ParserStateTable(); // Lookup table: AugmRoot => InitialState
        /// <summary>
        /// The states
        /// </summary>
        public readonly ParserStateList States = new ParserStateList();
        /// <summary>
        /// The error action
        /// </summary>
        public ParserAction ErrorAction;
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserData"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public ParserData(LanguageData language)
        {
            Language = language;
        }
    }

    /// <summary>
    /// Class ParserState.
    /// </summary>
    public partial class ParserState
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The actions
        /// </summary>
        public readonly ParserActionTable Actions = new ParserActionTable();
        //Defined for states with a single reduce item; Parser.GetAction returns this action if it is not null.
        /// <summary>
        /// The default action
        /// </summary>
        public ParserAction DefaultAction;
        //Expected terms contains terminals is to be used in 
        //Parser-advise-to-Scanner facility would use it to filter current terminals when Scanner has more than one terminal for current char,
        //   it can ask Parser to filter the list using the ExpectedTerminals in current Parser state. 
        /// <summary>
        /// The expected terminals
        /// </summary>
        public readonly TerminalSet ExpectedTerminals = new TerminalSet();
        //Used for error reporting, we would use it to include list of expected terms in error message 
        // It is reduced compared to ExpectedTerms - some terms are "merged" into other non-terminals (with non-empty DisplayName)
        //   to make message shorter and cleaner. It is computed on-demand in CoreParser
        /// <summary>
        /// The reported expected set
        /// </summary>
        public StringSet ReportedExpectedSet;
        /// <summary>
        /// The builder data
        /// </summary>
        internal ParserStateData BuilderData; //transient, used only during automaton construction and may be cleared after that

        //Custom flags available for use by language/parser authors, to "mark" states in some way
        // Darl reserves the highest order byte for internal use
        /// <summary>
        /// The custom flags
        /// </summary>
        public int CustomFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserState"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ParserState(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Clears the data.
        /// </summary>
        public void ClearData()
        {
            BuilderData = null;
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// Customs the flag is set.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CustomFlagIsSet(int flag)
        {
            return (CustomFlags & flag) != 0;
        }
    }

    /// <summary>
    /// Class ParserStateList.
    /// </summary>
    public class ParserStateList : List<ParserState> { }
    /// <summary>
    /// Class ParserStateSet.
    /// </summary>
    [Serializable]
    public class ParserStateSet : HashSet<ParserState> { }
    /// <summary>
    /// Class ParserStateHash.
    /// </summary>
    [Serializable]
    public class ParserStateHash : Dictionary<string, ParserState> { }
    /// <summary>
    /// Class ParserStateTable.
    /// </summary>
    [Serializable]
    public class ParserStateTable : Dictionary<NonTerminal, ParserState> { }

    /// <summary>
    /// Enum ProductionFlags
    /// </summary>
    [Flags]
    public enum ProductionFlags
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The has terminals
        /// </summary>
        HasTerminals = 0x02, //contains terminal
        /// <summary>
        /// The is error
        /// </summary>
        IsError = 0x04,      //contains Error terminal
        /// <summary>
        /// The is empty
        /// </summary>
        IsEmpty = 0x08,
    }

    /// <summary>
    /// Class Production.
    /// </summary>
    public partial class Production
    {
        /// <summary>
        /// The flags
        /// </summary>
        public ProductionFlags Flags;
        /// <summary>
        /// The l value
        /// </summary>
        public readonly NonTerminal LValue;                              // left-side element
        /// <summary>
        /// The r values
        /// </summary>
        public readonly BnfTermList RValues = new BnfTermList();         //the right-side elements sequence
        /// <summary>
        /// The l r0 items
        /// </summary>
        internal readonly Construction.LR0ItemList LR0Items = new Construction.LR0ItemList();        //LR0 items based on this production 

        /// <summary>
        /// Initializes a new instance of the <see cref="Production"/> class.
        /// </summary>
        /// <param name="lvalue">The lvalue.</param>
        public Production(NonTerminal lvalue)
        {
            LValue = lvalue;
        }

        /// <summary>
        /// To the string quoted.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToStringQuoted()
        {
            return "'" + ToString() + "'";
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ProductionToString(this, -1); //no dot
        }
        /// <summary>
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

    /// <summary>
    /// Class ProductionList.
    /// </summary>
    public class ProductionList : List<Production> { }


}
