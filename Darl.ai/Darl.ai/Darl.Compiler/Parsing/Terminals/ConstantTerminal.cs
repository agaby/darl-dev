// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ConstantTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

/// The Parsing namespace.
/// </summary>
namespace DarlCompiler.Parsing
{
    //This terminal allows to declare a set of constants in the input language
    // It should be used when constant symbols do not look like normal identifiers; e.g. in Scheme, #t, #f are true/false
    // constants, and they don't fit into Scheme identifier pattern.
    /// Class ConstantsTable.
    /// </summary>
    [Serializable]
    public class ConstantsTable : Dictionary<string, object> { }
    /// Class ConstantTerminal.
    /// </summary>
    public class ConstantTerminal : Terminal
    {
        /// The constants
        /// </summary>
        public readonly ConstantsTable Constants = new ConstantsTable();
        /// Initializes a new instance of the <see cref="ConstantTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="nodeType">Type of the node.</param>
        public ConstantTerminal(string name, Type? nodeType = null)
            : base(name)
        {
            base.SetFlag(TermFlags.IsConstant);
            if (nodeType != null)
                base.AstConfig.NodeType = nodeType;
            this.Priority = TerminalPriority.High; //constants have priority over normal identifiers
        }

        /// Adds the specified lexeme.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="value">The value.</param>
        public void Add(string lexeme, object value)
        {
            this.Constants[lexeme] = value;
        }

        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            if (this.EditorInfo == null)
                this.EditorInfo = new TokenEditorInfo(TokenType.Unknown, TokenColor.Text, TokenTriggers.None);
        }

        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            string text = source.Text;
            foreach (var entry in Constants)
            {
                source.PreviewPosition = source.Position;
                var constant = entry.Key;
                if (source.PreviewPosition + constant.Length > text.Length) continue;
                if (source.MatchSymbol(constant))
                {
                    source.PreviewPosition += constant.Length;
                    if (!this.Grammar.IsWhitespaceOrDelimiter(source.PreviewChar))
                        continue; //make sure it is delimiter
                    return source.CreateToken(this.OutputTerminal, entry.Value);
                }
            }
            return null;
        }

        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            string[] array = new string[Constants.Count];
            Constants.Keys.CopyTo(array, 0);
            return array;
        }

    }



}
