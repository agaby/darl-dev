// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LineContinuationTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class LineContinuationTerminal.
    /// </summary>
    public class LineContinuationTerminal : Terminal
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LineContinuationTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startSymbols">The start symbols.</param>
        public LineContinuationTerminal(string name, params string[] startSymbols)
            : base(name, TokenCategory.Outline)
        {
            var symbols = startSymbols.Where(s => !IsNullOrWhiteSpace(s)).ToArray();
            StartSymbols = new StringList(symbols);
            if (StartSymbols.Count == 0)
                StartSymbols.AddRange(_defaultStartSymbols);
            Priority = TerminalPriority.High;
        }

        /// <summary>
        /// The start symbols
        /// </summary>
        public StringList StartSymbols;
        /// <summary>
        /// The _start symbols firsts
        /// </summary>
        private string _startSymbolsFirsts = String.Concat(_defaultStartSymbols);
        /// <summary>
        /// The _default start symbols
        /// </summary>
        static readonly string[] _defaultStartSymbols = new[] { "\\", "_" };
        /// <summary>
        /// The line terminators
        /// </summary>
        public string LineTerminators = "\n\r\v";

        #region overrides

        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);

            // initialize string of start characters for fast lookup
            _startSymbolsFirsts = new String(StartSymbols.Select(s => s.First()).ToArray());

            if (this.EditorInfo == null)
            {
                this.EditorInfo = new TokenEditorInfo(TokenType.Delimiter, TokenColor.Comment, TokenTriggers.None);
            }
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            // Quick check
            var lookAhead = source.PreviewChar;
            var startIndex = _startSymbolsFirsts.IndexOf(lookAhead);
            if (startIndex < 0)
                return null;

            // Match start symbols
            if (!BeginMatch(source, startIndex, lookAhead))
                return null;

            // Match NewLine
            var result = CompleteMatch(source);
            if (result != null)
                return result;

            // Report an error
            return context.CreateErrorToken(Resources.ErrNewLineExpected);
        }

        /// <summary>
        /// Begins the match.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startFrom">The start from.</param>
        /// <param name="lookAhead">The look ahead.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool BeginMatch(ISourceStream source, int startFrom, char lookAhead)
        {
            foreach (var startSymbol in StartSymbols.Skip(startFrom))
            {
                if (startSymbol[0] != lookAhead)
                    continue;
                if (source.MatchSymbol(startSymbol))
                {
                    source.PreviewPosition += startSymbol.Length;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Completes the match.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        private Token CompleteMatch(ISourceStream source)
        {
            if (source.EOF())
                return null;

            do
            {
                // Match NewLine
                var lookAhead = source.PreviewChar;
                if (LineTerminators.IndexOf(lookAhead) >= 0)
                {
                    source.PreviewPosition++;
                    // Treat \r\n as single NewLine
                    if (!source.EOF() && lookAhead == '\r' && source.PreviewChar == '\n')
                        source.PreviewPosition++;
                    break;
                }

                // Eat up whitespace
                if (this.Grammar.IsWhitespaceOrDelimiter(lookAhead))
                {
                    source.PreviewPosition++;
                    continue;
                }

                // Fail on anything else
                return null;
            }
            while (!source.EOF());

            // Create output token
            return source.CreateToken(this.OutputTerminal);
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            return StartSymbols;
        }

        #endregion

        /// <summary>
        /// Determines whether [is null or white space] [the specified s].
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns><c>true</c> if [is null or white space] [the specified s]; otherwise, <c>false</c>.</returns>
        private static bool IsNullOrWhiteSpace(string s)
        {
#if VS2008
      if (String.IsNullOrEmpty(s))
        return true;
      return s.Trim().Length == 0;
#else
            return String.IsNullOrWhiteSpace(s);
#endif
        }

    } // LineContinuationTerminal class
}
