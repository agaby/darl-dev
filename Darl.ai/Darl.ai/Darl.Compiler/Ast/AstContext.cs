// ***********************************************************************
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
    /// <summary>
    /// Class AstContext.
    /// </summary>
    public class AstContext
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;

        public Dictionary<string, ILocalStore> Stores;
        /// <summary>
        /// The default node type
        /// </summary>
        public Type DefaultNodeType;
        /// <summary>
        /// The default literal node type
        /// </summary>
        public Type DefaultLiteralNodeType; //default node type for literals
        /// <summary>
        /// The default identifier node type
        /// </summary>
        public Type DefaultIdentifierNodeType; //default node type for identifiers
        /// <summary>
        /// The current namespace
        /// </summary>
        public string CurrentNamespace = string.Empty; //added for compatibility

        /// <summary>
        /// The values
        /// </summary>
        public Dictionary<object, object> Values = new Dictionary<object, object>();
        /// <summary>
        /// The messages
        /// </summary>
        public LogMessageList Messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="AstContext"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public AstContext(LanguageData language, Dictionary<string, ILocalStore>? stores = null)
        {
            Stores = stores;
            Language = language;
        }

        /// <summary>
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
