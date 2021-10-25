// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="QuotedValueLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    //Terminal for reading values enclosed in a pair of start/end characters. For ex, date literal #15/10/2009# in VB
    /// <summary>
    /// Class QuotedValueLiteral.
    /// </summary>
    public class QuotedValueLiteral : DataLiteralBase
    {
        /// <summary>
        /// The start symbol
        /// </summary>
        public string StartSymbol;
        /// <summary>
        /// The end symbol
        /// </summary>
        public string EndSymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotedValueLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="dataType">Type of the data.</param>
        public QuotedValueLiteral(string name, string startEndSymbol, TypeCode dataType) : this(name, startEndSymbol, startEndSymbol, dataType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotedValueLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startSymbol">The start symbol.</param>
        /// <param name="endSymbol">The end symbol.</param>
        /// <param name="dataType">Type of the data.</param>
        public QuotedValueLiteral(string name, string startSymbol, string endSymbol, TypeCode dataType)
            : base(name, dataType)
        {
            StartSymbol = startSymbol;
            EndSymbol = endSymbol;
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            return new string[] { StartSymbol };
        }
        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected override string ReadBody(ParsingContext context, ISourceStream source)
        {
            if (!source.MatchSymbol(StartSymbol)) return null; //this will result in null returned from TryMatch, no token
            var start = source.Location.Position + StartSymbol.Length;
            var end = source.Text.IndexOf(EndSymbol, start);
            if (end < 0) return null;
            var body = source.Text.Substring(start, end - start);
            source.PreviewPosition = end + EndSymbol.Length; //move beyond the end of EndSymbol
            return body;
        }
    }

}
