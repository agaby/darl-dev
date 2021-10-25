// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="FreeTextLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarlCompiler.Parsing
{
    // Sometimes language definition includes tokens that have no specific format, but are just "all text until some terminator character(s)";
    // FreeTextTerminal allows easy implementation of such language element.

    /// <summary>
    /// Enum FreeTextOptions
    /// </summary>
    [Flags]
    public enum FreeTextOptions
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The consume terminator
        /// </summary>
        ConsumeTerminator = 0x01, //move source pointer beyond terminator (so token "consumes" it from input), but don't include it in token text
        /// <summary>
        /// The include terminator
        /// </summary>
        IncludeTerminator = 0x02, // include terminator into token text/value
        /// <summary>
        /// The allow EOF
        /// </summary>
        AllowEof = 0x04, // treat EOF as legitimate terminator
        /// <summary>
        /// The allow empty
        /// </summary>
        AllowEmpty = 0x08,
    }

    /// <summary>
    /// Class FreeTextLiteral.
    /// </summary>
    public class FreeTextLiteral : Terminal
    {
        /// <summary>
        /// The terminators
        /// </summary>
        public StringSet Terminators = new StringSet();
        /// <summary>
        /// The firsts
        /// </summary>
        public StringSet Firsts = new StringSet();
        /// <summary>
        /// The escapes
        /// </summary>
        public StringDictionary Escapes = new StringDictionary();
        /// <summary>
        /// The free text options
        /// </summary>
        public FreeTextOptions FreeTextOptions;
        /// <summary>
        /// The _stop chars
        /// </summary>
        private char[] _stopChars;
        /// <summary>
        /// The _is simple
        /// </summary>
        bool _isSimple; //True if we have a single Terminator and no escapes
        /// <summary>
        /// The _single terminator
        /// </summary>
        string _singleTerminator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeTextLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="terminators">The terminators.</param>
        public FreeTextLiteral(string name, params string[] terminators) : this(name, FreeTextOptions.None, terminators) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeTextLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="freeTextOptions">The free text options.</param>
        /// <param name="terminators">The terminators.</param>
        public FreeTextLiteral(string name, FreeTextOptions freeTextOptions, params string[] terminators)
            : base(name)
        {
            FreeTextOptions = freeTextOptions;
            Terminators.UnionWith(terminators);
            base.SetFlag(TermFlags.IsLiteral);
            base.SetFlag(TermFlags.NoAstNode); //ane 12/05/18
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            var result = new StringList();
            result.AddRange(Firsts);
            return result;
        }
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            _isSimple = Terminators.Count == 1 && Escapes.Count == 0;
            if (_isSimple)
            {
                _singleTerminator = Terminators.First();
                return;
            }
            var stopChars = new CharHashSet();
            foreach (var key in Escapes.Keys)
                stopChars.Add(key[0]);
            foreach (var t in Terminators)
                stopChars.Add(t[0]);
            _stopChars = stopChars.ToArray();
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            if (!TryMatchPrefixes(context, source))
                return null;
            return _isSimple ? TryMatchContentSimple(context, source) : TryMatchContentExtended(context, source);
        }

        /// <summary>
        /// Tries the match prefixes.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool TryMatchPrefixes(ParsingContext context, ISourceStream source)
        {
            if (Firsts.Count == 0)
                return true;
            foreach (var first in Firsts)
                if (source.MatchSymbol(first))
                {
                    source.PreviewPosition += first.Length;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Tries the match content simple.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        private Token TryMatchContentSimple(ParsingContext context, ISourceStream source)
        {
            var startPos = source.PreviewPosition;
            var termLen = _singleTerminator.Length;
            var stringComp = Grammar.CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            int termPos = source.Text.IndexOf(_singleTerminator, startPos, stringComp);
            if (termPos < 0 && IsSet(FreeTextOptions.AllowEof))
                termPos = source.Text.Length;
            if (termPos < 0)
                return context.CreateErrorToken(Resources.ErrFreeTextNoEndTag, _singleTerminator);
            var textEnd = termPos;
            if (IsSet(FreeTextOptions.IncludeTerminator))
                textEnd += termLen;
            var tokenText = source.Text.Substring(startPos, textEnd - startPos);
            if (string.IsNullOrEmpty(tokenText) && (this.FreeTextOptions & Parsing.FreeTextOptions.AllowEmpty) == 0)
                return null;
            // The following line is a fix submitted by user rmcase
            source.PreviewPosition = IsSet(FreeTextOptions.ConsumeTerminator) ? termPos + termLen : termPos;
            return source.CreateToken(this.OutputTerminal, tokenText);
        }

        /// <summary>
        /// Tries the match content extended.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        private Token TryMatchContentExtended(ParsingContext context, ISourceStream source)
        {
            StringBuilder tokenText = new StringBuilder();
            while (true)
            {
                //Find next position of one of stop chars
                var nextPos = source.Text.IndexOfAny(_stopChars, source.PreviewPosition);
                if (nextPos == -1)
                {
                    if (IsSet(FreeTextOptions.AllowEof))
                    {
                        source.PreviewPosition = source.Text.Length;
                        return source.CreateToken(this.OutputTerminal);
                    }
                    else
                        return null;
                }
                var newText = source.Text.Substring(source.PreviewPosition, nextPos - source.PreviewPosition);
                tokenText.Append(newText);
                source.PreviewPosition = nextPos;
                //if it is escape, add escaped text and continue search
                if (CheckEscape(source, tokenText))
                    continue;
                //check terminators
                if (CheckTerminators(source, tokenText))
                    break; //from while (true); we reached 
                //The current stop is not at escape or terminator; add this char to token text and move on 
                tokenText.Append(source.PreviewChar);
                source.PreviewPosition++;
            }
            var text = tokenText.ToString();
            if (string.IsNullOrEmpty(text) && (this.FreeTextOptions & Parsing.FreeTextOptions.AllowEmpty) == 0)
                return null;
            return source.CreateToken(this.OutputTerminal, text);
        }

        /// <summary>
        /// Checks the escape.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="tokenText">The token text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckEscape(ISourceStream source, StringBuilder tokenText)
        {
            foreach (var dictEntry in Escapes)
            {
                if (source.MatchSymbol(dictEntry.Key))
                {
                    source.PreviewPosition += dictEntry.Key.Length;
                    tokenText.Append(dictEntry.Value);
                    return true;
                }
            }//foreach
            return false;
        }

        /// <summary>
        /// Checks the terminators.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="tokenText">The token text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckTerminators(ISourceStream source, StringBuilder tokenText)
        {
            foreach (var term in Terminators)
                if (source.MatchSymbol(term))
                {
                    if (IsSet(FreeTextOptions.IncludeTerminator))
                        tokenText.Append(term);
                    if (IsSet(FreeTextOptions.ConsumeTerminator | FreeTextOptions.IncludeTerminator))
                        source.PreviewPosition += term.Length;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Determines whether the specified option is set.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if the specified option is set; otherwise, <c>false</c>.</returns>
        private bool IsSet(FreeTextOptions option)
        {
            return (this.FreeTextOptions & option) != 0;
        }
    }

}
