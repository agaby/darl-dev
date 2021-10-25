// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="FixedLengthLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{

    //A terminal for representing fixed-length lexemes coming up sometimes in programming language
    // (in Fortran for ex, every line starts with 5-char label, followed by a single continuation char)
    // It may be also used to create grammar/parser for reading data files with fixed length fields
    /// <summary>
    /// Class FixedLengthLiteral.
    /// </summary>
    public class FixedLengthLiteral : DataLiteralBase
    {
        /// <summary>
        /// The length
        /// </summary>
        public int Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        /// <param name="dataType">Type of the data.</param>
        public FixedLengthLiteral(string name, int length, TypeCode dataType)
            : base(name, dataType)
        {
            Length = length;
        }

        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected override string ReadBody(ParsingContext context, ISourceStream source)
        {
            source.PreviewPosition = source.Location.Position + Length;
            var body = source.Text.Substring(source.Location.Position, Length);
            return body;
        }

    }

}
