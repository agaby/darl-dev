// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Token.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// Enum TokenFlags
    /// </summary>
    public enum TokenFlags
    {
        /// The is incomplete
        /// </summary>
        IsIncomplete = 0x01,
    }

    /// Enum TokenCategory
    /// </summary>
    public enum TokenCategory
    {
        /// The content
        /// </summary>
        Content,
        /// The outline
        /// </summary>
        Outline, //newLine, indent, dedent
        /// The comment
        /// </summary>
        Comment,
        /// The directive
        /// </summary>
        Directive,
        /// The error
        /// </summary>
        Error,
    }

    /// Class TokenList.
    /// </summary>
    public class TokenList : List<Token> { }
    /// Class TokenStack.
    /// </summary>
    public class TokenStack : Stack<Token> { }

    //Tokens are produced by scanner and fed to parser, optionally passing through Token filters in between. 
    /// Class Token.
    /// </summary>
    public partial class Token
    {
        /// Gets the terminal.
        /// </summary>
        /// <value>The terminal.</value>
        public Terminal Terminal { get; private set; }
        /// The key term
        /// </summary>
        public KeyTerm KeyTerm;
        /// The location
        /// </summary>
        public readonly SourceLocation Location;
        /// The text
        /// </summary>
        public readonly string Text;

        /// The value
        /// </summary>
        public object Value;
        /// Gets the value string.
        /// </summary>
        /// <value>The value string.</value>
        public string ValueString
        {
            get { return (Value == null ? string.Empty : Value.ToString()); }
        }

        /// The details
        /// </summary>
        public object Details;
        /// The flags
        /// </summary>
        public TokenFlags Flags;
        /// The editor information
        /// </summary>
        public TokenEditorInfo EditorInfo;

        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="location">The location.</param>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        public Token(Terminal term, SourceLocation location, string text, object value)
        {
            SetTerminal(term);
            this.KeyTerm = term as KeyTerm;
            Location = location;
            Text = text;
            Value = value;
        }

        /// Sets the terminal.
        /// </summary>
        /// <param name="terminal">The terminal.</param>
        public void SetTerminal(Terminal terminal)
        {
            Terminal = terminal;
            this.EditorInfo = Terminal.EditorInfo;  //set to term's EditorInfo by default
        }

        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public bool IsSet(TokenFlags flag)
        {
            return (Flags & flag) != 0;
        }
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        public TokenCategory Category
        {
            get { return Terminal.Category; }
        }

        /// Determines whether this instance is error.
        /// </summary>
        /// <returns><c>true</c> if this instance is error; otherwise, <c>false</c>.</returns>
        public bool IsError()
        {
            return Category == TokenCategory.Error;
        }

        /// Gets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Length
        {
            get { return Text == null ? 0 : Text.Length; }
        }

        //matching opening/closing brace
        /// The other brace
        /// </summary>
        public Token OtherBrace;

        /// The scanner state
        /// </summary>
        public short ScannerState; //Scanner state after producing token 

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override string ToString()
        {
            return Terminal.TokenToString(this);
        }

    }

    //Some terminals may need to return a bunch of tokens in one call to TryMatch; MultiToken is a container for these tokens
    /// Class MultiToken.
    /// </summary>
    public class MultiToken : Token
    {
        /// The child tokens
        /// </summary>
        public TokenList ChildTokens;

        /// Initializes a new instance of the <see cref="MultiToken"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        public MultiToken(params Token[] tokens)
            : this(tokens[0].Terminal, tokens[0].Location, new TokenList())
        {
            ChildTokens.AddRange(tokens);
        }
        /// Initializes a new instance of the <see cref="MultiToken"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="location">The location.</param>
        /// <param name="childTokens">The child tokens.</param>
        public MultiToken(Terminal term, SourceLocation location, TokenList childTokens)
            : base(term, location, string.Empty, null)
        {
            ChildTokens = childTokens;
        }
    }

}
