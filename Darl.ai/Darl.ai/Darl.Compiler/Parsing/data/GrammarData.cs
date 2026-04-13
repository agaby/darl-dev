/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="GrammarData.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DarlCompiler.Parsing
{

    //GrammarData is a container for all basic info about the grammar
    // GrammarData is a field in LanguageData object. 
    /// Class GrammarData.
    /// </summary>
    public class GrammarData
    {
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// The grammar
        /// </summary>
        public readonly Grammar Grammar;
        /// The augmented root
        /// </summary>
        public NonTerminal AugmentedRoot;
        /// The augmented snippet roots
        /// </summary>
        public NonTerminalSet AugmentedSnippetRoots = new NonTerminalSet();
        /// All terms
        /// </summary>
        public readonly BnfTermSet AllTerms = new BnfTermSet();
        /// The terminals
        /// </summary>
        public readonly TerminalSet Terminals = new TerminalSet();
        /// The non terminals
        /// </summary>
        public readonly NonTerminalSet NonTerminals = new NonTerminalSet();
        /// The no prefix terminals
        /// </summary>
        public TerminalSet NoPrefixTerminals = new TerminalSet(); //Terminals that have no limited set of prefixes

        /// Initializes a new instance of the <see cref="GrammarData"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public GrammarData(LanguageData language)
        {
            Language = language;
            Grammar = language.Grammar;
        }

    }



}
