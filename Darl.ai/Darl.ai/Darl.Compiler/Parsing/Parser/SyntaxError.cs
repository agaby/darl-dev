// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SyntaxError.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    //Container for syntax error
    /// <summary>
    /// Class SyntaxError.
    /// </summary>
    public class SyntaxError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxError"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <param name="parserState">State of the parser.</param>
        public SyntaxError(SourceLocation location, string message, ParserState parserState)
        {
            Location = location;
            Message = message;
            ParserState = parserState;
        }

        /// <summary>
        /// The location
        /// </summary>
        public readonly SourceLocation Location;
        /// <summary>
        /// The message
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The parser state
        /// </summary>
        public ParserState ParserState;

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
    /// Class SyntaxErrorList.
    /// </summary>
    public class SyntaxErrorList : List<SyntaxError>
    {
        /// <summary>
        /// Bies the location.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        public static int ByLocation(SyntaxError x, SyntaxError y)
        {
            return SourceLocation.Compare(x.Location, y.Location);
        }
    }

}
