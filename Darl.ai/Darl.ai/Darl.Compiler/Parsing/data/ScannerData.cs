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
// <copyright file="ScannerData.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class TerminalLookupTable.
    /// </summary>
    [Serializable]
    public class TerminalLookupTable : Dictionary<char, TerminalList> { }

    // ScannerData is a container for all detailed info needed by scanner to read input. 
    /// <summary>
    /// Class ScannerData.
    /// </summary>
    public class ScannerData
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// <summary>
        /// The terminals lookup
        /// </summary>
        public readonly TerminalLookupTable TerminalsLookup = new TerminalLookupTable(); //hash table for fast terminal lookup by input char
        /// <summary>
        /// The multiline terminals
        /// </summary>
        public readonly TerminalList MultilineTerminals = new TerminalList();
        /// <summary>
        /// The no prefix terminals
        /// </summary>
        public TerminalList NoPrefixTerminals = new TerminalList(); //Terminals with no limited set of prefixes, copied from GrammarData 
        //hash table for fast lookup of non-grammar terminals by input char
        /// <summary>
        /// The non grammar terminals lookup
        /// </summary>
        public readonly TerminalLookupTable NonGrammarTerminalsLookup = new TerminalLookupTable();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerData"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public ScannerData(LanguageData language)
        {
            Language = language;
        }
    }

}
