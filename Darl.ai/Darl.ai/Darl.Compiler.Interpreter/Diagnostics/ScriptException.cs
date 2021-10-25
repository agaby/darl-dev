// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScriptException.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using System;

namespace DarlCompiler.Interpreter
{
    /// <summary>
    /// Class ScriptException.
    /// </summary>
    [Serializable]
    public class ScriptException : Exception
    {
        /// <summary>
        /// The location
        /// </summary>
        public SourceLocation Location;
        /// <summary>
        /// The script stack trace
        /// </summary>
        public ScriptStackTrace ScriptStackTrace;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ScriptException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ScriptException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="location">The location.</param>
        /// <param name="stack">The stack.</param>
        public ScriptException(string message, Exception inner, SourceLocation location, ScriptStackTrace stack)
            : base(message, inner)
        {
            Location = location;
            ScriptStackTrace = stack;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Message + Environment.NewLine + ScriptStackTrace.ToString();
        }
    }

}
