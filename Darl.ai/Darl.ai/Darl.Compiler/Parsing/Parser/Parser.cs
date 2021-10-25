// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Parser.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DarlCompiler.Parsing
{

    //Parser class represents combination of scanner and LALR parser (CoreParser)
    /// <summary>
    /// Class Parser.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// <summary>
        /// The data
        /// </summary>
        public readonly ParserData Data;
        /// <summary>
        /// The _grammar
        /// </summary>
        private readonly Grammar _grammar;
        //public readonly CoreParser CoreParser;
        /// <summary>
        /// The scanner
        /// </summary>
        public readonly Scanner Scanner;
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public ParsingContext Context { get; internal set; }
        /// <summary>
        /// The root
        /// </summary>
        public readonly NonTerminal Root;
        // Either language root or initial state for parsing snippets - like Ruby's expressions in strings : "result= #{x+y}"  
        /// <summary>
        /// The initial state
        /// </summary>
        internal readonly ParserState InitialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        public Parser(Grammar grammar) : this(new LanguageData(grammar)) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public Parser(LanguageData language) : this(language, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="root">The root.</param>
        /// <exception cref="System.Exception"></exception>
        public Parser(LanguageData language, NonTerminal root)
        {
            Language = language;
            Data = Language.ParserData;
            _grammar = Language.Grammar;
            Context = new ParsingContext(this);
            Scanner = new Scanner(this);
            Root = root;
            if (Root == null)
            {
                Root = Language.Grammar.Root;
                InitialState = Language.ParserData.InitialState;
            }
            else
            {
                if (Root != Language.Grammar.Root && !Language.Grammar.SnippetRoots.Contains(Root))
                    throw new Exception(string.Format(Resources.ErrRootNotRegistered, root.Name));
                InitialState = Language.ParserData.InitialStates[Root];
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        internal void Reset()
        {
            Context.Reset();
            Scanner.Reset();
        }


        /// <summary>
        /// Parses the specified source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>ParseTree.</returns>
        public ParseTree Parse(string sourceText, Dictionary<string, ILocalStore>? stores = null)
        {
            SourceLocation loc = default(SourceLocation);
            Reset();
            /*      if (Context.Status == ParserStatus.AcceptedPartial) {
                    var oldLoc = Context.Source.Location;
                    loc = new SourceLocation(oldLoc.Position, oldLoc.Line + 1, 0);
                  } else {
                  }*/
            Context.Source = new SourceStream(sourceText, this.Language.Grammar.CaseSensitive, Context.TabWidth, loc);
            Context.CurrentParseTree = new ParseTree(sourceText, "Source");
            Context.Status = ParserStatus.Parsing;
            var sw = new Stopwatch();
            sw.Start();
            ParseAll();
            //Set Parse status
            var parseTree = Context.CurrentParseTree;
            bool hasErrors = parseTree.HasErrors();
            if (hasErrors)
                parseTree.Status = ParseTreeStatus.Error;
            else if (Context.Status == ParserStatus.AcceptedPartial)
                parseTree.Status = ParseTreeStatus.Partial;
            else
                parseTree.Status = ParseTreeStatus.Parsed;
            //Build AST if no errors and AST flag is set
            bool createAst = _grammar.LanguageFlags.IsSet(LanguageFlags.CreateAst);
            if (createAst && !hasErrors)
                Language.Grammar.BuildAst(Language, parseTree, stores);
            //Done; record the time
            sw.Stop();
            parseTree.ParseTimeMilliseconds = sw.ElapsedMilliseconds;
            if (parseTree.ParserMessages.Count > 0)
                parseTree.ParserMessages.Sort(LogMessageList.ByLocation);
            return parseTree;
        }

        /// <summary>
        /// Parses all.
        /// </summary>
        private void ParseAll()
        {
            //main loop
            Context.Status = ParserStatus.Parsing;
            while (Context.Status == ParserStatus.Parsing)
            {
                ExecuteNextAction();
            }
        }

        /// <summary>
        /// Scans the only.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>ParseTree.</returns>
        public ParseTree ScanOnly(string sourceText, string fileName)
        {
            Context.CurrentParseTree = new ParseTree(sourceText, fileName);
            Context.Source = new SourceStream(sourceText, Language.Grammar.CaseSensitive, Context.TabWidth);
            while (true)
            {
                var token = Scanner.GetToken();
                if (token == null || token.Terminal == Language.Grammar.Eof) break;
            }
            return Context.CurrentParseTree;
        }

        #region Parser Action execution
        /// <summary>
        /// Executes the next action.
        /// </summary>
        private void ExecuteNextAction()
        {
            //Read input only if DefaultReduceAction is null - in this case the state does not contain ExpectedSet,
            // so parser cannot assist scanner when it needs to select terminal and therefore can fail
            if (Context.CurrentParserInput == null && Context.CurrentParserState.DefaultAction == null)
                ReadInput();
            //Check scanner error
            if (Context.CurrentParserInput != null && Context.CurrentParserInput.IsError)
            {
                RecoverFromError();
                return;
            }
            //Try getting action
            var action = GetNextAction();
            if (action == null)
            {
                if (CheckPartialInputCompleted()) return;
                RecoverFromError();
                return;
            }
            //We have action. Write trace and execute it
            if (Context.TracingEnabled)
                Context.AddTrace(action.ToString());
            action.Execute(Context);
        }

        /// <summary>
        /// Gets the next action.
        /// </summary>
        /// <returns>ParserAction.</returns>
        internal ParserAction GetNextAction()
        {
            var currState = Context.CurrentParserState;
            var currInput = Context.CurrentParserInput;

            if (currState.DefaultAction != null)
                return currState.DefaultAction;
            ParserAction action;
            //First try as keyterm/key symbol; for example if token text = "while", then first try it as a keyword "while";
            // if this does not work, try as an identifier that happens to match a keyword but is in fact identifier
            Token inputToken = currInput.Token;
            if (inputToken != null && inputToken.KeyTerm != null)
            {
                var keyTerm = inputToken.KeyTerm;
                if (currState.Actions.TryGetValue(keyTerm, out action))
                {
                    #region comments
                    // Ok, we found match as a key term (keyword or special symbol)
                    // Backpatch the token's term. For example in most cases keywords would be recognized as Identifiers by Scanner.
                    // Identifier would also check with SymbolTerms table and set AsSymbol field to SymbolTerminal if there exist
                    // one for token content. So we first find action by Symbol if there is one; if we find action, then we 
                    // patch token's main terminal to AsSymbol value.  This is important for recognizing keywords (for colorizing), 
                    // and for operator precedence algorithm to work when grammar uses operators like "AND", "OR", etc. 
                    #endregion
                    inputToken.SetTerminal(keyTerm);
                    currInput.Term = keyTerm;
                    currInput.Precedence = keyTerm.Precedence;
                    currInput.Associativity = keyTerm.Associativity;
                    return action;
                }
            }
            //Try to get by main Terminal, only if it is not the same as symbol
            if (currState.Actions.TryGetValue(currInput.Term, out action))
                return action;
            //If input is EOF and NewLineBeforeEof flag is set, try using NewLine to find action
            if (currInput.Term == _grammar.Eof && _grammar.LanguageFlags.IsSet(LanguageFlags.NewLineBeforeEOF) &&
                currState.Actions.TryGetValue(_grammar.NewLine, out action))
            {
                //There's no action for EOF but there's action for NewLine. Let's add newLine token as input, just in case
                // action code wants to check input - it should see NewLine.
                var newLineToken = new Token(_grammar.NewLine, currInput.Token.Location, "\r\n", null);
                var newLineNode = new ParseTreeNode(newLineToken);
                Context.CurrentParserInput = newLineNode;
                return action;
            }//if
            return null;
        }

        #endregion

        #region reading input
        /// <summary>
        /// Reads the input.
        /// </summary>
        public void ReadInput()
        {
            Token token;
            Terminal term;
            //Get token from scanner while skipping all comment tokens (but accumulating them in comment block)
            do
            {
                token = Scanner.GetToken();
                term = token.Terminal;
                if (term.Category == TokenCategory.Comment)
                    Context.CurrentCommentTokens.Add(token);
            } while (term.Flags.IsSet(TermFlags.IsNonGrammar) && term != _grammar.Eof);
            //Check brace token      
            if (term.Flags.IsSet(TermFlags.IsBrace) && !CheckBraceToken(token))
                token = new Token(_grammar.SyntaxError, token.Location, token.Text,
                   string.Format(Resources.ErrUnmatchedCloseBrace, token.Text));
            //Create parser input node
            Context.CurrentParserInput = new ParseTreeNode(token);
            //attach comments if any accumulated to content token
            if (token.Terminal.Category == TokenCategory.Content)
            {
                Context.CurrentParserInput.Comments = Context.CurrentCommentTokens;
                Context.CurrentCommentTokens = new TokenList();
            }
            //Fire event on Terminal
            token.Terminal.OnParserInputPreview(Context);
        }
        #endregion

        #region Error Recovery
        /// <summary>
        /// Recovers from error.
        /// </summary>
        public void RecoverFromError()
        {
            this.Data.ErrorAction.Execute(Context);
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Checks the partial input completed.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckPartialInputCompleted()
        {
            bool partialCompleted = (Context.Mode == ParseMode.CommandLine && Context.CurrentParserInput.Term == _grammar.Eof);
            if (!partialCompleted) return false;
            Context.Status = ParserStatus.AcceptedPartial;
            // clean up EOF in input so we can continue parsing next line
            Context.CurrentParserInput = null;
            return true;
        }

        // We assume here that the token is a brace (opening or closing)
        /// <summary>
        /// Checks the brace token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckBraceToken(Token token)
        {
            if (token.Terminal.Flags.IsSet(TermFlags.IsOpenBrace))
            {
                Context.OpenBraces.Push(token);
                return true;
            }
            //it is closing brace; check if we have opening brace in the stack
            var braces = Context.OpenBraces;
            var match = (braces.Count > 0 && braces.Peek().Terminal.IsPairFor == token.Terminal);
            if (!match) return false;
            //Link both tokens, pop the stack and return true
            var openingBrace = braces.Pop();
            openingBrace.OtherBrace = token;
            token.OtherBrace = openingBrace;
            return true;
        }
        #endregion




    }
}
