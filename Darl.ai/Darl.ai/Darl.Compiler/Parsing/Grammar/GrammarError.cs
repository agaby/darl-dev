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
    /// Enum GrammarErrorLevel
    /// </summary>
    public enum GrammarErrorLevel
    {
        /// The no error
        /// </summary>
        NoError, //used only for max error level when there are no errors
        /// The information
        /// </summary>
        Info,
        /// The warning
        /// </summary>
        Warning,
        /// The conflict
        /// </summary>
        Conflict, //shift-reduce or reduce-reduce conflict
        /// The error
        /// </summary>
        Error,    //severe grammar error, parser construction cannot continue
        /// The internal error
        /// </summary>
        InternalError,  //internal Darl error
    }

    /// Class GrammarError.
    /// </summary>
    public class GrammarError
    {
        /// The level
        /// </summary>
        public readonly GrammarErrorLevel Level;
        /// The message
        /// </summary>
        public readonly string Message;
        /// The state
        /// </summary>
        public readonly ParserState State; //can be null!
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Message + " (" + State + ")";
        }
    }

    /// Class GrammarErrorList.
    /// </summary>
    public class GrammarErrorList : List<GrammarError>
    {
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
    /// Class GrammarErrorException.
    /// </summary>
    [Serializable]
    public class GrammarErrorException : Exception
    {
        /// The error
        /// </summary>
        public readonly GrammarError Error;
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
