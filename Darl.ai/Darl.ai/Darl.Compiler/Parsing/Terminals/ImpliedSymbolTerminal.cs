/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ImpliedSymbolTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using Darl.ai;

namespace DarlCompiler.Parsing
{
    //In some grammars there is a situation when some operator symbol can be skipped in source text and should be implied by parser.
    // In arithmetics, we often imply "*" operator in formulas:
    //  x y => x * y.
    // The SearchGrammar in Samples provides another example: two consequtive terms imply "and" operator and should be treated as such:
    //   x y   => x AND y 
    // We could use a simple nullable Non-terminal terminal in this case, but the problem is that we cannot associate precedence
    // and associativity with non-terminal, only with terminals. Precedence is important here because the implied symbol identifies binary
    // operation, so parser should be able to use precedence value(s) when resolving shift/reduce ambiguity. 
    // So here comes ImpliedSymbolTerminal - it is a terminal that produces a token with empty text. 
    // It relies on scanner-parser link enabled - so the implied symbol token is created ONLY 
    // when the current parser state allows it and there are no other alternatives (hence lowest priority value).
    // See SearchGrammar as an example of use of this terminal. 
    /// Class ImpliedSymbolTerminal.
    /// </summary>
    public class ImpliedSymbolTerminal : Terminal
    {
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ImpliedSymbolTerminal(string name)
            : base(name)
        {
            this.Priority = TerminalPriority.Low; //This terminal should be tried after all candidate terminals failed. 
        }

        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(DarlCompiler.Parsing.GrammarData grammarData)
        {
            base.Init(grammarData);
            //Check that Parser-scanner link is enabled - this terminal can be used only if this link is enabled
            if (Grammar.LanguageFlags.IsSet(LanguageFlags.DisableScannerParserLink))
                grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrImpliedOpUseParserLink, this.Name);
            //"ImpliedSymbolTerminal cannot be used in grammar with DisableScannerParserLink flag set"
        }

        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            return source.CreateToken(this); //Create an empty token representing an implied symbol.
        }

    }
}
