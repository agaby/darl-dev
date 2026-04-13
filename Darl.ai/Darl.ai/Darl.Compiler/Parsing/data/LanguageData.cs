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
// <copyright file="LanguageData.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing.Construction;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class LanguageData.
    /// </summary>
    public partial class LanguageData
    {
        /// <summary>
        /// The grammar
        /// </summary>
        public readonly Grammar Grammar;
        /// <summary>
        /// The grammar data
        /// </summary>
        public readonly GrammarData GrammarData;
        /// <summary>
        /// The parser data
        /// </summary>
        public readonly ParserData ParserData;
        /// <summary>
        /// The scanner data
        /// </summary>
        public readonly ScannerData ScannerData;
        /// <summary>
        /// The errors
        /// </summary>
        public readonly GrammarErrorList Errors = new GrammarErrorList();
        /// <summary>
        /// The error level
        /// </summary>
        public GrammarErrorLevel ErrorLevel = GrammarErrorLevel.NoError;
        /// <summary>
        /// The construction time
        /// </summary>
        public long ConstructionTime;
        /// <summary>
        /// The ast data verified
        /// </summary>
        public bool AstDataVerified;

        /// <summary>
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
        /// <summary>
        /// Constructs all.
        /// </summary>
        public void ConstructAll()
        {
            var builder = new LanguageDataBuilder(this);
            builder.Build();
        }
        /// <summary>
        /// Determines whether this instance can parse.
        /// </summary>
        /// <returns><c>true</c> if this instance can parse; otherwise, <c>false</c>.</returns>
        public bool CanParse()
        {
            return ErrorLevel < GrammarErrorLevel.Error;
        }
    }
}
