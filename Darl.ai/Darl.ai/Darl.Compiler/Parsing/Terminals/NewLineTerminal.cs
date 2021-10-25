// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="NewLineTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using Darl.ai;

namespace DarlCompiler.Parsing
{
    //This is a simple NewLine terminal recognizing line terminators for use in grammars for line-based languages like VB
    // instead of more complex alternative of using CodeOutlineFilter. 
    /// <summary>
    /// Class NewLineTerminal.
    /// </summary>
    public class NewLineTerminal : Terminal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NewLineTerminal(string name)
            : base(name, TokenCategory.Outline)
        {
            base.ErrorAlias = Resources.LabelLineBreak;  // "[line break]";
            this.Flags |= TermFlags.IsPunctuation;
        }

        /// <summary>
        /// The line terminators
        /// </summary>
        public string LineTerminators = "\n\r\v";

        #region overrides: Init, GetFirsts, TryMatch
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            Grammar.UsesNewLine = true; //That will prevent SkipWhitespace method from skipping new-line chars
        }
        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            StringList firsts = new StringList();
            foreach (char t in LineTerminators)
                firsts.Add(t.ToString());
            return firsts;
        }
        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            char current = source.PreviewChar;
            if (!LineTerminators.Contains(current)) return null;
            //Treat \r\n as a single terminator
            bool doExtraShift = (current == '\r' && source.NextPreviewChar == '\n');
            source.PreviewPosition++; //main shift
            if (doExtraShift)
                source.PreviewPosition++;
            Token result = source.CreateToken(this.OutputTerminal);
            return result;
        }

        #endregion


    }
}
