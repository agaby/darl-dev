// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="DataLiteralBase.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Parsing
{

    //DataLiteralBase is a base class for a set of specialized terminals with a primary purpose of building data readers
    // DsvLiteral is used for reading delimiter-separated values (DSV), comma-separated format is a specific case of DSV
    // FixedLengthLiteral may be used to read values of fixed length
    /// <summary>
    /// Class DataLiteralBase.
    /// </summary>
    public class DataLiteralBase : Terminal
    {
        /// <summary>
        /// The data type
        /// </summary>
        public TypeCode DataType;
        //For date format strings see MSDN help for "Custom format strings", available through help for DateTime.ParseExact(...) method
        /// <summary>
        /// The date time format
        /// </summary>
        public string DateTimeFormat = "d"; //standard format, identifies MM/dd/yyyy for invariant culture.
        /// <summary>
        /// The int radix
        /// </summary>
        public int IntRadix = 10; //Radix (base) for numeric numbers

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLiteralBase"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">Type of the data.</param>
        public DataLiteralBase(string name, TypeCode dataType)
            : base(name)
        {
            DataType = dataType;
        }

        /// <summary>
        /// Tries the match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>Token.</returns>
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            try
            {
                var textValue = ReadBody(context, source);
                if (textValue == null) return null;
                var value = ConvertValue(context, textValue);
                return source.CreateToken(this.OutputTerminal, value);
            }
            catch (Exception ex)
            {
                //we throw exception in DsvLiteral when we cannot find a closing quote for quoted value
                return context.CreateErrorToken(ex.Message);
            }
        }


        /// <summary>
        /// Reads the body.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected virtual string ReadBody(ParsingContext context, ISourceStream source)
        {
            return null;
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="textValue">The text value.</param>
        /// <returns>System.Object.</returns>
        protected virtual object ConvertValue(ParsingContext context, string textValue)
        {
            switch (DataType)
            {
                case TypeCode.String: return textValue;
                case TypeCode.DateTime: return DateTime.ParseExact(textValue, DateTimeFormat, context.Culture);
                case TypeCode.Single:
                case TypeCode.Double:
                    var dValue = Convert.ToDouble(textValue, context.Culture);
                    if (DataType == TypeCode.Double) return dValue;
                    return Convert.ChangeType(dValue, DataType, context.Culture);

                default: //integer types
                    var iValue = (IntRadix == 10) ? Convert.ToInt64(textValue, context.Culture) : Convert.ToInt64(textValue, IntRadix);
                    if (DataType == TypeCode.Int64) return iValue;
                    return Convert.ChangeType(iValue, DataType, context.Culture);
            }
        }

    }

}
