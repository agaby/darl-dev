// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="GrammarEnums.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Enum LanguageFlags
    /// </summary>
    [Flags]
    public enum LanguageFlags
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        //Compilation options
        //Be careful - use this flag ONLY if you use NewLine terminal in grammar explicitly!
        // - it happens only in line-based languages like Basic.
        /// <summary>
        /// The new line before EOF
        /// </summary>
        NewLineBeforeEOF = 0x01,
        //Emit LineStart token
        /// <summary>
        /// The emit line start token
        /// </summary>
        EmitLineStartToken = 0x02,
        /// <summary>
        /// The disable scanner parser link
        /// </summary>
        DisableScannerParserLink = 0x04, //in grammars that define TokenFilters (like Python) this flag should be set
        /// <summary>
        /// The create ast
        /// </summary>
        CreateAst = 0x08, //create AST nodes 

        //Runtime
        /// <summary>
        /// The supports command line
        /// </summary>
        SupportsCommandLine = 0x0200,
        /// <summary>
        /// The tail recursive
        /// </summary>
        TailRecursive = 0x0400, //Tail-recursive language - Scheme is one example
        /// <summary>
        /// The supports big int
        /// </summary>
        SupportsBigInt = 0x01000,
        /// <summary>
        /// The supports complex
        /// </summary>
        SupportsComplex = 0x02000,
        /// <summary>
        /// The supports rational
        /// </summary>
        SupportsRational = 0x04000,

        //Default value
        /// <summary>
        /// The default
        /// </summary>
        Default = None,
    }

    //Operator associativity types
    /// <summary>
    /// Enum Associativity
    /// </summary>
    public enum Associativity
    {
        /// <summary>
        /// The left
        /// </summary>
        Left,
        /// <summary>
        /// The right
        /// </summary>
        Right,
        /// <summary>
        /// The neutral
        /// </summary>
        Neutral  //honestly don't know what that means, but it is mentioned in literature 
    }

    //Used by Make-list-rule methods
    /// <summary>
    /// Enum TermListOptions
    /// </summary>
    [Flags]
    public enum TermListOptions
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The allow empty
        /// </summary>
        AllowEmpty = 0x01,
        /// <summary>
        /// The allow trailing delimiter
        /// </summary>
        AllowTrailingDelimiter = 0x02,

        // In some cases this hint would help to resolve the conflicts that come up when you have two lists separated by a nullable term.
        // This hint would resolve the conflict, telling the parser to include as many as possible elements in the first list, and the rest, 
        // if any, would go to the second list. By default, this flag is included in Star and Plus lists. 
        /// <summary>
        /// The add prefer shift hint
        /// </summary>
        AddPreferShiftHint = 0x04,
        //Combinations - use these 
        /// <summary>
        /// The plus list
        /// </summary>
        PlusList = AddPreferShiftHint,
        /// <summary>
        /// The star list
        /// </summary>
        StarList = AllowEmpty | AddPreferShiftHint,
    }

}
