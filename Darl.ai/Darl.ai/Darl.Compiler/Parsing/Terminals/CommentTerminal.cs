// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CommentTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System.Collections.Generic;


namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class CommentTerminal.
    /// </summary>
    public class CommentTerminal : Terminal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startSymbol">The start symbol.</param>
        /// <param name="endSymbols">The end symbols.</param>
        public CommentTerminal(string name, string startSymbol, params string[] endSymbols)
            : base(name, TokenCategory.Comment)
        {
            this.StartSymbol = startSymbol;
            this.EndSymbols = new StringList();
            EndSymbols.AddRange(endSymbols);
            Priority = TerminalPriority.High; //assign max priority
        }

        /// <summary>
        /// The start symbol
        /// </summary>
        public string StartSymbol;
        /// <summary>
        /// The end symbols
        /// </summary>
        public StringList EndSymbols;
        /// <summary>
        /// The _end symbols firsts
        /// </summary>
        private char[] _endSymbolsFirsts;
        /// <summary>
        /// The _is line comment
        /// </summary>
        private bool _isLineComment; //true if NewLine is one of EndSymbols; if yes, EOF is also considered a valid end symbol


        #region overrides
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            //_endSymbolsFirsts char array is used for fast search for end symbols using String's method IndexOfAny(...)
            _endSymbolsFirsts = new char[EndSymbols.Count];
            for (int i = 0; i < EndSymbols.Count; i++)
            {
                string sym = EndSymbols[i];
                _endSymbolsFirsts[i] = sym[0];
                _isLineComment |= sym.Contains("\n");
                if (!_isLineComment)
                    SetFlag(TermFlags.IsMultiline);
            }
            if (this.EditorInfo == null)
            {
                TokenType ttype = _isLineComment ? TokenType.LineComment : TokenType.Comment;
                this.EditorInfo = new TokenEditorInfo(ttype, TokenColor.Comment, TokenTriggers.None);
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
            Token result;
            if (context.VsLineScanState.Value != 0)
            {
                // we are continuing in line mode - restore internal env (none in this case)
                context.VsLineScanState.Value = 0;
            }
            else
            {
                //we are starting from scratch
                if (!BeginMatch(context, source)) return null;
            }
            result = CompleteMatch(context, source);
            if (result != null) return result;
            //if it is LineComment, it is ok to hit EOF without final line-break; just return all until end.
            if (_isLineComment)
                return source.CreateToken(this.OutputTerminal);
            if (context.Mode == ParseMode.VsLineScan)
                return CreateIncompleteToken(context, source);
            return context.CreateErrorToken(Resources.ErrUnclosedComment);
        }

        /// <summary>
        /// Creates the incomplete token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        private Token CreateIncompleteToken(ParsingContext context, ISourceStream source)
        {
            source.PreviewPosition = source.Text.Length;
            Token result = source.CreateToken(this.OutputTerminal);
            result.Flags |= TokenFlags.IsIncomplete;
            context.VsLineScanState.TerminalIndex = this.MultilineIndex;
            return result;
        }

        /// <summary>
        /// Begins the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool BeginMatch(ParsingContext context, ISourceStream source)
        {
            //Check starting symbol
            if (!source.MatchSymbol(StartSymbol)) return false;
            source.PreviewPosition += StartSymbol.Length;
            return true;
        }
        /// <summary>
        /// Completes the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        private Token CompleteMatch(ParsingContext context, ISourceStream source)
        {
            //Find end symbol
            while (!source.EOF())
            {
                int firstCharPos;
                if (EndSymbols.Count == 1)
                    firstCharPos = source.Text.IndexOf(EndSymbols[0], source.PreviewPosition);
                else
                    firstCharPos = source.Text.IndexOfAny(_endSymbolsFirsts, source.PreviewPosition);
                if (firstCharPos < 0)
                {
                    source.PreviewPosition = source.Text.Length;
                    return null; //indicating error
                }
                //We found a character that might start an end symbol; let's see if it is true.
                source.PreviewPosition = firstCharPos;
                foreach (string endSymbol in EndSymbols)
                {
                    if (source.MatchSymbol(endSymbol))
                    {
                        //We found end symbol; eat end symbol only if it is not line comment.
                        // For line comment, leave LF symbol there, it might be important to have a separate LF token
                        if (!_isLineComment)
                            source.PreviewPosition += endSymbol.Length;
                        return source.CreateToken(this.OutputTerminal);
                    }//if
                }//foreach endSymbol
                source.PreviewPosition++; //move to the next char and try again    
            }
            return null; //might happen if we found a start char of end symbol, but not the full endSymbol
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            return new string[] { StartSymbol };
        }
        #endregion
    }//CommentTerminal class


}
