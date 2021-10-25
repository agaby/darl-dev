// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="RegexLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DarlCompiler.Parsing
{
    // Regular expression literal, like javascript literal:   /abc?/i
    // Allows optional switches
    // example:
    //  regex = /abc\\\/de/
    //  matches fragments like  "abc\/de" 
    // Note: switches are returned in token.Details field. Unlike in StringLiteral, we don't need to unescape the escaped chars,
    // (this is the job of regex engine), we only need to correctly recognize the end of expression

    /// <summary>
    /// Enum RegexTermOptions
    /// </summary>
    [Flags]
    public enum RegexTermOptions
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The allow letter after
        /// </summary>
        AllowLetterAfter = 0x01, //if not set (default) then any following letter (after legal switches) is reported as invalid switch
        /// <summary>
        /// The create reg ex object
        /// </summary>
        CreateRegExObject = 0x02,  //if set, token.Value contains Regex object; otherwise, it contains a pattern (string)
        /// <summary>
        /// The unique switches
        /// </summary>
        UniqueSwitches = 0x04,    //require unique switches

        /// <summary>
        /// The default
        /// </summary>
        Default = CreateRegExObject | UniqueSwitches,
    }

    /// <summary>
    /// Class RegexLiteral.
    /// </summary>
    public class RegexLiteral : Terminal
    {
        /// <summary>
        /// Class RegexSwitchTable.
        /// </summary>
        [Serializable]
        public class RegexSwitchTable : Dictionary<char, RegexOptions> { }

        /// <summary>
        /// The start symbol
        /// </summary>
        public Char StartSymbol = '/';
        /// <summary>
        /// The end symbol
        /// </summary>
        public Char EndSymbol = '/';
        /// <summary>
        /// The escape symbol
        /// </summary>
        public Char EscapeSymbol = '\\';
        /// <summary>
        /// The switches
        /// </summary>
        public RegexSwitchTable Switches = new RegexSwitchTable();
        /// <summary>
        /// The default options
        /// </summary>
        public RegexOptions DefaultOptions = RegexOptions.None;
        /// <summary>
        /// The options
        /// </summary>
        public RegexTermOptions Options = RegexTermOptions.Default;

        /// <summary>
        /// The _stop chars
        /// </summary>
        private char[] _stopChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RegexLiteral(string name)
            : base(name)
        {
            Switches.Add('i', RegexOptions.IgnoreCase);
            Switches.Add('g', RegexOptions.None); //not sure what to do with this flag? anybody, any advice?
            Switches.Add('m', RegexOptions.Multiline);
            base.SetFlag(TermFlags.IsLiteral);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="startEndSymbol">The start end symbol.</param>
        /// <param name="escapeSymbol">The escape symbol.</param>
        public RegexLiteral(string name, char startEndSymbol, char escapeSymbol)
            : base(name)
        {
            StartSymbol = startEndSymbol;
            EndSymbol = startEndSymbol;
            EscapeSymbol = escapeSymbol;
        }

        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            _stopChars = new char[] { EndSymbol, '\r', '\n' };
        }
        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            var result = new StringList();
            result.Add(StartSymbol.ToString());
            return result;
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            while (true)
            {
                //Find next position
                var newPos = source.Text.IndexOfAny(_stopChars, source.PreviewPosition + 1);
                //we either didn't find it
                if (newPos == -1)
                    return context.CreateErrorToken(Resources.ErrNoEndForRegex);// "No end symbol for regex literal." 
                source.PreviewPosition = newPos;
                if (source.PreviewChar != EndSymbol)
                    //we hit CR or LF, this is an error
                    return context.CreateErrorToken(Resources.ErrNoEndForRegex);
                if (!CheckEscaped(source))
                    break;
            }
            source.PreviewPosition++; //move after end symbol
            //save pattern length, we will need it
            var patternLen = source.PreviewPosition - source.Location.Position - 2; //exclude start and end symbol
            //read switches and turn them into options
            RegexOptions options = RegexOptions.None;
            var switches = string.Empty;
            while (ReadSwitch(source, ref options))
            {
                if (IsSet(RegexTermOptions.UniqueSwitches) && switches.Contains(source.PreviewChar))
                    return context.CreateErrorToken(Resources.ErrDupRegexSwitch, source.PreviewChar); // "Duplicate switch '{0}' for regular expression" 
                switches += source.PreviewChar.ToString();
                source.PreviewPosition++;
            }
            //check following symbol
            if (!IsSet(RegexTermOptions.AllowLetterAfter))
            {
                var currChar = source.PreviewChar;
                if (char.IsLetter(currChar) || currChar == '_')
                    return context.CreateErrorToken(Resources.ErrInvRegexSwitch, currChar); // "Invalid switch '{0}' for regular expression"  
            }
            var token = source.CreateToken(this.OutputTerminal);
            //we have token, now what's left is to set its Value field. It is either pattern itself, or Regex instance
            string pattern = token.Text.Substring(1, patternLen); //exclude start and end symbol
            object value = pattern;
            if (IsSet(RegexTermOptions.CreateRegExObject))
            {
                value = new Regex(pattern, options);
            }
            token.Value = value;
            token.Details = switches; //save switches in token.Details
            return token;
        }

        /// <summary>
        /// Checks the escaped.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckEscaped(ISourceStream source)
        {
            var savePos = source.PreviewPosition;
            bool escaped = false;
            source.PreviewPosition--;
            while (source.PreviewChar == EscapeSymbol)
            {
                escaped = !escaped;
                source.PreviewPosition--;
            }
            source.PreviewPosition = savePos;
            return escaped;
        }
        /// <summary>
        /// Reads the switch.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ReadSwitch(ISourceStream source, ref RegexOptions options)
        {
            RegexOptions option;
            var result = Switches.TryGetValue(source.PreviewChar, out option);
            if (result)
                options |= option;
            return result;
        }

        /// <summary>
        /// Determines whether the specified option is set.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if the specified option is set; otherwise, <c>false</c>.</returns>
        public bool IsSet(RegexTermOptions option)
        {
            return (Options & option) != 0;
        }

    }

}
