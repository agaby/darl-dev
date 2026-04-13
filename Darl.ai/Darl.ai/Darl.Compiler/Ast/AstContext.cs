/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AstContext.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Ast
{
    /// Class AstContext.
    /// </summary>
    public class AstContext
    {
        /// The language
        /// </summary>
        public readonly LanguageData Language;

        public Dictionary<string, ILocalStore> Stores;
        /// The default node type
        /// </summary>
        public Type DefaultNodeType;
        /// The default literal node type
        /// </summary>
        public Type DefaultLiteralNodeType; //default node type for literals
        /// The default identifier node type
        /// </summary>
        public Type DefaultIdentifierNodeType; //default node type for identifiers
        /// The current namespace
        /// </summary>
        public string CurrentNamespace = string.Empty; //added for compatibility

        /// The values
        /// </summary>
        public Dictionary<object, object> Values = new Dictionary<object, object>();
        /// The messages
        /// </summary>
        public LogMessageList Messages;

        /// Initializes a new instance of the <see cref="AstContext"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public AstContext(LanguageData language, Dictionary<string, ILocalStore>? stores = null)
        {
            Stores = stores;
            Language = language;
        }

        /// Adds the message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddMessage(ErrorLevel level, SourceLocation location, string message, params object[]? args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            Messages.Add(new LogMessage(level, location, message, null));
        }

    }
}
