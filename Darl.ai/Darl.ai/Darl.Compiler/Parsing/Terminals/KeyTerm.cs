// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="KeyTerm.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using Darl.ai;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class KeyTermTable.
    /// </summary>
    [Serializable]
    public class KeyTermTable : Dictionary<string, KeyTerm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTermTable"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public KeyTermTable(StringComparer comparer) : base(100, comparer) { }
    }
    /// <summary>
    /// Class KeyTermList.
    /// </summary>
    public class KeyTermList : List<KeyTerm> { }

    //Keyterm is a keyword or a special symbol used in grammar rules, for example: begin, end, while, =, *, etc.
    // So "key" comes from the Keyword. 
    /// <summary>
    /// Class KeyTerm.
    /// </summary>
    public class KeyTerm : Terminal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTerm"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="name">The name.</param>
        public KeyTerm(string text, string name) : base(name)
        {
            Text = text;
            base.ErrorAlias = name;
            this.Flags |= TermFlags.NoAstNode;
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; private set; }

        //Normally false, meaning keywords (symbols in grammar consisting of letters) cannot be followed by a letter or digit
        /// <summary>
        /// The allow alpha after keyword
        /// </summary>
        public bool AllowAlphaAfterKeyword = false;

        #region overrides: TryMatch, Init, GetPrefixes(), ToString() 
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);

            #region comments about keyterms priority
            // Priority - determines the order in which multiple terminals try to match input for a given current char in the input.
            // For a given input char the scanner looks up the collection of terminals that may match this input symbol. It is the order
            // in this collection that is determined by Priority value - the higher the priority, the earlier the terminal gets a chance 
            // to check the input. 
            // Keywords found in grammar by default have lowest priority to allow other terminals (like identifiers)to check the input first.
            // Additionally, longer symbols have higher priority, so symbols like "+=" should have higher priority value than "+" symbol. 
            // As a result, Scanner would first try to match "+=", longer symbol, and if it fails, it will try "+". 
            // Reserved words are the opposite - they have the highest priority
            #endregion
            if (Flags.IsSet(TermFlags.IsReservedWord))
                base.Priority = TerminalPriority.ReservedWords + Text.Length; //the longer the word, the higher is the priority
            else
                base.Priority = TerminalPriority.Low + Text.Length;
            //Setup editor info      
            if (this.EditorInfo != null) return;
            TokenType tknType = TokenType.Identifier;
            if (Flags.IsSet(TermFlags.IsOperator))
                tknType |= TokenType.Operator;
            else if (Flags.IsSet(TermFlags.IsDelimiter | TermFlags.IsPunctuation))
                tknType |= TokenType.Delimiter;
            TokenTriggers triggers = TokenTriggers.None;
            if (this.Flags.IsSet(TermFlags.IsBrace))
                triggers |= TokenTriggers.MatchBraces;
            if (this.Flags.IsSet(TermFlags.IsMemberSelect))
                triggers |= TokenTriggers.MemberSelect;
            TokenColor color = TokenColor.Text;
            if (Flags.IsSet(TermFlags.IsKeyword))
                color = TokenColor.Keyword;
            this.EditorInfo = new TokenEditorInfo(tknType, color, triggers);
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            if (!source.MatchSymbol(Text))
                return null;
            source.PreviewPosition += Text.Length;
            //In case of keywords, check that it is not followed by letter or digit
            if (this.Flags.IsSet(TermFlags.IsKeyword) && !AllowAlphaAfterKeyword)
            {
                var previewChar = source.PreviewChar;
                if (char.IsLetterOrDigit(previewChar) || previewChar == '_') return null; //reject
            }
            var token = source.CreateToken(this.OutputTerminal, Text);
            return token;
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            return new string[] { Text };
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (Name != Text) return Name;
            return Text;
        }
        /// <summary>
        /// Tokens to string.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>System.String.</returns>
        public override string TokenToString(Token token)
        {
            var keyw = Flags.IsSet(TermFlags.IsKeyword) ? Resources.LabelKeyword : Resources.LabelKeySymbol; //"(Keyword)" : "(Key symbol)"
            var result = (token.ValueString ?? token.Text) + " " + keyw;
            return result;
        }
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

    }


}
