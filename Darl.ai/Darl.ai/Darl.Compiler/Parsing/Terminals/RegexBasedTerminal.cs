// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="RegexBasedTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DarlCompiler.Parsing
{

    /// Class RegexBasedTerminal.
    /// </summary>
    public class RegexBasedTerminal : Terminal
    {
        /// Initializes a new instance of the <see cref="RegexBasedTerminal"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="prefixes">The prefixes.</param>
        public RegexBasedTerminal(string pattern, params string[] prefixes)
            : base("name")
        {
            Pattern = pattern;
            if (prefixes != null)
                Prefixes.AddRange(prefixes);
        }
        /// Initializes a new instance of the <see cref="RegexBasedTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="prefixes">The prefixes.</param>
        public RegexBasedTerminal(string name, string pattern, params string[] prefixes)
            : base(name)
        {
            Pattern = pattern;
            if (prefixes != null)
                Prefixes.AddRange(prefixes);
        }

        #region public properties
        /// The pattern
        /// </summary>
        public readonly string Pattern;
        /// The prefixes
        /// </summary>
        public readonly StringList Prefixes = new StringList();

        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Regex Expression
        {
            get { return _expression; }
            /// The _expression
            /// </summary>
        }
        Regex _expression;
        #endregion

        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            string workPattern = @"\G(" + Pattern + ")";
            RegexOptions options = (Grammar.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            _expression = new Regex(workPattern, options);
            if (this.EditorInfo == null)
                this.EditorInfo = new TokenEditorInfo(TokenType.Unknown, TokenColor.Text, TokenTriggers.None);
        }

        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            return Prefixes;
        }

        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            Match m = _expression.Match(source.Text, source.PreviewPosition);
            if (!m.Success || m.Index != source.PreviewPosition)
                return null;
            source.PreviewPosition += m.Length;
            return source.CreateToken(this.OutputTerminal);
        }

    }




}
