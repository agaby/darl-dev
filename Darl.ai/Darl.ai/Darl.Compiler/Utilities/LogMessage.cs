// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LogMessage.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace DarlCompiler
{

    /// Enum ErrorLevel
    /// </summary>
    public enum ErrorLevel
    {
        /// The information
        /// </summary>
        Info = 0,
        /// The warning
        /// </summary>
        Warning = 1,
        /// The error
        /// </summary>
        Error = 2,
    }

    //Container for syntax errors and warnings
    /// Class LogMessage.
    /// </summary>
    public class LogMessage
    {
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

        /// The level
        /// </summary>
        public readonly ErrorLevel Level;
        /// The parser state
        /// </summary>
        public readonly ParserState ParserState;
        /// The location
        /// </summary>
        public readonly SourceLocation Location;
        /// The message
        /// </summary>
        public readonly string Message;

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Message;
        }
    }

    /// Class LogMessageList.
    /// </summary>
    public class LogMessageList : List<LogMessage>
    {
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
