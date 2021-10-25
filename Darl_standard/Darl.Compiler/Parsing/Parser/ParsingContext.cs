// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParsingContext.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using Darl_standard;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Enum ParseOptions
    /// </summary>
    [Flags]
    public enum ParseOptions
    {
        /// <summary>
        /// The reserved
        /// </summary>
        Reserved = 0x01,
        /// <summary>
        /// The analyze code
        /// </summary>
        AnalyzeCode = 0x10,   //run code analysis; effective only in Module mode
    }

    /// <summary>
    /// Enum ParseMode
    /// </summary>
    public enum ParseMode
    {
        /// <summary>
        /// The file
        /// </summary>
        File,       //default, continuous input file
        /// <summary>
        /// The vs line scan
        /// </summary>
        VsLineScan,   // line-by-line scanning in VS integration for syntax highlighting
        /// <summary>
        /// The command line
        /// </summary>
        CommandLine, //line-by-line from console
    }

    /// <summary>
    /// Enum ParserStatus
    /// </summary>
    public enum ParserStatus
    {
        /// <summary>
        /// The initialize
        /// </summary>
        Init, //initial state
        /// <summary>
        /// The parsing
        /// </summary>
        Parsing,
        /// <summary>
        /// The previewing
        /// </summary>
        Previewing, //previewing tokens
        /// <summary>
        /// The recovering
        /// </summary>
        Recovering, //recovering from error
        /// <summary>
        /// The accepted
        /// </summary>
        Accepted,
        /// <summary>
        /// The accepted partial
        /// </summary>
        AcceptedPartial,
        /// <summary>
        /// The error
        /// </summary>
        Error,
    }

    // The purpose of this class is to provide a container for information shared 
    // between parser, scanner and token filters.
    /// <summary>
    /// Class ParsingContext.
    /// </summary>
    public partial class ParsingContext
    {
        /// <summary>
        /// The parser
        /// </summary>
        public readonly Parser Parser;
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;

        //Parser settings
        /// <summary>
        /// The options
        /// </summary>
        public ParseOptions Options;
        /// <summary>
        /// The tracing enabled
        /// </summary>
        public bool TracingEnabled;
        /// <summary>
        /// The mode
        /// </summary>
        public ParseMode Mode = ParseMode.File;
        /// <summary>
        /// The maximum errors
        /// </summary>
        public int MaxErrors = 20; //maximum error count to report
        /// <summary>
        /// The culture
        /// </summary>
        public CultureInfo Culture; //defaults to Grammar.DefaultCulture, might be changed by app code

        #region properties and fields
        //Parser fields
        /// <summary>
        /// Gets the current parse tree.
        /// </summary>
        /// <value>The current parse tree.</value>
        public ParseTree CurrentParseTree { get; internal set; }
        /// <summary>
        /// The open braces
        /// </summary>
        public readonly TokenStack OpenBraces = new TokenStack();
        /// <summary>
        /// The parser trace
        /// </summary>
        public ParserTrace ParserTrace = new ParserTrace();
        /// <summary>
        /// The parser stack
        /// </summary>
        public readonly ParserStack ParserStack = new ParserStack();

        /// <summary>
        /// Gets the state of the current parser.
        /// </summary>
        /// <value>The state of the current parser.</value>
        public ParserState CurrentParserState { get; internal set; }
        /// <summary>
        /// Gets the current parser input.
        /// </summary>
        /// <value>The current parser input.</value>
        public ParseTreeNode CurrentParserInput { get; internal set; }
        /// <summary>
        /// The current token
        /// </summary>
        public Token CurrentToken; //The token just scanned by Scanner
        /// <summary>
        /// The current comment tokens
        /// </summary>
        public TokenList CurrentCommentTokens = new TokenList(); //accumulated comment tokens
        /// <summary>
        /// The previous token
        /// </summary>
        public Token PreviousToken;
        /// <summary>
        /// The previous line start
        /// </summary>
        public SourceLocation PreviousLineStart; //Location of last line start

        //list for terminals - for current parser state and current input char
        /// <summary>
        /// The current terminals
        /// </summary>
        public TerminalList CurrentTerminals = new TerminalList();

        /// <summary>
        /// The source
        /// </summary>
        public ISourceStream Source;

        //Internal fields
        /// <summary>
        /// The token filters
        /// </summary>
        internal TokenFilterList TokenFilters = new TokenFilterList();
        /// <summary>
        /// The buffered tokens
        /// </summary>
        internal TokenStack BufferedTokens = new TokenStack();
        /// <summary>
        /// The filtered tokens
        /// </summary>
        internal IEnumerator<Token> FilteredTokens; //stream of tokens after filter
        /// <summary>
        /// The preview tokens
        /// </summary>
        internal TokenStack PreviewTokens = new TokenStack();
        /// <summary>
        /// The shared parsing event arguments
        /// </summary>
        internal ParsingEventArgs SharedParsingEventArgs;
        /// <summary>
        /// The shared validate token event arguments
        /// </summary>
        internal ValidateTokenEventArgs SharedValidateTokenEventArgs;

        /// <summary>
        /// The vs line scan state
        /// </summary>
        public VsScannerStateMap VsLineScanState; //State variable used in line scanning mode for VS integration

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public ParserStatus Status { get; internal set; }
        /// <summary>
        /// The has errors
        /// </summary>
        public bool HasErrors; //error flag, once set remains set

        //values dictionary to use by custom language implementations to save some temporary values during parsing
        /// <summary>
        /// The values
        /// </summary>
        public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

        /// <summary>
        /// The tab width
        /// </summary>
        public int TabWidth = 8;

        #endregion


        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingContext"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public ParsingContext(Parser parser)
        {
            this.Parser = parser;
            Language = Parser.Language;
            Culture = Language.Grammar.DefaultCulture;
            //This might be a problem for multi-threading - if we have several contexts on parallel threads with different culture.
            //Resources.Culture is static property (this is not Darl's fault, this is auto-generated file).
            Resources.Culture = Culture;
            SharedParsingEventArgs = new ParsingEventArgs(this);
            SharedValidateTokenEventArgs = new ValidateTokenEventArgs(this);
        }
        #endregion


        #region Events: TokenCreated
        /// <summary>
        /// Occurs when [token created].
        /// </summary>
        public event EventHandler<ParsingEventArgs> TokenCreated;

        /// <summary>
        /// Called when [token created].
        /// </summary>
        internal void OnTokenCreated()
        {
            if (TokenCreated != null)
                TokenCreated(this, SharedParsingEventArgs);
        }
        #endregion

        #region Error handling and tracing

        /// <summary>
        /// Creates the error token.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Token.</returns>
        public Token CreateErrorToken(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            return Source.CreateToken(Language.Grammar.SyntaxError, message);
        }

        /// <summary>
        /// Adds the parser error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddParserError(string message, params object[] args)
        {
            var location = CurrentParserInput == null ? Source.Location : CurrentParserInput.Span.Location;
            HasErrors = true;
            AddParserMessage(ErrorLevel.Error, location, message, args);
        }
        /// <summary>
        /// Adds the parser message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddParserMessage(ErrorLevel level, SourceLocation location, string message, params object[] args)
        {
            if (CurrentParseTree == null) return;
            if (CurrentParseTree.ParserMessages.Count >= MaxErrors) return;
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            CurrentParseTree.ParserMessages.Add(new LogMessage(level, location, message, CurrentParserState));
            if (TracingEnabled)
                AddTrace(true, message);
        }

        /// <summary>
        /// Adds the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddTrace(string message, params object[] args)
        {
            AddTrace(false, message, args);
        }
        /// <summary>
        /// Adds the trace.
        /// </summary>
        /// <param name="asError">if set to <c>true</c> [as error].</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void AddTrace(bool asError, string message, params object[] args)
        {
            if (!TracingEnabled)
                return;
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            ParserTrace.Add(new ParserTraceEntry(CurrentParserState, ParserStack.Top, CurrentParserInput, message, asError));
        }

        #region comments
        // Computes set of expected terms in a parser state. While there may be extended list of symbols expected at some point,
        // we want to reorganize and reduce it. For example, if the current state expects all arithmetic operators as an input,
        // it would be better to not list all operators (+, -, *, /, etc) but simply put "operator" covering them all. 
        // To achieve this grammar writer can group operators (or any other terminals) into named groups using Grammar's methods
        // AddTermReportGroup, AddNoReportGroup etc. Then instead of reporting each operator separately, Darl would include 
        // a single "group name" to represent them all.
        // The "expected report set" is not computed during parser construction (it would take considerable time), 
        // but does it on demand during parsing, when error is detected and the expected set is actually needed for error message. 
        // Multi-threading concerns. When used in multi-threaded environment (web server), the LanguageData would be shared in 
        // application-wide cache to avoid rebuilding the parser data on every request. The LanguageData is immutable, except 
        // this one case - the expected sets are constructed late by CoreParser on the when-needed basis. 
        // We don't do any locking here, just compute the set and on return from this function the state field is assigned. 
        // We assume that this field assignment is an atomic, concurrency-safe operation. The worst thing that might happen
        // is "double-effort" when two threads start computing the same set around the same time, and the last one to finish would 
        // leave its result in the state field. 
        #endregion
        /// <summary>
        /// Computes the state of the grouped expected set for.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        /// <param name="state">The state.</param>
        /// <returns>StringSet.</returns>
        internal static StringSet ComputeGroupedExpectedSetForState(Grammar grammar, ParserState state)
        {
            var terms = new TerminalSet();
            terms.UnionWith(state.ExpectedTerminals);
            var result = new StringSet();
            //Eliminate no-report terminals
            foreach (var group in grammar.TermReportGroups)
            {
                if (group.GroupType == TermReportGroupType.DoNotReport)
                    terms.ExceptWith(group.Terminals);
            }
            //Add normal and operator groups
            foreach (var group in grammar.TermReportGroups)
            {
                if ((group.GroupType == TermReportGroupType.Normal || group.GroupType == TermReportGroupType.Operator) &&
                      terms.Overlaps(group.Terminals))
                {
                    result.Add(group.Alias);
                    terms.ExceptWith(group.Terminals);
                }
            }
            //Add remaining terminals "as is"
            foreach (var terminal in terms)
                result.Add(terminal.ErrorAlias);
            return result;
        }

        #endregion

        /// <summary>
        /// Resets this instance.
        /// </summary>
        internal void Reset()
        {
            CurrentParserState = Parser.InitialState;
            CurrentParserInput = null;
            CurrentCommentTokens = new TokenList();
            ParserStack.Clear();
            HasErrors = false;
            ParserStack.Push(new ParseTreeNode(CurrentParserState));
            CurrentParseTree = null;
            OpenBraces.Clear();
            ParserTrace.Clear();
            CurrentTerminals.Clear();
            CurrentToken = null;
            PreviousToken = null;
            PreviousLineStart = new SourceLocation(0, -1, 0);
            BufferedTokens.Clear();
            PreviewTokens.Clear();
            Values.Clear();
            foreach (var filter in TokenFilters)
                filter.Reset();
        }

        /// <summary>
        /// Sets the source location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetSourceLocation(SourceLocation location)
        {
            foreach (var filter in TokenFilters)
                filter.OnSetSourceLocation(location);
            Source.Location = location;
        }

        /// <summary>
        /// Computes the stack range span.
        /// </summary>
        /// <param name="nodeCount">The node count.</param>
        /// <returns>SourceSpan.</returns>
        public SourceSpan ComputeStackRangeSpan(int nodeCount)
        {
            if (nodeCount == 0)
                return new SourceSpan(CurrentParserInput.Span.Location, 0);
            var first = ParserStack[ParserStack.Count - nodeCount];
            var last = ParserStack.Top;
            return new SourceSpan(first.Span.Location, last.Span.EndPosition - first.Span.Location.Position);
        }


        #region Expected term set computations
        /// <summary>
        /// Gets the expected term set.
        /// </summary>
        /// <returns>StringSet.</returns>
        public StringSet GetExpectedTermSet()
        {
            if (CurrentParserState == null)
                return new StringSet();
            //See note about multi-threading issues in ComputeReportedExpectedSet comments.
            if (CurrentParserState.ReportedExpectedSet == null)
                CurrentParserState.ReportedExpectedSet = Construction.ParserDataBuilder.ComputeGroupedExpectedSetForState(Language.Grammar, CurrentParserState);
            //Filter out closing braces which are not expected based on previous input.
            // While the closing parenthesis ")" might be expected term in a state in general, 
            // if there was no opening parenthesis in preceding input then we would not
            //  expect a closing one. 
            var expectedSet = FilterBracesInExpectedSet(CurrentParserState.ReportedExpectedSet);
            return expectedSet;
        }

        /// <summary>
        /// Filters the braces in expected set.
        /// </summary>
        /// <param name="stateExpectedSet">The state expected set.</param>
        /// <returns>StringSet.</returns>
        private StringSet FilterBracesInExpectedSet(StringSet stateExpectedSet)
        {
            var result = new StringSet();
            result.UnionWith(stateExpectedSet);
            //Find what brace we expect
            var nextClosingBrace = string.Empty;
            if (OpenBraces.Count > 0)
            {
                var lastOpenBraceTerm = OpenBraces.Peek().KeyTerm;
                var nextClosingBraceTerm = lastOpenBraceTerm.IsPairFor as KeyTerm;
                if (nextClosingBraceTerm != null)
                    nextClosingBrace = nextClosingBraceTerm.Text;
            }
            //Now check all closing braces in result set, and leave only nextClosingBrace
            foreach (var term in Language.Grammar.KeyTerms.Values)
            {
                if (term.Flags.IsSet(TermFlags.IsCloseBrace))
                {
                    var brace = term.Text;
                    if (result.Contains(brace) && brace != nextClosingBrace)
                        result.Remove(brace);
                }
            }//foreach term
            return result;
        }

        #endregion


    }

    // A struct used for packing/unpacking ScannerState int value; used for VS integration.
    // When Terminal produces incomplete token, it sets 
    // this state to non-zero value; this value identifies this terminal as the one who will continue scanning when
    // it resumes, and the terminal's internal state when there may be several types of multi-line tokens for one terminal.
    // For ex., there maybe several types of string literal like in Python. 
    /// <summary>
    /// Struct VsScannerStateMap
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct VsScannerStateMap
    {
        /// <summary>
        /// The value
        /// </summary>
        [FieldOffset(0)]
        public int Value;
        /// <summary>
        /// The terminal index
        /// </summary>
        [FieldOffset(0)]
        public byte TerminalIndex;   //1-based index of active multiline term in MultilineTerminals
        /// <summary>
        /// The token sub type
        /// </summary>
        [FieldOffset(1)]
        public byte TokenSubType;         //terminal subtype (used in StringLiteral to identify string kind)
        /// <summary>
        /// The terminal flags
        /// </summary>
        [FieldOffset(2)]
        public short TerminalFlags;  //Terminal flags
    }//struct


}
