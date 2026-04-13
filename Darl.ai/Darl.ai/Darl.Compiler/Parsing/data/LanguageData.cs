/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LanguageData.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing.Construction;

namespace DarlCompiler.Parsing
{
    /// Class LanguageData.
    /// </summary>
    public partial class LanguageData
    {
        /// The grammar
        /// </summary>
        public readonly Grammar Grammar;
        /// The grammar data
        /// </summary>
        public readonly GrammarData GrammarData;
        /// The parser data
        /// </summary>
        public readonly ParserData ParserData;
        /// The scanner data
        /// </summary>
        public readonly ScannerData ScannerData;
        /// The errors
        /// </summary>
        public readonly GrammarErrorList Errors = new GrammarErrorList();
        /// The error level
        /// </summary>
        public GrammarErrorLevel ErrorLevel = GrammarErrorLevel.NoError;
        /// The construction time
        /// </summary>
        public long ConstructionTime;
        /// The ast data verified
        /// </summary>
        public bool AstDataVerified;

        /// Initializes a new instance of the <see cref="LanguageData"/> class.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        public LanguageData(Grammar grammar)
        {
            Grammar = grammar;
            GrammarData = new GrammarData(this);
            ParserData = new ParserData(this);
            ScannerData = new ScannerData(this);
            ConstructAll();
        }
        /// Constructs all.
        /// </summary>
        public void ConstructAll()
        {
            var builder = new LanguageDataBuilder(this);
            builder.Build();
        }
        /// Determines whether this instance can parse.
        /// </summary>
        /// <returns><c>true</c> if this instance can parse; otherwise, <c>false</c>.</returns>
        public bool CanParse()
        {
            return ErrorLevel < GrammarErrorLevel.Error;
        }
    }
}
