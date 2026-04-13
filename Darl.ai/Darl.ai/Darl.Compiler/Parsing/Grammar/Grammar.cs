// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Grammar.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using DarlCompiler.Ast;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DarlCompiler.Parsing
{


    /// Class Grammar.
    /// </summary>
    public class Grammar
    {

        #region properties
        /// Gets case sensitivity of the grammar. Read-only, true by default.
        /// Can be set to false only through a parameter to grammar constructor.
        /// </summary>
        public readonly bool CaseSensitive;

        //List of chars that unambiguously identify the start of new token. 
        //used in scanner error recovery, and in quick parse path in NumberLiterals, Identifiers 
        /// The delimiters
        /// </summary>
        [Obsolete("Use IsWhitespaceOrDelimiter() method instead.")]
        public string Delimiters = null;

        /// The whitespace chars
        /// </summary>
        [Obsolete("Override Grammar.SkipWhitespace method instead.")]
        // Not used anymore
        public string WhitespaceChars = " \t\r\n\v";

        /// The language flags
        /// </summary>
        public LanguageFlags LanguageFlags = LanguageFlags.Default;

        /// The term report groups
        /// </summary>
        public TermReportGroupList TermReportGroups = new TermReportGroupList();

        //Terminals not present in grammar expressions and not reachable from the Root
        // (Comment terminal is usually one of them)
        // Tokens produced by these terminals will be ignored by parser input. 
        /// The non grammar terminals
        /// </summary>
        public readonly TerminalSet NonGrammarTerminals = new TerminalSet();

        /// The main root entry for the grammar.
        /// </summary>
        public NonTerminal Root;

        /// Alternative roots for parsing code snippets.
        /// </summary>
        public NonTerminalSet SnippetRoots = new NonTerminalSet();

        /// The grammar comments
        /// </summary>
        public string GrammarComments; //shown in Grammar info tab

        /// The default culture
        /// </summary>
        public CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

        //Console-related properties, initialized in grammar constructor
        /// The console title
        /// </summary>
        public string ConsoleTitle;
        /// The console greeting
        /// </summary>
        public string ConsoleGreeting;
        /// The console prompt
        /// </summary>
        public string ConsolePrompt; //default prompt
        /// The console prompt more input
        /// </summary>
        public string ConsolePromptMoreInput; //prompt to show when more input is expected
        #endregion

        #region constructors

        /// Initializes a new instance of the <see cref="Grammar"/> class.
        /// </summary>
        public Grammar() : this(true) { } //case sensitive by default

        /// Initializes a new instance of the <see cref="Grammar"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public Grammar(bool caseSensitive)
        {
            _currentGrammar = this;
            this.CaseSensitive = caseSensitive;
            bool ignoreCase = !this.CaseSensitive;
            var stringComparer = StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, ignoreCase);
            KeyTerms = new KeyTermTable(stringComparer);
            //Initialize console attributes
            ConsoleTitle = Resources.MsgDefaultConsoleTitle;
            ConsoleGreeting = string.Format(Resources.MsgDefaultConsoleGreeting, this.GetType().Name);
            ConsolePrompt = ">";
            ConsolePromptMoreInput = ".";
        }
        #endregion

        #region Reserved words handling
        //Reserved words handling 
        /// Marks the reserved words.
        /// </summary>
        /// <param name="reservedWords">The reserved words.</param>
        public void MarkReservedWords(params string[] reservedWords)
        {
            foreach (var word in reservedWords)
            {
                var wdTerm = ToTerm(word);
                wdTerm.SetFlag(TermFlags.IsReservedWord);
            }
        }
        #endregion

        #region Register/Mark methods
        /// Registers the operators.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="opSymbols">The op symbols.</param>
        public void RegisterOperators(int precedence, params string[] opSymbols)
        {
            RegisterOperators(precedence, Associativity.Left, opSymbols);
        }

        /// Registers the operators.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="associativity">The associativity.</param>
        /// <param name="opSymbols">The op symbols.</param>
        public void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols)
        {
            foreach (string op in opSymbols)
            {
                KeyTerm opSymbol = ToTerm(op);
                opSymbol.SetFlag(TermFlags.IsOperator);
                opSymbol.Precedence = precedence;
                opSymbol.Associativity = associativity;
            }
        }

        /// Registers the operators.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="opTerms">The op terms.</param>
        public void RegisterOperators(int precedence, params BnfTerm[] opTerms)
        {
            RegisterOperators(precedence, Associativity.Left, opTerms);
        }
        /// Registers the operators.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="associativity">The associativity.</param>
        /// <param name="opTerms">The op terms.</param>
        public void RegisterOperators(int precedence, Associativity associativity, params BnfTerm[] opTerms)
        {
            foreach (var term in opTerms)
            {
                term.SetFlag(TermFlags.IsOperator);
                term.Precedence = precedence;
                term.Associativity = associativity;
            }
        }

        /// Registers the brace pair.
        /// </summary>
        /// <param name="openBrace">The open brace.</param>
        /// <param name="closeBrace">The close brace.</param>
        public void RegisterBracePair(string openBrace, string closeBrace)
        {
            KeyTerm openS = ToTerm(openBrace);
            KeyTerm closeS = ToTerm(closeBrace);
            openS.SetFlag(TermFlags.IsOpenBrace);
            openS.IsPairFor = closeS;
            closeS.SetFlag(TermFlags.IsCloseBrace);
            closeS.IsPairFor = openS;
        }

        /// Marks the punctuation.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        public void MarkPunctuation(params string[] symbols)
        {
            foreach (string symbol in symbols)
            {
                KeyTerm term = ToTerm(symbol);
                term.SetFlag(TermFlags.IsPunctuation | TermFlags.NoAstNode);
            }
        }

        /// Marks the punctuation.
        /// </summary>
        /// <param name="terms">The terms.</param>
        public void MarkPunctuation(params BnfTerm[] terms)
        {
            foreach (BnfTerm term in terms)
                term.SetFlag(TermFlags.IsPunctuation | TermFlags.NoAstNode);
        }


        /// Marks the transient.
        /// </summary>
        /// <param name="nonTerminals">The non terminals.</param>
        public void MarkTransient(params NonTerminal[] nonTerminals)
        {
            foreach (NonTerminal nt in nonTerminals)
                nt.Flags |= TermFlags.IsTransient | TermFlags.NoAstNode;
        }
        //MemberSelect are symbols invoking member list dropdowns in editor; for ex: . (dot), ::
        /// Marks the member select.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        public void MarkMemberSelect(params string[] symbols)
        {
            foreach (var symbol in symbols)
                ToTerm(symbol).SetFlag(TermFlags.IsMemberSelect);
        }
        //Sets IsNotReported flag on terminals. As a result the terminal wouldn't appear in expected terminal list
        // in syntax error messages
        /// Marks the not reported.
        /// </summary>
        /// <param name="terms">The terms.</param>
        public void MarkNotReported(params BnfTerm[] terms)
        {
            foreach (var term in terms)
                term.SetFlag(TermFlags.IsNotReported);
        }
        /// Marks the not reported.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        public void MarkNotReported(params string[] symbols)
        {
            foreach (var symbol in symbols)
                ToTerm(symbol).SetFlag(TermFlags.IsNotReported);
        }

        #endregion

        #region virtual methods: CreateTokenFilters, TryMatch
        /// Creates the token filters.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="filters">The filters.</param>
        public virtual void CreateTokenFilters(LanguageData language, TokenFilterList filters)
        {
        }

        //This method is called if Scanner fails to produce a token; it offers custom method a chance to produce the token    
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public virtual Token TryMatch(ParsingContext context, ISourceStream source)
        {
            return null;
        }

        //Gives a way to customize parse tree nodes captions in the tree view. 
        /// Gets the parse node caption.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>System.String.</returns>
        public virtual string GetParseNodeCaption(ParseTreeNode node)
        {
            if (node.IsError)
                return node.Term.Name + " (Syntax error)";
            if (node.Token != null)
                return node.Token.ToString();
            if (node.Term == null) //special case for initial node pushed into the stack at parser start
                return (node.State != null ? string.Empty : "(State " + node.State.Name + ")"); //  Resources.LabelInitialState;
            var ntTerm = node.Term as NonTerminal;
            if (ntTerm != null && !string.IsNullOrEmpty(ntTerm.NodeCaptionTemplate))
                return ntTerm.GetNodeCaption(node);
            return node.Term.Name;
        }

        /// Override this method to help scanner select a terminal to create token when there are more than one candidates
        /// for an input char. context.CurrentTerminals contains candidate terminals; leave a single terminal in this list
        /// as the one to use.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void OnScannerSelectTerminal(ParsingContext context) { }

        /// Skips whitespace characters in the input stream.
        /// </summary>
        /// <param name="source">Source stream.</param>
        /// <remarks>Override this method if your language has non-standard whitespace characters.</remarks>
        public virtual void SkipWhitespace(ISourceStream source)
        {
            while (!source.EOF())
            {
                switch (source.PreviewChar)
                {
                    case ' ':
                    case '\t':
                        break;
                    case '\r':
                    case '\n':
                    case '\v':
                        if (UsesNewLine) return; //do not treat as whitespace if language is line-based
                        break;
                    default:
                        return;
                }//switch
                source.PreviewPosition++;
            }
        }

        /// Returns true if a character is whitespace or delimiter. Used in quick-scanning versions of some terminals.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>True if a character is whitespace or delimiter; otherwise, false.</returns>
        /// <remarks>Does not have to be completely accurate, should recognize most common characters that are special chars by themselves
        /// and may never be part of other multi-character tokens.</remarks>
        public virtual bool IsWhitespaceOrDelimiter(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '\v': //whitespaces
                case '(':
                case ')':
                case ',':
                case ';':
                case '[':
                case ']':
                case '{':
                case '}':
                case (char)0: //EOF
                    return true;
                default:
                    return false;
            }
        }


        //The method is called after GrammarData is constructed 
        /// Called when [grammar data constructed].
        /// </summary>
        /// <param name="language">The language.</param>
        public virtual void OnGrammarDataConstructed(LanguageData language)
        {
        }

        /// Called when [language data constructed].
        /// </summary>
        /// <param name="language">The language.</param>
        public virtual void OnLanguageDataConstructed(LanguageData language)
        {
        }


        //Constructs the error message in situation when parser has no available action for current input.
        // override this method if you want to change this message
        /// Constructs the parser error message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="expectedTerms">The expected terms.</param>
        /// <returns>System.String.</returns>
        public virtual string ConstructParserErrorMessage(ParsingContext context, StringSet expectedTerms)
        {
            if (expectedTerms.Count > 0)
                return string.Format(Resources.ErrSyntaxErrorExpected, expectedTerms.ToString(", "));
            else
                return Resources.ErrParserUnexpectedInput;
        }

        // Override this method to perform custom error processing
        /// Reports the parse error.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void ReportParseError(ParsingContext context)
        {
            string error = null;
            if (context.CurrentParserInput.Term == this.SyntaxError)
                error = context.CurrentParserInput.Token.Value as string; //scanner error
            else if (context.CurrentParserInput.Term == this.Indent)
                error = Resources.ErrUnexpIndent;
            else if (context.CurrentParserInput.Term == this.Eof && context.OpenBraces.Count > 0)
            {
                if (context.OpenBraces.Count > 0)
                {
                    //report unclosed braces/parenthesis
                    var openBrace = context.OpenBraces.Peek();
                    error = string.Format(Resources.ErrNoClosingBrace, openBrace.Text);
                }
                else
                    error = Resources.ErrUnexpEof;
            }
            else
            {
                var expectedTerms = context.GetExpectedTermSet();
                error = ConstructParserErrorMessage(context, expectedTerms);
            }
            context.AddParserError(error);
        }
        #endregion

        #region MakePlusRule, MakeStarRule methods
        /// Makes the plus rule.
        /// </summary>
        /// <param name="listNonTerminal">The list non terminal.</param>
        /// <param name="listMember">The list member.</param>
        /// <returns>BnfExpression.</returns>
        public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm listMember)
        {
            return MakeListRule(listNonTerminal, null, listMember);
        }
        /// Makes the plus rule.
        /// </summary>
        /// <param name="listNonTerminal">The list non terminal.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="listMember">The list member.</param>
        /// <returns>BnfExpression.</returns>
        public BnfExpression MakePlusRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember)
        {
            return MakeListRule(listNonTerminal, delimiter, listMember);
        }
        /// Makes the star rule.
        /// </summary>
        /// <param name="listNonTerminal">The list non terminal.</param>
        /// <param name="listMember">The list member.</param>
        /// <returns>BnfExpression.</returns>
        public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm listMember)
        {
            return MakeListRule(listNonTerminal, null, listMember, TermListOptions.StarList);
        }
        /// Makes the star rule.
        /// </summary>
        /// <param name="listNonTerminal">The list non terminal.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="listMember">The list member.</param>
        /// <returns>BnfExpression.</returns>
        public BnfExpression MakeStarRule(NonTerminal listNonTerminal, BnfTerm delimiter, BnfTerm listMember)
        {
            return MakeListRule(listNonTerminal, delimiter, listMember, TermListOptions.StarList);
        }

        /// Makes the list rule.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="listMember">The list member.</param>
        /// <param name="options">The options.</param>
        /// <returns>BnfExpression.</returns>
        protected BnfExpression MakeListRule(NonTerminal list, BnfTerm delimiter, BnfTerm listMember, TermListOptions options = TermListOptions.PlusList)
        {
            //If it is a star-list (allows empty), then we first build plus-list
            var isPlusList = !options.IsSet(TermListOptions.AllowEmpty);
            var allowTrailingDelim = options.IsSet(TermListOptions.AllowTrailingDelimiter) && delimiter != null;
            //"plusList" is the list for which we will construct expression - it is either extra plus-list or original list. 
            // In the former case (extra plus-list) we will use it later to construct expression for list
            NonTerminal plusList = isPlusList ? list : new NonTerminal(listMember.Name + "+");
            plusList.SetFlag(TermFlags.IsList);
            plusList.Rule = plusList;  // rule => list
            if (delimiter != null)
                plusList.Rule += delimiter;  // rule => list + delim
            if (options.IsSet(TermListOptions.AddPreferShiftHint))
                plusList.Rule += PreferShiftHere(); // rule => list + delim + PreferShiftHere()
            plusList.Rule += listMember;          // rule => list + delim + PreferShiftHere() + elem
            plusList.Rule |= listMember;        // rule => list + delim + PreferShiftHere() + elem | elem
            if (isPlusList)
            {
                // if we build plus list - we're almost done; plusList == list
                // add trailing delimiter if necessary; for star list we'll add it to final expression
                if (allowTrailingDelim)
                    plusList.Rule |= list + delimiter; // rule => list + delim + PreferShiftHere() + elem | elem | list + delim
            }
            else
            {
                // Setup list.Rule using plus-list we just created
                list.Rule = Empty | plusList;
                if (allowTrailingDelim)
                    list.Rule |= plusList + delimiter | delimiter;
                plusList.SetFlag(TermFlags.NoAstNode);
                list.SetFlag(TermFlags.IsListContainer); //indicates that real list is one level lower
            }
            return list.Rule;
        }
        #endregion

        #region Hint utilities
        /// Prefers the shift here.
        /// </summary>
        /// <returns>GrammarHint.</returns>
        protected GrammarHint PreferShiftHere()
        {
            return new PreferredActionHint(PreferredActionType.Shift);
        }
        /// Reduces the here.
        /// </summary>
        /// <returns>GrammarHint.</returns>
        protected GrammarHint ReduceHere()
        {
            return new PreferredActionHint(PreferredActionType.Reduce);
        }
        /// Reduces if.
        /// </summary>
        /// <param name="thisSymbol">The this symbol.</param>
        /// <param name="comesBefore">The comes before.</param>
        /// <returns>TokenPreviewHint.</returns>
        protected TokenPreviewHint ReduceIf(string thisSymbol, params string[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Reduce, thisSymbol, comesBefore);
        }
        /// Reduces if.
        /// </summary>
        /// <param name="thisSymbol">The this symbol.</param>
        /// <param name="comesBefore">The comes before.</param>
        /// <returns>TokenPreviewHint.</returns>
        protected TokenPreviewHint ReduceIf(Terminal thisSymbol, params Terminal[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Reduce, thisSymbol, comesBefore);
        }
        /// Shifts if.
        /// </summary>
        /// <param name="thisSymbol">The this symbol.</param>
        /// <param name="comesBefore">The comes before.</param>
        /// <returns>TokenPreviewHint.</returns>
        protected TokenPreviewHint ShiftIf(string thisSymbol, params string[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Shift, thisSymbol, comesBefore);
        }
        /// Shifts if.
        /// </summary>
        /// <param name="thisSymbol">The this symbol.</param>
        /// <param name="comesBefore">The comes before.</param>
        /// <returns>TokenPreviewHint.</returns>
        protected TokenPreviewHint ShiftIf(Terminal thisSymbol, params Terminal[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Shift, thisSymbol, comesBefore);
        }
        /// Implies the precedence here.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <returns>GrammarHint.</returns>
        protected GrammarHint ImplyPrecedenceHere(int precedence)
        {
            return ImplyPrecedenceHere(precedence, Associativity.Left);
        }
        /// Implies the precedence here.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="associativity">The associativity.</param>
        /// <returns>GrammarHint.</returns>
        protected GrammarHint ImplyPrecedenceHere(int precedence, Associativity associativity)
        {
            return new ImpliedPrecedenceHint(precedence, associativity);
        }
        /// Customs the action here.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="previewMethod">The preview method.</param>
        /// <returns>CustomActionHint.</returns>
        protected CustomActionHint CustomActionHere(ExecuteActionMethod executeMethod, PreviewActionMethod? previewMethod = null)
        {
            return new CustomActionHint(executeMethod, previewMethod);
        }

        #endregion

        #region Term report group methods
        /// Creates a terminal reporting group, so all terminals in the group will be reported as a single "alias" in syntex error messages like
        /// "Syntax error, expected: [list of terms]"
        /// </summary>
        /// <param name="alias">An alias for all terminals in the group.</param>
        /// <param name="symbols">Symbols to be included into the group.</param>
        protected void AddTermsReportGroup(string alias, params string[] symbols)
        {
            TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Normal, SymbolsToTerms(symbols)));
        }
        /// Creates a terminal reporting group, so all terminals in the group will be reported as a single "alias" in syntex error messages like
        /// "Syntax error, expected: [list of terms]"
        /// </summary>
        /// <param name="alias">An alias for all terminals in the group.</param>
        /// <param name="terminals">Terminals to be included into the group.</param>
        protected void AddTermsReportGroup(string alias, params Terminal[] terminals)
        {
            TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Normal, terminals));
        }
        /// Adds symbols to a group with no-report type, so symbols will not be shown in expected lists in syntax error messages.
        /// </summary>
        /// <param name="symbols">Symbols to exclude.</param>
        protected void AddToNoReportGroup(params string[] symbols)
        {
            TermReportGroups.Add(new TermReportGroup(string.Empty, TermReportGroupType.DoNotReport, SymbolsToTerms(symbols)));
        }
        /// Adds symbols to a group with no-report type, so symbols will not be shown in expected lists in syntax error messages.
        /// </summary>
        /// <param name="terminals">The terminals.</param>
        protected void AddToNoReportGroup(params Terminal[] terminals)
        {
            TermReportGroups.Add(new TermReportGroup(string.Empty, TermReportGroupType.DoNotReport, terminals));
        }
        /// Adds a group and an alias for all operator symbols used in the grammar.
        /// </summary>
        /// <param name="alias">An alias for operator symbols.</param>
        protected void AddOperatorReportGroup(string alias)
        {
            TermReportGroups.Add(new TermReportGroup(alias, TermReportGroupType.Operator, null)); //operators will be filled later
        }

        /// Symbolses to terms.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        /// <returns>IEnumerable&lt;Terminal&gt;.</returns>
        private IEnumerable<Terminal> SymbolsToTerms(IEnumerable<string> symbols)
        {
            var termList = new TerminalList();
            foreach (var symbol in symbols)
                termList.Add(ToTerm(symbol));
            return termList;
        }
        #endregion

        #region Standard terminals: EOF, Empty, NewLine, Indent, Dedent
        // Empty object is used to identify optional element: 
        //    term.Rule = term1 | Empty;
        /// The empty
        /// </summary>
        public readonly Terminal Empty = new Terminal("EMPTY");
        /// The new line
        /// </summary>
        public readonly NewLineTerminal NewLine = new NewLineTerminal("LF");
        //set to true automatically by NewLine terminal; prevents treating new-line characters as whitespaces
        /// The uses new line
        /// </summary>
        public bool UsesNewLine;
        // The following terminals are used in indent-sensitive languages like Python;
        // they are not produced by scanner but are produced by CodeOutlineFilter after scanning
        /// The indent
        /// </summary>
        public readonly Terminal Indent = new Terminal("INDENT", TokenCategory.Outline, TermFlags.IsNonScanner);
        /// The dedent
        /// </summary>
        public readonly Terminal Dedent = new Terminal("DEDENT", TokenCategory.Outline, TermFlags.IsNonScanner);
        //End-of-Statement terminal - used in indentation-sensitive language to signal end-of-statement;
        // it is not always synced with CRLF chars, and CodeOutlineFilter carefully produces Eos tokens
        // (as well as Indent and Dedent) based on line/col information in incoming content tokens.
        /// The eos
        /// </summary>
        public readonly Terminal Eos = new Terminal("EOS", Resources.LabelEosLabel, TokenCategory.Outline, TermFlags.IsNonScanner);
        // Identifies end of file
        // Note: using Eof in grammar rules is optional. Parser automatically adds this symbol 
        // as a lookahead to Root non-terminal
        /// The EOF
        /// </summary>
        public readonly Terminal Eof = new Terminal("EOF", TokenCategory.Outline);

        //Artificial terminal to use for injected/replaced tokens that must be ignored by parser. 
        /// The skip
        /// </summary>
        public readonly Terminal Skip = new Terminal("(SKIP)", TokenCategory.Outline, TermFlags.IsNonGrammar);

        //Used as a "line-start" indicator
        /// The line start terminal
        /// </summary>
        public readonly Terminal LineStartTerminal = new Terminal("LINE_START", TokenCategory.Outline);

        //Used for error tokens
        /// The syntax error
        /// </summary>
        public readonly Terminal SyntaxError = new Terminal("SYNTAX_ERROR", TokenCategory.Error, TermFlags.IsNonScanner);

        /// Gets the new line plus.
        /// </summary>
        /// <value>The new line plus.</value>
        public NonTerminal NewLinePlus
        {
            get
            {
                if (_newLinePlus == null)
                {
                    _newLinePlus = new NonTerminal("LF+");
                    //We do no use MakePlusRule method; we specify the rule explicitly to add PrefereShiftHere call - this solves some unintended shift-reduce conflicts
                    // when using NewLinePlus 
                    _newLinePlus.Rule = NewLine | _newLinePlus + PreferShiftHere() + NewLine;
                    MarkPunctuation(_newLinePlus);
                    _newLinePlus.SetFlag(TermFlags.IsList);
                }
                return _newLinePlus;
            }
            /// The _new line plus
            /// </summary>
        }
        NonTerminal _newLinePlus;

        /// Gets the new line star.
        /// </summary>
        /// <value>The new line star.</value>
        public NonTerminal NewLineStar
        {
            get
            {
                if (_newLineStar == null)
                {
                    _newLineStar = new NonTerminal("LF*");
                    MarkPunctuation(_newLineStar);
                    _newLineStar.Rule = MakeStarRule(_newLineStar, NewLine);
                }
                return _newLineStar;
            }
            /// The _new line star
            /// </summary>
        }
        NonTerminal _newLineStar;

        #endregion

        #region KeyTerms (keywords + special symbols)
        /// The key terms
        /// </summary>
        public KeyTermTable KeyTerms;

        /// To the term.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>KeyTerm.</returns>
        public KeyTerm ToTerm(string text)
        {
            return ToTerm(text, text);
        }
        /// To the term.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="name">The name.</param>
        /// <returns>KeyTerm.</returns>
        public KeyTerm ToTerm(string text, string name)
        {
            KeyTerm term;
            if (KeyTerms.TryGetValue(text, out term))
            {
                //update name if it was specified now and not before
                if (string.IsNullOrEmpty(term.Name) && !string.IsNullOrEmpty(name))
                    term.Name = name;
                return term;
            }
            //create new term
            if (!CaseSensitive)
                text = text.ToLower(CultureInfo.InvariantCulture);
            string.Intern(text);
            term = new KeyTerm(text, name);
            KeyTerms[text] = term;
            return term;
        }

        #endregion

        #region CurrentGrammar static field
        //Static per-thread instance; Grammar constructor sets it to self (this). 
        // This field/property is used by operator overloads (which are static) to access Grammar's predefined terminals like Empty,
        //  and SymbolTerms dictionary to convert string literals to symbol terminals and add them to the SymbolTerms dictionary
        /// The _current grammar
        /// </summary>
        [ThreadStatic]
        private static Grammar _currentGrammar;
        /// Gets the current grammar.
        /// </summary>
        /// <value>The current grammar.</value>
        public static Grammar CurrentGrammar
        {
            get { return _currentGrammar; }
        }

        #endregion

        #region AST construction
        /// Builds the ast.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="parseTree">The parse tree.</param>
        public virtual void BuildAst(LanguageData language, ParseTree parseTree, Dictionary<string, ILocalStore> stores)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
                return;
            var astContext = new AstContext(language);
            var astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }
        #endregion
    }

}
