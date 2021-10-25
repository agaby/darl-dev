// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="_Terminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class TerminalPriority.
    /// </summary>
    public static class TerminalPriority
    {
        /// <summary>
        /// The low
        /// </summary>
        public static int Low = -1000;
        /// <summary>
        /// The normal
        /// </summary>
        public static int Normal = 0;
        /// <summary>
        /// The high
        /// </summary>
        public static int High = 1000;
        /// <summary>
        /// The reserved words
        /// </summary>
        public static int ReservedWords = 900;

    }

    /// <summary>
    /// Class Terminal.
    /// </summary>
    public partial class Terminal : BnfTerm
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Terminal(string name) : this(name, TokenCategory.Content, TermFlags.None) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        public Terminal(string name, TokenCategory category) : this(name, category, TermFlags.None) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        /// <param name="category">The category.</param>
        /// <param name="flags">The flags.</param>
        public Terminal(string name, string errorAlias, TokenCategory category, TermFlags flags)
            : this(name, category, flags)
        {
            this.ErrorAlias = errorAlias;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="flags">The flags.</param>
        public Terminal(string name, TokenCategory category, TermFlags flags)
            : base(name)
        {
            Category = category;
            this.Flags |= flags;
            if (Category == TokenCategory.Outline)
                this.SetFlag(TermFlags.IsPunctuation);
            OutputTerminal = this;
        }
        #endregion

        #region fields and properties
        /// <summary>
        /// The category
        /// </summary>
        public TokenCategory Category = TokenCategory.Content;
        // Priority is used when more than one terminal may match the input char. 
        // It determines the order in which terminals will try to match input for a given char in the input.
        // For a given input char the scanner uses the hash table to look up the collection of terminals that may match this input symbol. 
        // It is the order in this collection that is determined by Priority property - the higher the priority, 
        // the earlier the terminal gets a chance to check the input. 
        /// <summary>
        /// The priority
        /// </summary>
        public int Priority = TerminalPriority.Normal; //default is 0

        //Terminal to attach to the output token. By default is set to the Terminal itself
        // Use SetOutputTerminal method to change it. For example of use see TerminalFactory.CreateSqlIdentifier and sample SQL grammar
        /// <summary>
        /// Gets or sets the output terminal.
        /// </summary>
        /// <value>The output terminal.</value>
        public Terminal OutputTerminal { get; protected set; }

        /// <summary>
        /// The editor information
        /// </summary>
        public TokenEditorInfo EditorInfo;
        /// <summary>
        /// The multiline index
        /// </summary>
        public byte MultilineIndex;
        /// <summary>
        /// The is pair for
        /// </summary>
        public Terminal IsPairFor;
        #endregion

        #region virtual methods: GetFirsts(), TryMatch, Init, TokenToString
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
        }

        //"Firsts" (chars) collections are used for quick search for possible matching terminal(s) using current character in the input stream.
        // A terminal might declare no firsts. In this case, the terminal is tried for match for any current input character. 
        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public virtual IList<string> GetFirsts()
        {
            return null;
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public virtual Token TryMatch(ParsingContext context, ISourceStream source)
        {
            return null;
        }

        /// <summary>
        /// Tokens to string.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>System.String.</returns>
        public virtual string TokenToString(Token token)
        {
            if (token.ValueString == this.Name)
                return token.ValueString;
            else
                return (token.ValueString ?? token.Text) + " (" + Name + ")";
        }


        #endregion

        #region Events: ValidateToken, ParserInputPreview
        /// <summary>
        /// Occurs when [validate token].
        /// </summary>
        public event EventHandler<ValidateTokenEventArgs> ValidateToken;
        /// <summary>
        /// Called when [validate token].
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void OnValidateToken(ParsingContext context)
        {
            if (ValidateToken != null)
                ValidateToken(this, context.SharedValidateTokenEventArgs);
        }

        //Invoked when ParseTreeNode is created from the token. This is parser-preview event, when parser
        // just received the token, wrapped it into ParseTreeNode and is about to look at it.
        /// <summary>
        /// Occurs when [parser input preview].
        /// </summary>
        public event EventHandler<ParsingEventArgs> ParserInputPreview;
        /// <summary>
        /// Called when [parser input preview].
        /// </summary>
        /// <param name="context">The context.</param>
        protected internal virtual void OnParserInputPreview(ParsingContext context)
        {
            if (ParserInputPreview != null)
                ParserInputPreview(this, context.SharedParsingEventArgs);
        }
        #endregion

        #region static comparison methods
        /// <summary>
        /// Bies the priority reverse.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        public static int ByPriorityReverse(Terminal x, Terminal y)
        {
            if (x.Priority > y.Priority)
                return -1;
            if (x.Priority == y.Priority)
                return 0;
            return 1;
        }
        #endregion

        #region Miscellaneous: SetOutputTerminal
        /// <summary>
        /// Sets the output terminal.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        /// <param name="outputTerminal">The output terminal.</param>
        public void SetOutputTerminal(Grammar grammar, Terminal outputTerminal)
        {
            OutputTerminal = outputTerminal;
            grammar.NonGrammarTerminals.Add(this);
        }

        #endregion
        //Priority constants
        /// <summary>
        /// The lowest priority
        /// </summary>
        [Obsolete("Deprecated: use constants in TerminalPriority class instead")]
        public const int LowestPriority = -1000;
        /// <summary>
        /// The highest priority
        /// </summary>
        [Obsolete("Deprecated: use constants in TerminalPriority class instead")]
        public const int HighestPriority = 1000;
        /// <summary>
        /// The reserved words priority
        /// </summary>
        [Obsolete("Deprecated: use constants in TerminalPriority class instead")]
        public const int ReservedWordsPriority = 900; //almost top one

        /// <summary>
        /// Terminalses to string.
        /// </summary>
        /// <param name="terminals">The terminals.</param>
        /// <returns>System.String.</returns>
        public static string TerminalsToString(IEnumerable<Terminal> terminals)
        {
            return string.Join(" ", terminals);
        }

    }

    /// <summary>
    /// Class TerminalSet.
    /// </summary>
    [Serializable]
    public class TerminalSet : HashSet<Terminal>
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Terminal.TerminalsToString(this);
        }
    }

    /// <summary>
    /// Class TerminalList.
    /// </summary>
    public class TerminalList : List<Terminal>
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Terminal.TerminalsToString(this);
        }
    }


}
