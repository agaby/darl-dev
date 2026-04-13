/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParseTree.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    /* 
      A node for a parse tree (concrete syntax tree) - an initial syntax representation produced by parser.
      It contains all syntax elements of the input text, each element represented by a generic node ParseTreeNode. 
      The parse tree is converted into abstract syntax tree (AST) which contains custom nodes. The conversion might 
      happen on-the-fly: as parser creates the parse tree nodes it can create the AST nodes and puts them into AstNode field. 
      Alternatively it might happen as a separate step, after completing the parse tree. 
      AST node might optimally implement IAstNodeInit interface, so Darl parser can initialize the node providing it
      with all relevant information. 
      The ParseTreeNode also works as a stack element in the parser stack, so it has the State property to carry 
      the pushed parser state while it is in the stack. 
    */
    /// Class ParseTreeNode.
    /// </summary>
    public class ParseTreeNode
    {
        /// The ast node
        /// </summary>
        public object AstNode;
        /// The token
        /// </summary>
        public Token Token;
        /// The term
        /// </summary>
        public BnfTerm Term;
        /// The precedence
        /// </summary>
        public int Precedence;
        /// The associativity
        /// </summary>
        public Associativity Associativity;
        /// The span
        /// </summary>
        public SourceSpan Span;
        //Making ChildNodes property (not field) following request by Matt K, Bill H
        /// Gets the child nodes.
        /// </summary>
        /// <value>The child nodes.</value>
        public ParseTreeNodeList ChildNodes { get; private set; }
        /// The is error
        /// </summary>
        public bool IsError;
        /// The state
        /// </summary>
        internal ParserState State;      //used by parser to store current state when node is pushed into the parser stack
        /// The tag
        /// </summary>
        public object Tag; //for use by custom parsers, Darl does not use it
        /// The comments
        /// </summary>
        public TokenList Comments; //Comments preceding this node

        /// Prevents a default instance of the <see cref="ParseTreeNode"/> class from being created.
        /// </summary>
        private ParseTreeNode()
        {
            ChildNodes = new ParseTreeNodeList();
        }

        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class.
        /// </summary>
        /// <param name="token">The token.</param>
        public ParseTreeNode(Token token)
            : this()
        {
            Token = token;
            Term = token.Terminal;
            Precedence = Term.Precedence;
            Associativity = token.Terminal.Associativity;
            Span = new SourceSpan(token.Location, token.Length);
            IsError = token.IsError();
        }

        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        public ParseTreeNode(ParserState initialState)
            : this()
        {
            State = initialState;
        }

        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="span">The span.</param>
        public ParseTreeNode(NonTerminal term, SourceSpan span)
            : this()
        {
            Term = term;
            Span = span;
        }

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (Term == null)
                return "(S0)"; //initial state node
            else
                return Term.GetParseNodeCaption(this);
        }


        /// Finds the token and get text.
        /// </summary>
        /// <returns>System.String.</returns>
        public string FindTokenAndGetText()
        {
            var tkn = FindToken();
            return tkn == null ? null : tkn.Text;
        }
        /// Finds the token.
        /// </summary>
        /// <returns>Token.</returns>
        public Token FindToken()
        {
            return FindFirstChildTokenRec(this);
        }
        /// Finds the first child token record.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Token.</returns>
        private static Token FindFirstChildTokenRec(ParseTreeNode node)
        {
            if (node.Token != null) return node.Token;
            foreach (var child in node.ChildNodes)
            {
                var tkn = FindFirstChildTokenRec(child);
                if (tkn != null) return tkn;
            }
            return null;
        }

        /// Returns true if the node is punctuation or it is transient with empty child list.
        /// </summary>
        /// <returns>True if parser can safely ignore this node.</returns>
        public bool IsPunctuationOrEmptyTransient()
        {
            if (Term.Flags.IsSet(TermFlags.IsPunctuation))
                return true;
            if (Term.Flags.IsSet(TermFlags.IsTransient) && ChildNodes.Count == 0)
                return true;
            return false;
        }

        /// Determines whether this instance is operator.
        /// </summary>
        /// <returns><c>true</c> if this instance is operator; otherwise, <c>false</c>.</returns>
        public bool IsOperator()
        {
            return Term.Flags.IsSet(TermFlags.IsOperator);
        }

    }

    /// Class ParseTreeNodeList.
    /// </summary>
    public class ParseTreeNodeList : List<ParseTreeNode> { }

    /// Enum ParseTreeStatus
    /// </summary>
    public enum ParseTreeStatus
    {
        /// The parsing
        /// </summary>
        Parsing,
        /// The partial
        /// </summary>
        Partial,
        /// The parsed
        /// </summary>
        Parsed,
        /// The error
        /// </summary>
        Error,
    }

    /// Class ParseTree.
    /// </summary>
    public class ParseTree
    {
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public ParseTreeStatus Status { get; internal set; }
        /// The source text
        /// </summary>
        public readonly string SourceText;
        /// The file name
        /// </summary>
        public readonly string FileName;
        /// The tokens
        /// </summary>
        public readonly TokenList Tokens = new TokenList();
        /// The open braces
        /// </summary>
        public readonly TokenList OpenBraces = new TokenList();
        /// The root
        /// </summary>
        public ParseTreeNode Root;
        /// The parser messages
        /// </summary>
        public readonly LogMessageList ParserMessages = new LogMessageList();
        /// The parse time milliseconds
        /// </summary>
        public long ParseTimeMilliseconds;
        /// The tag
        /// </summary>
        public object Tag; //custom data object, use it anyway you want

        /// Initializes a new instance of the <see cref="ParseTree"/> class.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="fileName">Name of the file.</param>
        public ParseTree(string sourceText, string fileName)
        {
            SourceText = sourceText;
            FileName = fileName;
            Status = ParseTreeStatus.Parsing;
        }

        /// Determines whether this instance has errors.
        /// </summary>
        /// <returns><c>true</c> if this instance has errors; otherwise, <c>false</c>.</returns>
        public bool HasErrors()
        {
            if (ParserMessages.Count == 0) return false;
            foreach (var err in ParserMessages)
                if (err.Level == ErrorLevel.Error) return true;
            return false;
        }

    }

}
