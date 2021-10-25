// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="StringLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using DarlCompiler.Ast;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Enum StringOptions
    /// </summary>
    [Flags]
    public enum StringOptions : short
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The is character
        /// </summary>
        IsChar = 0x01,
        /// <summary>
        /// The allows doubled quote
        /// </summary>
        AllowsDoubledQuote = 0x02, //Convert doubled start/end symbol to a single symbol; for ex. in SQL, '' -> '
        /// <summary>
        /// The allows line break
        /// </summary>
        AllowsLineBreak = 0x04,
        /// <summary>
        /// The is template
        /// </summary>
        IsTemplate = 0x08, //Can include embedded expressions that should be evaluated on the fly; ex in Ruby: "hello #{name}"
        /// <summary>
        /// The no escapes
        /// </summary>
        NoEscapes = 0x10,
        /// <summary>
        /// The allows u escapes
        /// </summary>
        AllowsUEscapes = 0x20,
        /// <summary>
        /// The allows x escapes
        /// </summary>
        AllowsXEscapes = 0x40,
        /// <summary>
        /// The allows octal escapes
        /// </summary>
        AllowsOctalEscapes = 0x80,
        /// <summary>
        /// The allows all escapes
        /// </summary>
        AllowsAllEscapes = AllowsUEscapes | AllowsXEscapes | AllowsOctalEscapes,

    }

    //Container for settings of template string parser, to interpret strings having embedded values or expressions
    // like in Ruby:
    // "Hello, #{name}"
    // Default values match settings for Ruby strings
    /// <summary>
    /// Class StringTemplateSettings.
    /// </summary>
    public class StringTemplateSettings
    {
        /// <summary>
        /// The start tag
        /// </summary>
        public string StartTag = "#{";
        /// <summary>
        /// The end tag
        /// </summary>
        public string EndTag = "}";
        /// <summary>
        /// The expression root
        /// </summary>
        public NonTerminal ExpressionRoot;
    }

    /// <summary>
    /// Class StringLiteral.
    /// </summary>
    public class StringLiteral : CompoundTerminalBase
    {

        /// <summary>
        /// Enum StringFlagsInternal
        /// </summary>
        public enum StringFlagsInternal : short
        {
            /// <summary>
            /// The has escapes
            /// </summary>
            HasEscapes = 0x100,
        }

        #region StringSubType
        /// <summary>
        /// Class StringSubType.
        /// </summary>
        class StringSubType
        {
            /// <summary>
            /// The start
            /// </summary>
            internal readonly string Start, End;
            /// <summary>
            /// The flags
            /// </summary>
            internal readonly StringOptions Flags;
            /// <summary>
            /// The index
            /// </summary>
            internal readonly byte Index;
            /// <summary>
            /// Initializes a new instance of the <see cref="StringSubType"/> class.
            /// </summary>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="flags">The flags.</param>
            /// <param name="index">The index.</param>
            internal StringSubType(string start, string end, StringOptions flags, byte index)
            {
                Start = start;
                End = end;
                Flags = flags;
                Index = index;
            }

            /// <summary>
            /// Longers the start first.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            /// <returns>System.Int32.</returns>
            internal static int LongerStartFirst(StringSubType x, StringSubType y)
            {
                try
                {//in case any of them is null
                    if (x.Start.Length > y.Start.Length) return -1;
                }
                catch { }
                return 0;
            }
        }
        /// <summary>
        /// Class StringSubTypeList.
        /// </summary>
        class StringSubTypeList : List<StringSubType>
        {
            /// <summary>
            /// Adds the specified start.
            /// </summary>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="flags">The flags.</param>
            internal void Add(string start, string end, StringOptions flags)
            {
                base.Add(new StringSubType(start, end, flags, (byte)this.Count));
            }
        }
        #endregion

        #region constructors and initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StringLiteral(string name)
            : base(name)
        {
            base.SetFlag(TermFlags.IsLiteral);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="options">The options.</param>
        public StringLiteral(string name, string startEndSymbol, StringOptions options)
            : this(name)
        {
            _subtypes.Add(startEndSymbol, startEndSymbol, options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        public StringLiteral(string name, string startEndSymbol) : this(name, startEndSymbol, StringOptions.None) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="options">The options.</param>
        /// <param name="astNodeType">Type of the ast node.</param>
        public StringLiteral(string name, string startEndSymbol, StringOptions options, Type astNodeType)
            : this(name, startEndSymbol, options)
        {
            base.AstConfig.NodeType = astNodeType;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StringLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="options">The options.</param>
        /// <param name="astNodeCreator">The ast node creator.</param>
        public StringLiteral(string name, string startEndSymbol, StringOptions options, AstNodeCreator astNodeCreator)
            : this(name, startEndSymbol, options)
        {
            base.AstConfig.NodeCreator = astNodeCreator;
        }

        /// <summary>
        /// Adds the start end.
        /// </summary>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="stringOptions">The string options.</param>
        public void AddStartEnd(string startEndSymbol, StringOptions stringOptions)
        {
            AddStartEnd(startEndSymbol, startEndSymbol, stringOptions);
        }
        /// <summary>
        /// Adds the start end.
        /// </summary>
        /// <param name="startSymbol">The start symbol.</param>
        /// <param name="endSymbol">The end symbol.</param>
        /// <param name="stringOptions">The string options.</param>
        public void AddStartEnd(string startSymbol, string endSymbol, StringOptions stringOptions)
        {
            _subtypes.Add(startSymbol, endSymbol, stringOptions);
        }
        /// <summary>
        /// Adds the prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="flags">The flags.</param>
        public void AddPrefix(string prefix, StringOptions flags)
        {
            base.AddPrefixFlag(prefix, (short)flags);
        }

        #endregion

        #region Properties/Fields
        /// <summary>
        /// The _subtypes
        /// </summary>
        private readonly StringSubTypeList _subtypes = new StringSubTypeList();
        /// <summary>
        /// The _start symbols firsts
        /// </summary>
        string _startSymbolsFirsts; //first chars  of start-end symbols
        #endregion

        #region overrides: Init, GetFirsts, ReadBody, etc...
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            _startSymbolsFirsts = string.Empty;
            if (_subtypes.Count == 0)
            {
                grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrInvStrDef, this.Name); //"Error in string literal [{0}]: No start/end symbols specified."
                return;
            }
            //collect all start-end symbols in lists and create strings of first chars
            var allStartSymbols = new StringSet(); //to detect duplicate start symbols
            _subtypes.Sort(StringSubType.LongerStartFirst);
            bool isTemplate = false;
            foreach (StringSubType subType in _subtypes)
            {
                if (allStartSymbols.Contains(subType.Start))
                    grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null,
                      Resources.ErrDupStartSymbolStr, subType.Start, this.Name); //"Duplicate start symbol {0} in string literal [{1}]."
                allStartSymbols.Add(subType.Start);
                _startSymbolsFirsts += subType.Start[0].ToString();
                if ((subType.Flags & StringOptions.IsTemplate) != 0) isTemplate = true;
            }
            if (!CaseSensitivePrefixesSuffixes)
                _startSymbolsFirsts = _startSymbolsFirsts.ToLower() + _startSymbolsFirsts.ToUpper();
            //Set multiline flag
            foreach (StringSubType info in _subtypes)
            {
                if ((info.Flags & StringOptions.AllowsLineBreak) != 0)
                {
                    SetFlag(TermFlags.IsMultiline);
                    break;
                }
            }
            //For templates only
            if (isTemplate)
            {
                //Check that template settings object is provided
                var templateSettings = this.AstConfig.Data as StringTemplateSettings;
                if (templateSettings == null)
                    grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrTemplNoSettings, this.Name); //"Error in string literal [{0}]: IsTemplate flag is set, but TemplateSettings is not provided."
                else if (templateSettings.ExpressionRoot == null)
                    grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrTemplMissingExprRoot, this.Name); //""
                else if (!Grammar.SnippetRoots.Contains(templateSettings.ExpressionRoot))
                    grammarData.Language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrTemplExprNotRoot, this.Name); //""
            }//if
            //Create editor info
            if (this.EditorInfo == null)
                this.EditorInfo = new TokenEditorInfo(TokenType.String, TokenColor.String, TokenTriggers.None);
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            StringList result = new StringList();
            result.AddRange(Prefixes);
            //we assume that prefix is always optional, so string can start with start-end symbol
            foreach (char ch in _startSymbolsFirsts)
                result.Add(ch.ToString());
            return result;
        }

        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool ReadBody(ISourceStream source, CompoundTokenDetails details)
        {
            if (!details.PartialContinues)
            {
                if (!ReadStartSymbol(source, details)) return false;
            }
            return CompleteReadBody(source, details);
        }

        /// <summary>
        /// Completes the read body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CompleteReadBody(ISourceStream source, CompoundTokenDetails details)
        {
            bool escapeEnabled = !details.IsSet((short)StringOptions.NoEscapes);
            int start = source.PreviewPosition;
            string endQuoteSymbol = details.EndSymbol;
            string endQuoteDoubled = endQuoteSymbol + endQuoteSymbol; //doubled quote symbol
            bool lineBreakAllowed = details.IsSet((short)StringOptions.AllowsLineBreak);
            //1. Find the string end
            // first get the position of the next line break; we are interested in it to detect malformed string, 
            //  therefore do it only if linebreak is NOT allowed; if linebreak is allowed, set it to -1 (we don't care).  
            int nlPos = lineBreakAllowed ? -1 : source.Text.IndexOf('\n', source.PreviewPosition);
            //fix by ashmind for EOF right after opening symbol
            while (true)
            {
                int endPos = source.Text.IndexOf(endQuoteSymbol, source.PreviewPosition);
                //Check for partial token in line-scanning mode
                if (endPos < 0 && details.PartialOk && lineBreakAllowed)
                {
                    ProcessPartialBody(source, details);
                    return true;
                }
                //Check for malformed string: either EndSymbol not found, or LineBreak is found before EndSymbol
                bool malformed = endPos < 0 || nlPos >= 0 && nlPos < endPos;
                if (malformed)
                {
                    //Set source position for recovery: move to the next line if linebreak is not allowed.
                    if (nlPos > 0) endPos = nlPos;
                    if (endPos > 0) source.PreviewPosition = endPos + 1;
                    details.Error = Resources.ErrBadStrLiteral;//    "Mal-formed  string literal - cannot find termination symbol.";
                    return true; //we did find start symbol, so it is definitely string, only malformed
                }//if malformed

                if (source.EOF())
                    return true;

                //We found EndSymbol - check if it is escaped; if yes, skip it and continue search
                if (escapeEnabled && IsEndQuoteEscaped(source.Text, endPos))
                {
                    source.PreviewPosition = endPos + endQuoteSymbol.Length;
                    continue; //searching for end symbol
                }

                //Check if it is doubled end symbol
                source.PreviewPosition = endPos;
                if (details.IsSet((short)StringOptions.AllowsDoubledQuote) && source.MatchSymbol(endQuoteDoubled))
                {
                    source.PreviewPosition = endPos + endQuoteDoubled.Length;
                    continue;
                }//checking for doubled end symbol

                //Ok, this is normal endSymbol that terminates the string. 
                // Advance source position and get out from the loop
                details.Body = source.Text.Substring(start, endPos - start);
                source.PreviewPosition = endPos + endQuoteSymbol.Length;
                return true; //if we come here it means we're done - we found string end.
            }  //end of loop to find string end; 
        }
        /// <summary>
        /// Processes the partial body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        private void ProcessPartialBody(ISourceStream source, CompoundTokenDetails details)
        {
            int from = source.PreviewPosition;
            source.PreviewPosition = source.Text.Length;
            details.Body = source.Text.Substring(from, source.PreviewPosition - from);
            details.IsPartial = true;
        }

        /// <summary>
        /// Initializes the details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="details">The details.</param>
        protected override void InitDetails(ParsingContext context, CompoundTerminalBase.CompoundTokenDetails details)
        {
            base.InitDetails(context, details);
            if (context.VsLineScanState.Value != 0)
            {
                //we are continuing partial string on the next line
                details.Flags = context.VsLineScanState.TerminalFlags;
                details.SubTypeIndex = context.VsLineScanState.TokenSubType;
                var stringInfo = _subtypes[context.VsLineScanState.TokenSubType];
                details.StartSymbol = stringInfo.Start;
                details.EndSymbol = stringInfo.End;
            }
        }

        /// <summary>
        /// Reads the suffix.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        protected override void ReadSuffix(ISourceStream source, CompoundTerminalBase.CompoundTokenDetails details)
        {
            base.ReadSuffix(source, details);
            //"char" type can be identified by suffix (like VB where c suffix identifies char)
            // in this case we have details.TypeCodes[0] == char  and we need to set the IsChar flag
            if (details.TypeCodes != null && details.TypeCodes[0] == TypeCode.Char)
                details.Flags |= (int)StringOptions.IsChar;
            else
                //we may have IsChar flag set (from startEndSymbol, like in c# single quote identifies char)
                // in this case set type code
                if (details.IsSet((short)StringOptions.IsChar))
                details.TypeCodes = new TypeCode[] { TypeCode.Char };
        }

        /// <summary>
        /// Determines whether [is end quote escaped] [the specified text].
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="quotePosition">The quote position.</param>
        /// <returns><c>true</c> if [is end quote escaped] [the specified text]; otherwise, <c>false</c>.</returns>
        private bool IsEndQuoteEscaped(string text, int quotePosition)
        {
            bool escaped = false;
            int p = quotePosition - 1;
            while (p > 0 && text[p] == EscapeChar)
            {
                escaped = !escaped;
                p--;
            }
            return escaped;
        }

        /// <summary>
        /// Reads the start symbol.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ReadStartSymbol(ISourceStream source, CompoundTokenDetails details)
        {
            if (_startSymbolsFirsts.IndexOf(source.PreviewChar) < 0)
                return false;
            foreach (StringSubType subType in _subtypes)
            {
                if (!source.MatchSymbol(subType.Start))
                    continue;
                //We found start symbol
                details.StartSymbol = subType.Start;
                details.EndSymbol = subType.End;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                details.Flags |= (short)subType.Flags;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
                details.SubTypeIndex = subType.Index;
                source.PreviewPosition += subType.Start.Length;
                return true;
            }
            return false;
        }


        //Extract the string content from lexeme, adjusts the escaped and double-end symbols
        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool ConvertValue(CompoundTokenDetails details)
        {
            string value = details.Body;
            bool escapeEnabled = !details.IsSet((short)StringOptions.NoEscapes);
            //Fix all escapes
            if (escapeEnabled && value.IndexOf(EscapeChar) >= 0)
            {
                details.Flags |= (int)StringFlagsInternal.HasEscapes;
                string[] arr = value.Split(EscapeChar);
                bool ignoreNext = false;
                //we skip the 0 element as it is not preceeded by "\"
                for (int i = 1; i < arr.Length; i++)
                {
                    if (ignoreNext)
                    {
                        ignoreNext = false;
                        continue;
                    }
                    string s = arr[i];
                    if (string.IsNullOrEmpty(s))
                    {
                        //it is "\\" - escaped escape symbol. 
                        arr[i] = @"\";
                        ignoreNext = true;
                        continue;
                    }
                    //The char is being escaped is the first one; replace it with char in Escapes table
                    char first = s[0];
                    char newFirst;
                    if (Escapes.TryGetValue(first, out newFirst))
                        arr[i] = newFirst + s.Substring(1);
                    else
                    {
                        arr[i] = HandleSpecialEscape(arr[i], details);
                    }//else
                }//for i
                value = string.Join(string.Empty, arr);
            }

            //Check for doubled end symbol
            string endSymbol = details.EndSymbol;
            if (details.IsSet((short)StringOptions.AllowsDoubledQuote) && value.IndexOf(endSymbol) >= 0)
                value = value.Replace(endSymbol + endSymbol, endSymbol);

            if (details.IsSet((short)StringOptions.IsChar))
            {
                if (value.Length != 1)
                {
                    details.Error = Resources.ErrBadChar;  //"Invalid length of char literal - should be a single character.";
                    return false;
                }
                details.Value = value[0];
            }
            else
            {
                details.TypeCodes = new TypeCode[] { TypeCode.String };
                details.Value = value;
            }
            return true;
        }

        //Should support:  \Udddddddd, \udddd, \xdddd, \N{name}, \0, \ddd (octal),  
        /// <summary>
        /// Handles the special escape.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="details">The details.</param>
        /// <returns>System.String.</returns>
        protected virtual string HandleSpecialEscape(string segment, CompoundTokenDetails details)
        {
            if (string.IsNullOrEmpty(segment)) return string.Empty;
            int len, p; string digits; char ch; string result;
            char first = segment[0];
            switch (first)
            {
                case 'u':
                case 'U':
                    if (details.IsSet((short)StringOptions.AllowsUEscapes))
                    {
                        len = (first == 'u' ? 4 : 8);
                        if (segment.Length < len + 1)
                        {
                            details.Error = string.Format(Resources.ErrBadUnEscape, segment.Substring(len + 1), len);// "Invalid unicode escape ({0}), expected {1} hex digits."
                            return segment;
                        }
                        digits = segment.Substring(1, len);
                        ch = (char)Convert.ToUInt32(digits, 16);
                        result = ch + segment.Substring(len + 1);
                        return result;
                    }//if
                    break;
                case 'x':
                    if (details.IsSet((short)StringOptions.AllowsXEscapes))
                    {
                        //x-escape allows variable number of digits, from one to 4; let's count them
                        p = 1; //current position
                        while (p < 5 && p < segment.Length)
                        {
                            if (Strings.HexDigits.IndexOf(segment[p]) < 0) break;
                            p++;
                        }
                        //p now point to char right after the last digit
                        if (p <= 1)
                        {
                            details.Error = Resources.ErrBadXEscape; // @"Invalid \x escape, at least one digit expected.";
                            return segment;
                        }
                        digits = segment.Substring(1, p - 1);
                        ch = (char)Convert.ToUInt32(digits, 16);
                        result = ch + segment.Substring(p);
                        return result;
                    }//if
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    if (details.IsSet((short)StringOptions.AllowsOctalEscapes))
                    {
                        //octal escape allows variable number of digits, from one to 3; let's count them
                        p = 0; //current position
                        while (p < 3 && p < segment.Length)
                        {
                            if (Strings.OctalDigits.IndexOf(segment[p]) < 0) break;
                            p++;
                        }
                        //p now point to char right after the last digit
                        digits = segment.Substring(0, p);
                        ch = (char)Convert.ToUInt32(digits, 8);
                        result = ch + segment.Substring(p);
                        return result;
                    }
                    break;
            }
            details.Error = string.Format(Resources.ErrInvEscape, segment); //"Invalid escape sequence: \{0}"
            return segment;
        }
        #endregion

    }

}
