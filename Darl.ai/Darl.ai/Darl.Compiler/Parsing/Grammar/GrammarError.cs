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
// <copyright file="GrammarError.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Enum GrammarErrorLevel
    /// </summary>
    public enum GrammarErrorLevel
    {
        /// <summary>
        /// The no error
        /// </summary>
        NoError, //used only for max error level when there are no errors
        /// <summary>
        /// The information
        /// </summary>
        Info,
        /// <summary>
        /// The warning
        /// </summary>
        Warning,
        /// <summary>
        /// The conflict
        /// </summary>
        Conflict, //shift-reduce or reduce-reduce conflict
        /// <summary>
        /// The error
        /// </summary>
        Error,    //severe grammar error, parser construction cannot continue
        /// <summary>
        /// The internal error
        /// </summary>
        InternalError,  //internal Darl error
    }

    /// <summary>
    /// Class GrammarError.
    /// </summary>
    public class GrammarError
    {
        /// <summary>
        /// The level
        /// </summary>
        public readonly GrammarErrorLevel Level;
        /// <summary>
        /// The message
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The state
        /// </summary>
        public readonly ParserState State; //can be null!
        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarError"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        public GrammarError(GrammarErrorLevel level, ParserState state, string message)
        {
            Level = level;
            State = state;
            Message = message;
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Message + " (" + State + ")";
        }
    }

    /// <summary>
    /// Class GrammarErrorList.
    /// </summary>
    public class GrammarErrorList : List<GrammarError>
    {
        /// <summary>
        /// Adds the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Add(GrammarErrorLevel level, ParserState state, string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = String.Format(message, args);
            base.Add(new GrammarError(level, state, message));
        }
        /// <summary>
        /// Adds the and throw.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddAndThrow(GrammarErrorLevel level, ParserState state, string message, params object[] args)
        {
            Add(level, state, message, args);
            var error = this[this.Count - 1];
            var exc = new GrammarErrorException(error.Message, error);
            throw exc;
        }
        /// <summary>
        /// Gets the maximum level.
        /// </summary>
        /// <returns>GrammarErrorLevel.</returns>
        public GrammarErrorLevel GetMaxLevel()
        {
            var max = GrammarErrorLevel.NoError;
            foreach (var err in this)
                if (max < err.Level)
                    max = err.Level;
            return max;
        }
    }

    //Used to cancel parser construction when fatal error is found
    /// <summary>
    /// Class GrammarErrorException.
    /// </summary>
    [Serializable]
    public class GrammarErrorException : Exception
    {
        /// <summary>
        /// The error
        /// </summary>
        public readonly GrammarError Error;
        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarErrorException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error.</param>
        public GrammarErrorException(string message, GrammarError error)
            : base(message)
        {
            Error = error;
        }

    }


}
