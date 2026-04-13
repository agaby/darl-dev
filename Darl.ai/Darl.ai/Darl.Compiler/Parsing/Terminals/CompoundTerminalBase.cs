// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CompoundTerminalBase.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{
    #region About compound terminals
    /*
   As  it turns out, many terminal types in real-world languages have 3-part structure: prefix-body-suffix
   The body is essentially the terminal "value", while prefix and suffix are used to specify additional 
   information (options), while not  being a part of the terminal itself. 
   For example:
   1. c# numbers, may have 0x prefix for hex representation, and suffixes specifying 
     the exact data type of the literal (f, l, m, etc)
   2. c# string may have "@" prefix which disables escaping inside the string
   3. c# identifiers may have "@" prefix and escape sequences inside - just like strings
   4. Python string may have "u" and "r" prefixes, "r" working the same way as @ in c# strings
   5. VB string literals may have "c" suffix identifying that the literal is a character, not a string
   6. VB number literals and identifiers may have suffixes identifying data type
   
   So it seems like all these terminals have the format "prefix-body-suffix". 
   The CompoundTerminalBase base class implements base functionality supporting this multi-part structure.
   The IdentifierTerminal, NumberLiteral and StringLiteral classes inherit from this base class. 
   The methods in TerminalFactory static class demonstrate that with this architecture we can define the whole 
   variety of terminals for c#, Python and VB.NET languages. 
*/
    #endregion


    /// Class EscapeTable.
    /// </summary>
    [Serializable]
    public class EscapeTable : Dictionary<char, char> { }

    /// Class CompoundTerminalBase.
    /// </summary>
    public abstract class CompoundTerminalBase : Terminal
    {

        #region Nested classes
        /// Class ScanFlagTable.
        /// </summary>
        [Serializable]
        protected class ScanFlagTable : Dictionary<string, short> { }
        /// Class TypeCodeTable.
        /// </summary>
        [Serializable]
        protected class TypeCodeTable : Dictionary<string, TypeCode[]> { }

        /// Class CompoundTokenDetails.
        /// </summary>
        public class CompoundTokenDetails
        {
            /// The prefix
            /// </summary>
            public string Prefix;
            /// The body
            /// </summary>
            public string Body;
            /// The suffix
            /// </summary>
            public string Suffix;
            /// The sign
            /// </summary>
            public string Sign;
            /// The flags
            /// </summary>
            public short Flags;  //need to be short, because we need to save it in Scanner state for Vs integration
            /// The error
            /// </summary>
            public string Error;
            /// The type codes
            /// </summary>
            public TypeCode[] TypeCodes;
            /// The exponent symbol
            /// </summary>
            public string ExponentSymbol;  //exponent symbol for Number literal
            /// The start symbol
            /// </summary>
            public string StartSymbol;     //string start and end symbols
            /// The end symbol
            /// </summary>
            public string EndSymbol;
            /// The value
            /// </summary>
            public object Value;
            //partial token info, used by VS integration
            /// The partial ok
            /// </summary>
            public bool PartialOk;
            /// The is partial
            /// </summary>
            public bool IsPartial;
            /// The partial continues
            /// </summary>
            public bool PartialContinues;
            /// The sub type index
            /// </summary>
            public byte SubTypeIndex; //used for string literal kind
            //Flags helper method
            /// Determines whether the specified flag is set.
            /// </summary>
            /// <param name="flag">The flag.</param>
            /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
            public bool IsSet(short flag)
            {
                return (Flags & flag) != 0;
            }
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            public string Text { get { return Prefix + Body + Suffix; } }
        }

        #endregion

        #region constructors and initialization
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CompoundTerminalBase(string name) : this(name, TermFlags.None) { }
        /// Initializes a new instance of the <see cref="CompoundTerminalBase"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="flags">The flags.</param>
        public CompoundTerminalBase(string name, TermFlags flags)
            : base(name)
        {
            SetFlag(flags);
            Escapes = GetDefaultEscapes();
        }

        /// Adds the prefix flag.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="flags">The flags.</param>
        protected void AddPrefixFlag(string prefix, short flags)
        {
            PrefixFlags.Add(prefix, flags);
            Prefixes.Add(prefix);
        }
        /// Adds the suffix.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        /// <param name="typeCodes">The type codes.</param>
        public void AddSuffix(string suffix, params TypeCode[] typeCodes)
        {
            SuffixTypeCodes.Add(suffix, typeCodes);
            Suffixes.Add(suffix);
        }
        #endregion

        #region public Properties/Fields
        /// The escape character
        /// </summary>
        public Char EscapeChar = '\\';
        /// The escapes
        /// </summary>
        public EscapeTable Escapes = new EscapeTable();
        //Case sensitivity for prefixes and suffixes
        /// The case sensitive prefixes suffixes
        /// </summary>
        public bool CaseSensitivePrefixesSuffixes = false;
        #endregion


        #region private fields
        /// The prefix flags
        /// </summary>
        protected readonly ScanFlagTable PrefixFlags = new ScanFlagTable();
        /// The suffix type codes
        /// </summary>
        protected readonly TypeCodeTable SuffixTypeCodes = new TypeCodeTable();
        /// The prefixes
        /// </summary>
        protected StringList Prefixes = new StringList();
        /// The suffixes
        /// </summary>
        protected StringList Suffixes = new StringList();
        /// The _prefixes firsts
        /// </summary>
        CharHashSet _prefixesFirsts; //first chars of all prefixes, for fast prefix detection
        /// The _suffixes firsts
        /// </summary>
        CharHashSet _suffixesFirsts; //first chars of all suffixes, for fast suffix detection
        #endregion


        #region overrides: Init, TryMatch
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            //collect all suffixes, prefixes in lists and create sets of first chars for both
            Prefixes.Sort(StringList.LongerFirst);
            Suffixes.Sort(StringList.LongerFirst);

            _prefixesFirsts = new CharHashSet(CaseSensitivePrefixesSuffixes);
            _suffixesFirsts = new CharHashSet(CaseSensitivePrefixesSuffixes);
            foreach (string pfx in Prefixes)
                _prefixesFirsts.Add(pfx[0]);

            foreach (string sfx in Suffixes)
                _suffixesFirsts.Add(sfx[0]);
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
            Token token;
            //Try quick parse first, but only if we're not continuing
            if (context.VsLineScanState.Value == 0)
            {
                token = QuickParse(context, source);
                if (token != null) return token;
                source.PreviewPosition = source.Position; //revert the position
            }

            CompoundTokenDetails details = new CompoundTokenDetails();
            InitDetails(context, details);

            if (context.VsLineScanState.Value == 0)
                ReadPrefix(source, details);
            if (!ReadBody(source, details))
                return null;
            if (details.Error != null)
                return context.CreateErrorToken(details.Error);
            if (details.IsPartial)
            {
                details.Value = details.Body;
            }
            else
            {
                ReadSuffix(source, details);

                if (!ConvertValue(details))
                {
                    if (string.IsNullOrEmpty(details.Error))
                        details.Error = Resources.ErrInvNumber;
                    return context.CreateErrorToken(details.Error); // "Failed to convert the value: {0}"
                }
            }
            token = CreateToken(context, source, details);

            if (details.IsPartial)
            {
                //Save terminal state so we can continue
                context.VsLineScanState.TokenSubType = details.SubTypeIndex;
                context.VsLineScanState.TerminalFlags = details.Flags;
                context.VsLineScanState.TerminalIndex = this.MultilineIndex;
            }
            else
                context.VsLineScanState.Value = 0;
            return token;
        }

        /// Creates the token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns>Token.</returns>
        protected virtual Token CreateToken(ParsingContext context, ISourceStream source, CompoundTokenDetails details)
        {
            var token = source.CreateToken(this.OutputTerminal, details.Value);
            token.Details = details;
            if (details.IsPartial)
                token.Flags |= TokenFlags.IsIncomplete;
            return token;
        }

        /// Initializes the details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="details">The details.</param>
        protected virtual void InitDetails(ParsingContext context, CompoundTokenDetails details)
        {
            details.PartialOk = (context.Mode == ParseMode.VsLineScan);
            details.PartialContinues = (context.VsLineScanState.Value != 0);
        }

        /// Quicks the parse.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        protected virtual Token QuickParse(ParsingContext context, ISourceStream source)
        {
            return null;
        }

        /// Reads the prefix.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        protected virtual void ReadPrefix(ISourceStream source, CompoundTokenDetails details)
        {
            if (!_prefixesFirsts.Contains(source.PreviewChar))
                return;
            var comparisonType = CaseSensitivePrefixesSuffixes ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            foreach (string pfx in Prefixes)
            {
                // Prefixes are usually case insensitive, even if language is case-sensitive. So we cannot use source.MatchSymbol here,
                // we need case-specific comparison
                if (string.Compare(source.Text, source.PreviewPosition, pfx, 0, pfx.Length, comparisonType) != 0)
                    continue;
                //We found prefix
                details.Prefix = pfx;
                source.PreviewPosition += pfx.Length;
                //Set flag from prefix
                short pfxFlags;
                if (!string.IsNullOrEmpty(details.Prefix) && PrefixFlags.TryGetValue(details.Prefix, out pfxFlags))
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                    details.Flags |= pfxFlags;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
                return;
            }
        }

        /// Reads the body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected virtual bool ReadBody(ISourceStream source, CompoundTokenDetails details)
        {
            return false;
        }

        /// Reads the suffix.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        protected virtual void ReadSuffix(ISourceStream source, CompoundTokenDetails details)
        {
            if (!_suffixesFirsts.Contains(source.PreviewChar)) return;
            var comparisonType = CaseSensitivePrefixesSuffixes ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            foreach (string sfx in Suffixes)
            {
                //Suffixes are usually case insensitive, even if language is case-sensitive. So we cannot use source.MatchSymbol here,
                // we need case-specific comparison
                if (string.Compare(source.Text, source.PreviewPosition, sfx, 0, sfx.Length, comparisonType) != 0)
                    continue;
                //We found suffix
                details.Suffix = sfx;
                source.PreviewPosition += sfx.Length;
                //Set TypeCode from suffix
                TypeCode[] codes;
                if (!string.IsNullOrEmpty(details.Suffix) && SuffixTypeCodes.TryGetValue(details.Suffix, out codes))
                    details.TypeCodes = codes;
                return;
            }//foreach
        }

        /// Converts the value.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected virtual bool ConvertValue(CompoundTokenDetails details)
        {
            details.Value = details.Body;
            return false;
        }


        #endregion

        #region utils: GetDefaultEscapes
        /// Gets the default escapes.
        /// </summary>
        /// <returns>EscapeTable.</returns>
        public static EscapeTable GetDefaultEscapes()
        {
            EscapeTable escapes = new EscapeTable();
            escapes.Add('a', '\u0007');
            escapes.Add('b', '\b');
            escapes.Add('t', '\t');
            escapes.Add('n', '\n');
            escapes.Add('v', '\v');
            escapes.Add('f', '\f');
            escapes.Add('r', '\r');
            escapes.Add('"', '"');
            escapes.Add('\'', '\'');
            escapes.Add('\\', '\\');
            escapes.Add(' ', ' ');
            escapes.Add('\n', '\n'); //this is a special escape of the linebreak itself, 
            // when string ends with "\" char and continues on the next line
            return escapes;
        }
        #endregion

    }

}
