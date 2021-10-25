// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="NumberLiteral.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DarlCompiler.Parsing
{
    using DarlCompiler.Ast; // Microsoft.Scripting.Math.Complex64;
    using Complex64 = System.Numerics.Complex;

    /// <summary>
    /// Enum NumberOptions
    /// </summary>
    [Flags]
    public enum NumberOptions
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The default
        /// </summary>
        Default = None,

        /// <summary>
        /// The allow start end dot
        /// </summary>
        AllowStartEndDot = 0x01,     //python : http://docs.python.org/ref/floating.html
        /// <summary>
        /// The int only
        /// </summary>
        IntOnly = 0x02,
        /// <summary>
        /// The no dot after int
        /// </summary>
        NoDotAfterInt = 0x04,     //for use with IntOnly flag; essentially tells terminal to avoid matching integer if 
        // it is followed by dot (or exp symbol) - leave to another terminal that will handle float numbers
        /// <summary>
        /// The allow sign
        /// </summary>
        AllowSign = 0x08,
        /// <summary>
        /// The disable quick parse
        /// </summary>
        DisableQuickParse = 0x10,
        /// <summary>
        /// The allow letter after
        /// </summary>
        AllowLetterAfter = 0x20,      // allow number be followed by a letter or underscore; by default this flag is not set, so "3a" would not be 
        //  recognized as number followed by an identifier
        /// <summary>
        /// The allow underscore
        /// </summary>
        AllowUnderscore = 0x40,      // Ruby allows underscore inside number: 1_234

        //The following should be used with base-identifying prefixes
        /// <summary>
        /// The binary
        /// </summary>
        Binary = 0x0100, //e.g. GNU GCC C Extension supports binary number literals
        /// <summary>
        /// The octal
        /// </summary>
        Octal = 0x0200,
        /// <summary>
        /// The hexadecimal
        /// </summary>
        Hex = 0x0400,
    }


    /// <summary>
    /// Class NumberLiteral.
    /// </summary>
    public class NumberLiteral : CompoundTerminalBase
    {

        //Flags for internal use
        /// <summary>
        /// Enum NumberFlagsInternal
        /// </summary>
        public enum NumberFlagsInternal : short
        {
            /// <summary>
            /// The has dot
            /// </summary>
            HasDot = 0x1000,
            /// <summary>
            /// The has exp
            /// </summary>
            HasExp = 0x2000,
        }
        //nested helper class
        /// <summary>
        /// Class ExponentsTable.
        /// </summary>
        [Serializable]
        public class ExponentsTable : Dictionary<char, TypeCode> { }

        #region Public Consts
        //currently using TypeCodes for identifying numeric types
        /// <summary>
        /// The type code big int
        /// </summary>
        public const TypeCode TypeCodeBigInt = (TypeCode)30;
        /// <summary>
        /// The type code imaginary
        /// </summary>
        public const TypeCode TypeCodeImaginary = (TypeCode)31;
        #endregion

        #region constructors and initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NumberLiteral(string name)
            : this(name, NumberOptions.Default)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="astNodeType">Type of the ast node.</param>
        public NumberLiteral(string name, NumberOptions options, Type astNodeType)
            : this(name, options)
        {
            base.AstConfig.NodeType = astNodeType;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="astNodeCreator">The ast node creator.</param>
        public NumberLiteral(string name, NumberOptions options, AstNodeCreator astNodeCreator)
            : this(name, options)
        {
            base.AstConfig.NodeCreator = astNodeCreator;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberLiteral"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        public NumberLiteral(string name, NumberOptions options)
            : base(name)
        {
            Options = options;
            base.SetFlag(TermFlags.IsLiteral);
        }
        /// <summary>
        /// Adds the prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="options">The options.</param>
        public void AddPrefix(string prefix, NumberOptions options)
        {
            PrefixFlags.Add(prefix, (short)options);
            Prefixes.Add(prefix);
        }
        /// <summary>
        /// Adds the exponent symbols.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        /// <param name="floatType">Type of the float.</param>
        public void AddExponentSymbols(string symbols, TypeCode floatType)
        {
            foreach (var exp in symbols)
                _exponentsTable[exp] = floatType;
        }
        #endregion

        #region Public fields/properties: ExponentSymbols, Suffixes
        /// <summary>
        /// The options
        /// </summary>
        public NumberOptions Options;
        /// <summary>
        /// The decimal separator
        /// </summary>
        public char DecimalSeparator = '.';

        //Default types are assigned to literals without suffixes; first matching type used
        /// <summary>
        /// The default int types
        /// </summary>
        public TypeCode[] DefaultIntTypes = new TypeCode[] { TypeCode.Int32 };
        /// <summary>
        /// The default float type
        /// </summary>
        public TypeCode DefaultFloatType = TypeCode.Double;
        /// <summary>
        /// The _exponents table
        /// </summary>
        private readonly ExponentsTable _exponentsTable = new ExponentsTable();

        /// <summary>
        /// Determines whether the specified option is set.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><c>true</c> if the specified option is set; otherwise, <c>false</c>.</returns>
        public bool IsSet(NumberOptions option)
        {
            return (Options & option) != 0;
        }
        #endregion

        #region Private fields: _quickParseTerminators
        #endregion

        #region overrides
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            //Default Exponent symbols if table is empty 
            if (_exponentsTable.Count == 0 && !IsSet(NumberOptions.IntOnly))
            {
                _exponentsTable['e'] = DefaultFloatType;
                _exponentsTable['E'] = DefaultFloatType;
            }
            if (this.EditorInfo == null)
                this.EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None);
        }

        /// <summary>
        /// Gets the firsts.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public override IList<string> GetFirsts()
        {
            StringList result = new StringList();
            result.AddRange(base.Prefixes);
            //we assume that prefix is always optional, so number can always start with plain digit
            result.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
            // Python float numbers can start with a dot
            if (IsSet(NumberOptions.AllowStartEndDot))
                result.Add(DecimalSeparator.ToString());
            if (IsSet(NumberOptions.AllowSign))
                result.AddRange(new string[] { "-", "+" });
            return result;
        }

        //Most numbers in source programs are just one-digit instances of 0, 1, 2, and maybe others until 9
        // so we try to do a quick parse for these, without starting the whole general process
        /// <summary>
        /// Quicks the parse.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        protected override Token QuickParse(ParsingContext context, ISourceStream source)
        {
            if (IsSet(NumberOptions.DisableQuickParse)) return null;
            char current = source.PreviewChar;
            //it must be a digit followed by a whitespace or delimiter
            if (!char.IsDigit(current)) return null;
            if (!Grammar.IsWhitespaceOrDelimiter(source.NextPreviewChar))
                return null;
            int iValue = current - '0';
            object value = null;
            switch (DefaultIntTypes[0])
            {
                case TypeCode.Int32: value = iValue; break;
                case TypeCode.UInt32: value = (UInt32)iValue; break;
                case TypeCode.Byte: value = (byte)iValue; break;
                case TypeCode.SByte: value = (sbyte)iValue; break;
                case TypeCode.Int16: value = (Int16)iValue; break;
                case TypeCode.UInt16: value = (UInt16)iValue; break;
                default: return null;
            }
            source.PreviewPosition++;
            return source.CreateToken(this.OutputTerminal, value);
        }

        /// <summary>
        /// Initializes the details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="details">The details.</param>
        protected override void InitDetails(ParsingContext context, CompoundTokenDetails details)
        {
            base.InitDetails(context, details);
            details.Flags = (short)this.Options;
        }

        /// <summary>
        /// Reads the prefix.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        protected override void ReadPrefix(ISourceStream source, CompoundTokenDetails details)
        {
            //check that is not a  0 followed by dot; 
            //this may happen in Python for number "0.123" - we can mistakenly take "0" as octal prefix
            if (source.PreviewChar == '0' && source.NextPreviewChar == '.') return;
            base.ReadPrefix(source, details);
        }

        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool ReadBody(ISourceStream source, CompoundTokenDetails details)
        {
            //remember start - it may be different from source.TokenStart, we may have skipped prefix
            int start = source.PreviewPosition;
            char current = source.PreviewChar;
            if (IsSet(NumberOptions.AllowSign) && (current == '-' || current == '+'))
            {
                details.Sign = current.ToString();
                source.PreviewPosition++;
            }
            //Figure out digits set
            string digits = GetDigits(details);
            bool isDecimal = !details.IsSet((short)(NumberOptions.Binary | NumberOptions.Octal | NumberOptions.Hex));
            bool allowFloat = !IsSet(NumberOptions.IntOnly);
            bool foundDigits = false;

            while (!source.EOF())
            {
                current = source.PreviewChar;
                //1. If it is a digit, just continue going; the same for '_' if it is allowed
                if (digits.IndexOf(current) >= 0 || IsSet(NumberOptions.AllowUnderscore) && current == '_')
                {
                    source.PreviewPosition++;
                    foundDigits = true;
                    continue;
                }
                //2. Check if it is a dot in float number
                bool isDot = current == DecimalSeparator;
                if (allowFloat && isDot)
                {
                    //If we had seen already a dot or exponent, don't accept this one;
                    bool hasDotOrExp = details.IsSet((short)(NumberFlagsInternal.HasDot | NumberFlagsInternal.HasExp));
                    if (hasDotOrExp) break; //from while loop
                    //In python number literals (NumberAllowPointFloat) a point can be the first and last character,
                    //We accept dot only if it is followed by a digit
                    if (digits.IndexOf(source.NextPreviewChar) < 0 && !IsSet(NumberOptions.AllowStartEndDot))
                        break; //from while loop
                    details.Flags |= (int)NumberFlagsInternal.HasDot;
                    source.PreviewPosition++;
                    continue;
                }
                //3. Check if it is int number followed by dot or exp symbol
                bool isExpSymbol = (details.ExponentSymbol == null) && _exponentsTable.ContainsKey(current);
                if (!allowFloat && foundDigits && (isDot || isExpSymbol))
                {
                    //If no partial float allowed then return false - it is not integer, let float terminal recognize it as float
                    if (IsSet(NumberOptions.NoDotAfterInt)) return false;
                    //otherwise break, it is integer and we're done reading digits
                    break;
                }


                //4. Only for decimals - check if it is (the first) exponent symbol
                if (allowFloat && isDecimal && isExpSymbol)
                {
                    char next = source.NextPreviewChar;
                    bool nextIsSign = next == '-' || next == '+';
                    bool nextIsDigit = digits.IndexOf(next) >= 0;
                    if (!nextIsSign && !nextIsDigit)
                        break;  //Exponent should be followed by either sign or digit
                    //ok, we've got real exponent
                    details.ExponentSymbol = current.ToString(); //remember the exp char
                    details.Flags |= (int)NumberFlagsInternal.HasExp;
                    source.PreviewPosition++;
                    if (nextIsSign)
                        source.PreviewPosition++; //skip +/- explicitly so we don't have to deal with them on the next iteration
                    continue;
                }
                //4. It is something else (not digit, not dot or exponent) - we're done
                break; //from while loop
            }
            int end = source.PreviewPosition;
            if (!foundDigits)
                return false;
            details.Body = source.Text.Substring(start, end - start);
            return true;
        }

        /// <summary>
        /// Called when [validate token].
        /// </summary>
        /// <param name="context">The context.</param>
        public override void OnValidateToken(ParsingContext context)
        {
            if (!IsSet(NumberOptions.AllowLetterAfter))
            {
                var current = context.Source.PreviewChar;
                if (char.IsLetter(current) || current == '_')
                {
                    context.CurrentToken = context.CreateErrorToken("Number cannot be followed by a letter."); //  
                }
            }
            base.OnValidateToken(context);
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool ConvertValue(CompoundTokenDetails details)
        {
            if (String.IsNullOrEmpty(details.Body))
            {
                details.Error = "Invalid number.";
                return false;
            }
            AssignTypeCodes(details);
            //check for underscore
            if (IsSet(NumberOptions.AllowUnderscore) && details.Body.Contains("_"))
                details.Body = details.Body.Replace("_", string.Empty);

            //Try quick paths
            switch (details.TypeCodes[0])
            {
                case TypeCode.Int32:
                    if (QuickConvertToInt32(details)) return true;
                    break;
                case TypeCode.Double:
                    if (QuickConvertToDouble(details)) return true;
                    break;
            }

            //Go full cycle
            details.Value = null;
            foreach (TypeCode typeCode in details.TypeCodes)
            {
                switch (typeCode)
                {
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCodeImaginary:
                        return ConvertToFloat(typeCode, details);
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        if (details.Value == null) //if it is not done yet
                            TryConvertToLong(details, typeCode == TypeCode.UInt64); //try to convert to Long/Ulong and place the result into details.Value field;
                        if (TryCastToIntegerType(typeCode, details)) //now try to cast the ULong value to the target type 
                            return true;
                        break;
                }//switch
            }
            return false;
        }

        /// <summary>
        /// Assigns the type codes.
        /// </summary>
        /// <param name="details">The details.</param>
        private void AssignTypeCodes(CompoundTokenDetails details)
        {
            //Type could be assigned when we read suffix; if so, just exit
            if (details.TypeCodes != null) return;
            //Decide on float types
            var hasDot = details.IsSet((short)(NumberFlagsInternal.HasDot));
            var hasExp = details.IsSet((short)(NumberFlagsInternal.HasExp));
            var isFloat = (hasDot || hasExp);
            if (!isFloat)
            {
                details.TypeCodes = DefaultIntTypes;
                return;
            }
            //so we have a float. If we have exponent symbol then use it to select type
            if (hasExp)
            {
                TypeCode code;
                if (_exponentsTable.TryGetValue(details.ExponentSymbol[0], out code))
                {
                    details.TypeCodes = new TypeCode[] { code };
                    return;
                }
            }//if hasExp
            //Finally assign default float type
            details.TypeCodes = new TypeCode[] { DefaultFloatType };
        }

        #endregion

        #region private utilities
        /// <summary>
        /// Quicks the convert to int32.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool QuickConvertToInt32(CompoundTokenDetails details)
        {
            int radix = GetRadix(details);
            if (radix == 10 && details.Body.Length > 10) return false;    //10 digits is maximum for int32; int32.MaxValue = 2 147 483 647
            try
            {
                //workaround for .Net FX bug: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=278448
                int iValue = 0;
                if (radix == 10)
                    iValue = Convert.ToInt32(details.Body, CultureInfo.InvariantCulture);
                else
                    iValue = Convert.ToInt32(details.Body, radix);
                details.Value = iValue;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Quicks the convert to double.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool QuickConvertToDouble(CompoundTokenDetails details)
        {
            if (details.IsSet((short)(NumberOptions.Binary | NumberOptions.Octal | NumberOptions.Hex))) return false;
            if (details.IsSet((short)(NumberFlagsInternal.HasExp))) return false;
            if (DecimalSeparator != '.') return false;
            double dvalue;
            if (!double.TryParse(details.Body, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dvalue)) return false;
            details.Value = dvalue;
            return true;
        }


        /// <summary>
        /// Converts to float.
        /// </summary>
        /// <param name="typeCode">The type code.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ConvertToFloat(TypeCode typeCode, CompoundTokenDetails details)
        {
            //only decimal numbers can be fractions
            if (details.IsSet((short)(NumberOptions.Binary | NumberOptions.Octal | NumberOptions.Hex)))
            {
                details.Error = "Invalid number.";
                return false;
            }
            string body = details.Body;
            //Some languages allow exp symbols other than E. Check if it is the case, and change it to E
            // - otherwise .NET conversion methods may fail
            if (details.IsSet((short)NumberFlagsInternal.HasExp) && details.ExponentSymbol.ToUpper() != "E")
                body = body.Replace(details.ExponentSymbol, "E");

            //'.' decimal seperator required by invariant culture
            if (details.IsSet((short)NumberFlagsInternal.HasDot) && DecimalSeparator != '.')
                body = body.Replace(DecimalSeparator, '.');

            switch (typeCode)
            {
                case TypeCode.Double:
                case TypeCodeImaginary:
                    double dValue;
                    if (!Double.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out dValue)) return false;
                    if (typeCode == TypeCodeImaginary)
                        details.Value = new Complex64(0, dValue);
                    else
                        details.Value = dValue;
                    return true;
                case TypeCode.Single:
                    float fValue;
                    if (!Single.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue)) return false;
                    details.Value = fValue;
                    return true;
                case TypeCode.Decimal:
                    decimal decValue;
                    if (!Decimal.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out decValue)) return false;
                    details.Value = decValue;
                    return true;
            }//switch
            return false;
        }
        /// <summary>
        /// Tries the type of the cast to integer.
        /// </summary>
        /// <param name="typeCode">The type code.</param>
        /// <param name="details">The details.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool TryCastToIntegerType(TypeCode typeCode, CompoundTokenDetails details)
        {
            if (details.Value == null) return false;
            try
            {
                if (typeCode != TypeCode.UInt64)
                    details.Value = Convert.ChangeType(details.Value, typeCode, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                details.Error = string.Format("Error, Cannot Convert Value To Type", details.Value, typeCode.ToString());
                return false;
            }
        }

        /// <summary>
        /// Tries the convert to long.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <param name="useULong">if set to <c>true</c> [use u long].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool TryConvertToLong(CompoundTokenDetails details, bool useULong)
        {
            try
            {
                int radix = GetRadix(details);
                //workaround for .Net FX bug: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=278448
                if (radix == 10)
                    if (useULong)
                        details.Value = Convert.ToUInt64(details.Body, CultureInfo.InvariantCulture);
                    else
                        details.Value = Convert.ToInt64(details.Body, CultureInfo.InvariantCulture);
                else
                    if (useULong)
                    details.Value = Convert.ToUInt64(details.Body, radix);
                else
                    details.Value = Convert.ToInt64(details.Body, radix);
                return true;
            }
            catch (OverflowException)
            {
                details.Error = string.Format("Error, Cannot Convert Value To Type", details.Value, TypeCode.Int64.ToString());
                return false;
            }
        }


        /// <summary>
        /// Gets the radix.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>System.Int32.</returns>
        private int GetRadix(CompoundTokenDetails details)
        {
            if (details.IsSet((short)NumberOptions.Hex))
                return 16;
            if (details.IsSet((short)NumberOptions.Octal))
                return 8;
            if (details.IsSet((short)NumberOptions.Binary))
                return 2;
            return 10;
        }
        /// <summary>
        /// Gets the digits.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>System.String.</returns>
        private string GetDigits(CompoundTokenDetails details)
        {
            if (details.IsSet((short)NumberOptions.Hex))
                return Strings.HexDigits;
            if (details.IsSet((short)NumberOptions.Octal))
                return Strings.OctalDigits;
            if (details.IsSet((short)NumberOptions.Binary))
                return Strings.BinaryDigits;
            return Strings.DecimalDigits;
        }
        /// <summary>
        /// Gets the length of the safe word.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>System.Int32.</returns>
        private int GetSafeWordLength(CompoundTokenDetails details)
        {
            if (details.IsSet((short)NumberOptions.Hex))
                return 15;
            if (details.IsSet((short)NumberOptions.Octal))
                return 21; //maxWordLength 22
            if (details.IsSet((short)NumberOptions.Binary))
                return 63;
            return 19; //maxWordLength 20
        }
        /// <summary>
        /// Gets the section count.
        /// </summary>
        /// <param name="stringLength">Length of the string.</param>
        /// <param name="safeWordLength">Length of the safe word.</param>
        /// <returns>System.Int32.</returns>
        private int GetSectionCount(int stringLength, int safeWordLength)
        {
            int quotient = stringLength / safeWordLength;
            int remainder = stringLength - quotient * safeWordLength;
            return remainder == 0 ? quotient : quotient + 1;
        }

        //radix^safeWordLength
        /// <summary>
        /// Gets the safe word radix.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>System.UInt64.</returns>
        private ulong GetSafeWordRadix(CompoundTokenDetails details)
        {
            if (details.IsSet((short)NumberOptions.Hex))
                return 1152921504606846976;
            if (details.IsSet((short)NumberOptions.Octal))
                return 9223372036854775808;
            if (details.IsSet((short)NumberOptions.Binary))
                return 9223372036854775808;
            return 10000000000000000000;
        }

        #endregion


    }


}
