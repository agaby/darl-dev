// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="DsvLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Text;
using Darl.ai;

namespace DarlCompiler.Parsing
{

    //A terminal for DSV-formatted files (Delimiter-Separated Values), a generalization of CSV (comma-separated values) format.  
    // See http://en.wikipedia.org/wiki/Delimiter-separated_values
    // For CSV format, there's a recommendation RFC4180 (http://tools.ietf.org/html/rfc4180)
    // It might seem that this terminal is not that useful and it is easy enough to create a custom CSV reader for a particular data format
    // format. However, if you consider all escaping and double-quote enclosing rules, then a custom reader solution would not seem so trivial.
    // So DsvLiteral can simplify this task.  
    /// <summary>
    /// Class DsvLiteral.
    /// </summary>
    public class DsvLiteral : DataLiteralBase
    {
        /// <summary>
        /// The terminator
        /// </summary>
        public string Terminator = ",";
        /// <summary>
        /// The consume terminator
        /// </summary>
        public bool ConsumeTerminator = true; //if true, the source pointer moves after the separator 
        /// <summary>
        /// The _terminators
        /// </summary>
        private char[] _terminators;

        //For last value on the line specify terminator = null; the DsvLiteral will then look for NewLine as terminator
        /// <summary>
        /// Initializes a new instance of the <see cref="DsvLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="terminator">The terminator.</param>
        public DsvLiteral(string name, TypeCode dataType, string terminator)
            : this(name, dataType)
        {
            Terminator = terminator;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLiteralBase" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">Type of the data.</param>
        public DsvLiteral(string name, TypeCode dataType) : base(name, dataType) { }

        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            if (Terminator == null)
                _terminators = new char[] { '\n', '\r' };
            else
                _terminators = new char[] { Terminator[0] };
        }

        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected override string ReadBody(ParsingContext context, ISourceStream source)
        {
            string body;
            if (source.PreviewChar == '"')
                body = ReadQuotedBody(context, source);
            else
                body = ReadNotQuotedBody(context, source);
            if (ConsumeTerminator && Terminator != null)
                MoveSourcePositionAfterTerminator(source);
            return body;
        }

        /// <summary>
        /// Reads the quoted body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.Exception"></exception>
        private string ReadQuotedBody(ParsingContext context, ISourceStream source)
        {
            const char dQuoute = '"';
            StringBuilder sb = null;
            var from = source.Location.Position + 1; //skip initial double quote
            while (true)
            {
                var until = source.Text.IndexOf(dQuoute, from);
                if (until < 0)
                    throw new Exception(Resources.ErrDsvNoClosingQuote); // "Could not find a closing quote for quoted value."
                source.PreviewPosition = until; //now points at double-quote
                var piece = source.Text.Substring(from, until - from);
                source.PreviewPosition++; //move after double quote
                if (source.PreviewChar != dQuoute && sb == null)
                    return piece; //quick path - if sb (string builder) was not created yet, we are looking at the very first segment;
                // and if we found a standalone dquote, then we are done - the "piece" is the result. 
                if (sb == null)
                    sb = new StringBuilder(100);
                sb.Append(piece);
                if (source.PreviewChar != dQuoute)
                    return sb.ToString();
                //we have doubled double-quote; add a single double-quoute char to the result and move over both symbols
                sb.Append(dQuoute);
                from = source.PreviewPosition + 1;
            }
        }

        /// <summary>
        /// Reads the not quoted body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        private string ReadNotQuotedBody(ParsingContext context, ISourceStream source)
        {
            var startPos = source.Location.Position;
            var sepPos = source.Text.IndexOfAny(_terminators, startPos);
            if (sepPos < 0)
                sepPos = source.Text.Length;
            source.PreviewPosition = sepPos;
            var valueText = source.Text.Substring(startPos, sepPos - startPos);
            return valueText;
        }

        /// <summary>
        /// Moves the source position after terminator.
        /// </summary>
        /// <param name="source">The source.</param>
        private void MoveSourcePositionAfterTerminator(ISourceStream source)
        {
            while (!source.EOF())
            {
                while (source.PreviewChar != Terminator[0])
                    source.PreviewPosition++;
                if (source.MatchSymbol(Terminator))
                {
                    source.PreviewPosition += Terminator.Length;
                    return;
                }//if
            }
        }

    }


}
