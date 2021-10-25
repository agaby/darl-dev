// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CodeOutlineFilter.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Enum OutlineOptions
    /// </summary>
    [Flags]
    public enum OutlineOptions
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The produce indents
        /// </summary>
        ProduceIndents = 0x01,
        /// <summary>
        /// The check braces
        /// </summary>
        CheckBraces = 0x02,
        /// <summary>
        /// The check operator
        /// </summary>
        CheckOperator = 0x04, //to implement, auto line joining if line ends with operator 
    }

    /// <summary>
    /// Class CodeOutlineFilter.
    /// </summary>
    public class CodeOutlineFilter : TokenFilter
    {

        /// <summary>
        /// The options
        /// </summary>
        public readonly OutlineOptions Options;
        /// <summary>
        /// The continuation terminal
        /// </summary>
        public readonly KeyTerm ContinuationTerminal; //Terminal

        /// <summary>
        /// The _grammar data
        /// </summary>
        readonly GrammarData _grammarData;

        /// <summary>
        /// The _grammar
        /// </summary>
        readonly Grammar _grammar;
        /// <summary>
        /// The _context
        /// </summary>
        ParsingContext _context;

        /// <summary>
        /// The _produce indents
        /// </summary>
        readonly bool _produceIndents;

        /// <summary>
        /// The _check braces
        /// </summary>
        readonly bool _checkBraces, _checkOperator;

        /// <summary>
        /// The indents
        /// </summary>
        public Stack<int> Indents = new Stack<int>();
        /// <summary>
        /// The current token
        /// </summary>
        public Token CurrentToken;
        /// <summary>
        /// The previous token
        /// </summary>
        public Token PreviousToken;
        /// <summary>
        /// The previous token location
        /// </summary>
        public SourceLocation PreviousTokenLocation;
        /// <summary>
        /// The output tokens
        /// </summary>
        public TokenStack OutputTokens = new TokenStack();
        /// <summary>
        /// The _is continuation
        /// </summary>
        bool _isContinuation, _prevIsContinuation;
        /// <summary>
        /// The _is operator
        /// </summary>
        bool _isOperator, _prevIsOperator;
        /// <summary>
        /// The _double EOF
        /// </summary>
        bool _doubleEof;

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeOutlineFilter"/> class.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        /// <param name="options">The options.</param>
        /// <param name="continuationTerminal">The continuation terminal.</param>
        public CodeOutlineFilter(GrammarData grammarData, OutlineOptions options, KeyTerm continuationTerminal)
        {
            _grammarData = grammarData;
            _grammar = grammarData.Grammar;
            _grammar.LanguageFlags |= LanguageFlags.EmitLineStartToken;
            Options = options;
            ContinuationTerminal = continuationTerminal;
            if (ContinuationTerminal != null)
                if (!_grammar.NonGrammarTerminals.Contains(ContinuationTerminal))
                    _grammarData.Language.Errors.Add(GrammarErrorLevel.Warning, null, Resources.ErrOutlineFilterContSymbol, ContinuationTerminal.Name);
            //"CodeOutlineFilter: line continuation symbol '{0}' should be added to Grammar.NonGrammarTerminals list.",
            _produceIndents = OptionIsSet(OutlineOptions.ProduceIndents);
            _checkBraces = OptionIsSet(OutlineOptions.CheckBraces);
            _checkOperator = OptionIsSet(OutlineOptions.CheckOperator);
            Reset();
        }
        #endregion

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Indents.Clear();
            Indents.Push(0);
            OutputTokens.Clear();
            PreviousToken = null;
            CurrentToken = null;
            PreviousTokenLocation = new SourceLocation();
        }

        /// <summary>
        /// Options the is set.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool OptionIsSet(OutlineOptions option)
        {
            return (Options & option) != 0;
        }

        /// <summary>
        /// Begins the filtering.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="tokens">The tokens.</param>
        /// <returns>IEnumerable&lt;Token&gt;.</returns>
        public override IEnumerable<Token> BeginFiltering(ParsingContext context, IEnumerable<Token> tokens)
        {
            _context = context;
            foreach (Token token in tokens)
            {
                ProcessToken(token);
                while (OutputTokens.Count > 0)
                    yield return OutputTokens.Pop();
            }//foreach
        }

        /// <summary>
        /// Processes the token.
        /// </summary>
        /// <param name="token">The token.</param>
        public void ProcessToken(Token token)
        {
            SetCurrentToken(token);
            //Quick checks
            if (_isContinuation)
                return;
            var tokenTerm = token.Terminal;

            //check EOF
            if (tokenTerm == _grammar.Eof)
            {
                ProcessEofToken();
                return;
            }

            if (tokenTerm != _grammar.LineStartTerminal) return;
            //if we are here, we have LineStart token on new line; first remove it from stream, it should not go to parser
            OutputTokens.Pop();

            if (PreviousToken == null) return;


            // first check if there was continuation symbol before
            // or - if checkBraces flag is set - check if there were open braces
            if (_prevIsContinuation || _checkBraces && _context.OpenBraces.Count > 0)
                return; //no Eos token in this case
            if (_prevIsOperator && _checkOperator)
                return; //no Eos token in this case

            //We need to produce Eos token and indents (if _produceIndents is set). 
            // First check indents - they go first into OutputTokens stack, so they will be popped out last
            if (_produceIndents)
            {
                var currIndent = token.Location.Column;
                var prevIndent = Indents.Peek();
                if (currIndent > prevIndent)
                {
                    Indents.Push(currIndent);
                    PushOutlineToken(_grammar.Indent, token.Location);
                }
                else if (currIndent < prevIndent)
                {
                    PushDedents(currIndent);
                    //check that current indent exactly matches the previous indent 
                    if (Indents.Peek() != currIndent)
                    {
                        //fire error
                        OutputTokens.Push(new Token(_grammar.SyntaxError, token.Location, string.Empty, Resources.ErrInvDedent));
                        // "Invalid dedent level, no previous matching indent found."
                    }
                }
            }//if _produceIndents
            //Finally produce Eos token, but not in command line mode. In command line mode the Eos was already produced 
            // when we encountered Eof on previous line
            if (_context.Mode != ParseMode.CommandLine)
            {
                var eosLocation = ComputeEosLocation();
                PushOutlineToken(_grammar.Eos, eosLocation);
            }

        }

        /// <summary>
        /// Sets the current token.
        /// </summary>
        /// <param name="token">The token.</param>
        private void SetCurrentToken(Token token)
        {
            _doubleEof = CurrentToken != null && CurrentToken.Terminal == _grammar.Eof
                            && token.Terminal == _grammar.Eof;
            //Copy CurrentToken to PreviousToken
            if (CurrentToken != null && CurrentToken.Category == TokenCategory.Content)
            { //remember only content tokens
                PreviousToken = CurrentToken;
                _prevIsContinuation = _isContinuation;
                _prevIsOperator = _isOperator;
                if (PreviousToken != null)
                    PreviousTokenLocation = PreviousToken.Location;
            }
            CurrentToken = token;
            _isContinuation = (token.Terminal == ContinuationTerminal && ContinuationTerminal != null);
            _isOperator = token.Terminal.Flags.IsSet(TermFlags.IsOperator);
            if (!_isContinuation)
                OutputTokens.Push(token); //by default input token goes to output, except continuation symbol
        }

        //Processes Eof token. We should take into account the special case of processing command line input. 
        // In this case we should not automatically dedent all stacked indents if we get EOF.
        // Note that tokens will be popped from the OutputTokens stack and sent to parser in the reverse order compared to 
        // the order we pushed them into OutputTokens stack. We have Eof already in stack; we first push dedents, then Eos
        // They will come out to parser in the following order: Eos, Dedents, Eof.
        /// <summary>
        /// Processes the EOF token.
        /// </summary>
        private void ProcessEofToken()
        {
            //First decide whether we need to produce dedents and Eos symbol
            bool pushDedents = false;
            bool pushEos = true;
            switch (_context.Mode)
            {
                case ParseMode.File:
                    pushDedents = _produceIndents; //Do dedents if token filter tracks indents
                    break;
                case ParseMode.CommandLine:
                    //only if user entered empty line, we dedent all
                    pushDedents = _produceIndents && _doubleEof;
                    pushEos = !_prevIsContinuation && !_doubleEof; //if previous symbol is continuation symbol then don't push Eos
                    break;
                case ParseMode.VsLineScan:
                    pushDedents = false; //Do not dedent at all on every line end
                    break;
            }
            //unindent all buffered indents; 
            if (pushDedents) PushDedents(0);
            //now push Eos token - it will be popped first, then dedents, then EOF token
            if (pushEos)
            {
                var eosLocation = ComputeEosLocation();
                PushOutlineToken(_grammar.Eos, eosLocation);
            }
        }

        /// <summary>
        /// Pushes the dedents.
        /// </summary>
        /// <param name="untilPosition">The until position.</param>
        private void PushDedents(int untilPosition)
        {
            while (Indents.Peek() > untilPosition)
            {
                Indents.Pop();
                PushOutlineToken(_grammar.Dedent, CurrentToken.Location);
            }
        }

        /// <summary>
        /// Computes the eos location.
        /// </summary>
        /// <returns>SourceLocation.</returns>
        private SourceLocation ComputeEosLocation()
        {
            if (PreviousToken == null)
                return new SourceLocation();
            //Return position at the end of previous token
            var loc = PreviousToken.Location;
            var len = PreviousToken.Length;
            return new SourceLocation(loc.Position + len, loc.Line, loc.Column + len);
        }

        /// <summary>
        /// Pushes the outline token.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="location">The location.</param>
        private void PushOutlineToken(Terminal term, SourceLocation location)
        {
            OutputTokens.Push(new Token(term, location, string.Empty, null));
        }

    }
}
