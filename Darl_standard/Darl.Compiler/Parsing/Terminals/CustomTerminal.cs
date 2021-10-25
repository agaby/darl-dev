// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CustomTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

/// <summary>
/// The Parsing namespace.
/// </summary>
namespace DarlCompiler.Parsing
{
    //Terminal based on custom method; allows creating custom match without creating new class derived from Terminal 
    /// <summary>
    /// Delegate MatchHandler
    /// </summary>
    /// <param name="terminal">The terminal.</param>
    /// <param name="context">The context.</param>
    /// <param name="source">The source.</param>
    /// <returns>Token.</returns>
    public delegate Token MatchHandler(Terminal terminal, ParsingContext context, ISourceStream source);

    /// <summary>
    /// Class CustomTerminal.
    /// </summary>
    public class CustomTerminal : Terminal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="prefixes">The prefixes.</param>
        public CustomTerminal(string name, MatchHandler handler, params string[] prefixes)
            : base(name)
        {
            _handler = handler;
            if (prefixes != null)
                Prefixes.AddRange(prefixes);
            this.EditorInfo = new TokenEditorInfo(TokenType.Unknown, TokenColor.Text, TokenTriggers.None);
        }

        /// <summary>
        /// The prefixes
        /// </summary>
        public readonly StringList Prefixes = new StringList();

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <value>The handler.</value>
        public MatchHandler Handler
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _handler; }
            /// <summary>
            /// The _handler
            /// </summary>
        }
        MatchHandler _handler;

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            return _handler(this, context, source);
        }
        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public override IList<string> GetFirsts()
        {
            return Prefixes;
        }
    }


}
