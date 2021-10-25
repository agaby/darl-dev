// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LogMessage.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

using DarlCompiler.Parsing;

namespace DarlCompiler
{

    /// <summary>
    /// Enum ErrorLevel
    /// </summary>
    public enum ErrorLevel
    {
        /// <summary>
        /// The information
        /// </summary>
        Info = 0,
        /// <summary>
        /// The warning
        /// </summary>
        Warning = 1,
        /// <summary>
        /// The error
        /// </summary>
        Error = 2,
    }

    //Container for syntax errors and warnings
    /// <summary>
    /// Class LogMessage.
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <param name="parserState">State of the parser.</param>
        public LogMessage(ErrorLevel level, SourceLocation location, string message, ParserState parserState)
        {
            Level = level;
            Location = location;
            Message = message;
            ParserState = parserState;
        }

        /// <summary>
        /// The level
        /// </summary>
        public readonly ErrorLevel Level;
        /// <summary>
        /// The parser state
        /// </summary>
        public readonly ParserState ParserState;
        /// <summary>
        /// The location
        /// </summary>
        public readonly SourceLocation Location;
        /// <summary>
        /// The message
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// Class LogMessageList.
    /// </summary>
    public class LogMessageList : List<LogMessage>
    {
        /// <summary>
        /// Bies the location.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        public static int ByLocation(LogMessage x, LogMessage y)
        {
            return SourceLocation.Compare(x.Location, y.Location);
        }
    }

}
