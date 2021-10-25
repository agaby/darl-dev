// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="OperatorHandler.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlCompiler.Interpreter.Ast
{


    /// <summary>
    /// Class OperatorInfo.
    /// </summary>
    public class OperatorInfo
    {
        /// <summary>
        /// The symbol
        /// </summary>
        public string Symbol;
        /// <summary>
        /// The expression type
        /// </summary>
        public ExpressionType ExpressionType;
        /// <summary>
        /// The precedence
        /// </summary>
        public int Precedence;
        /// <summary>
        /// The associativity
        /// </summary>
        public Associativity Associativity;
    }

    /// <summary>
    /// Class OperatorInfoDictionary.
    /// </summary>
    [Serializable]
    public class OperatorInfoDictionary : Dictionary<string, OperatorInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorInfoDictionary"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public OperatorInfoDictionary(bool caseSensitive) : base(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase) { }

        /// <summary>
        /// Adds the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="expressionType">Type of the expression.</param>
        /// <param name="precedence">The precedence.</param>
        /// <param name="associativity">The associativity.</param>
        public void Add(string symbol, ExpressionType expressionType, int precedence, Associativity associativity = Associativity.Left)
        {
            var info = new OperatorInfo()
            {
                Symbol = symbol,
                ExpressionType = expressionType,
                Precedence = precedence,
                Associativity = associativity
            };
            this[symbol] = info;
        }
    }


    /// <summary>
    /// Class OperatorHandler.
    /// </summary>
    public class OperatorHandler
    {
        /// <summary>
        /// The _registered operators
        /// </summary>
        private readonly OperatorInfoDictionary _registeredOperators;


        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorHandler"/> class.
        /// </summary>
        /// <param name="languageCaseSensitive">if set to <c>true</c> [language case sensitive].</param>
        public OperatorHandler(bool languageCaseSensitive)
        {
            _registeredOperators = new OperatorInfoDictionary(languageCaseSensitive);
            BuildDefaultOperatorMappings();
        }

        /// <summary>
        /// Gets the type of the operator expression.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>ExpressionType.</returns>
        public ExpressionType GetOperatorExpressionType(string symbol)
        {
            OperatorInfo opInfo;
            if (_registeredOperators.TryGetValue(symbol, out opInfo))
                return opInfo.ExpressionType;
            return CustomExpressionTypes.NotAnExpression;
        }

        /// <summary>
        /// Gets the type of the unary operator expression.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>ExpressionType.</returns>
        public virtual ExpressionType GetUnaryOperatorExpressionType(string symbol)
        {
            switch (symbol.ToLowerInvariant())
            {
                case "+": return ExpressionType.UnaryPlus;
                case "-": return ExpressionType.Negate;
                case "!":
                case "not":
                case "~":
                    return ExpressionType.Not;
                default:
                    return CustomExpressionTypes.NotAnExpression;
            }
        }


        /// <summary>
        /// Gets the binary operator for augmented.
        /// </summary>
        /// <param name="augmented">The augmented.</param>
        /// <returns>ExpressionType.</returns>
        public virtual ExpressionType GetBinaryOperatorForAugmented(ExpressionType augmented)
        {
            switch (augmented)
            {
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                    return ExpressionType.AddChecked;
                case ExpressionType.AndAssign:
                    return ExpressionType.And;
                case ExpressionType.Decrement:
                    return ExpressionType.SubtractChecked;
                case ExpressionType.DivideAssign:
                    return ExpressionType.Divide;
                case ExpressionType.ExclusiveOrAssign:
                    return ExpressionType.ExclusiveOr;
                case ExpressionType.LeftShiftAssign:
                    return ExpressionType.LeftShift;
                case ExpressionType.ModuloAssign:
                    return ExpressionType.Modulo;
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                    return ExpressionType.MultiplyChecked;
                case ExpressionType.OrAssign:
                    return ExpressionType.Or;
                case ExpressionType.RightShiftAssign:
                    return ExpressionType.RightShift;
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                    return ExpressionType.SubtractChecked;
                default:
                    return CustomExpressionTypes.NotAnExpression;
            }
        }

        /// <summary>
        /// Builds the default operator mappings.
        /// </summary>
        /// <returns>OperatorInfoDictionary.</returns>
        public virtual OperatorInfoDictionary BuildDefaultOperatorMappings()
        {
            var dict = _registeredOperators;
            dict.Clear();
            int p = 0; //precedence

            p += 10;
            dict.Add("=", ExpressionType.Assign, p);
            dict.Add("+=", ExpressionType.AddAssignChecked, p);
            dict.Add("-=", ExpressionType.SubtractAssignChecked, p);
            dict.Add("*=", ExpressionType.MultiplyAssignChecked, p);
            dict.Add("/=", ExpressionType.DivideAssign, p);
            dict.Add("%=", ExpressionType.ModuloAssign, p);
            dict.Add("|=", ExpressionType.OrAssign, p);
            dict.Add("&=", ExpressionType.AndAssign, p);
            dict.Add("^=", ExpressionType.ExclusiveOrAssign, p);

            p += 10;
            dict.Add("==", ExpressionType.Equal, p);
            dict.Add("!=", ExpressionType.NotEqual, p);
            dict.Add("<>", ExpressionType.NotEqual, p);

            p += 10;
            dict.Add("<", ExpressionType.LessThan, p);
            dict.Add("<=", ExpressionType.LessThanOrEqual, p);
            dict.Add(">", ExpressionType.GreaterThan, p);
            dict.Add(">=", ExpressionType.GreaterThanOrEqual, p);

            p += 10;
            dict.Add("|", ExpressionType.Or, p);
            dict.Add("or", ExpressionType.Or, p);
            dict.Add("||", ExpressionType.OrElse, p);
            dict.Add("orelse", ExpressionType.OrElse, p);
            dict.Add("^", ExpressionType.ExclusiveOr, p);
            dict.Add("xor", ExpressionType.ExclusiveOr, p);

            p += 10;
            dict.Add("&", ExpressionType.And, p);
            dict.Add("and", ExpressionType.And, p);
            dict.Add("&&", ExpressionType.AndAlso, p);
            dict.Add("andalso", ExpressionType.AndAlso, p);

            p += 10;
            dict.Add("!", ExpressionType.Not, p);
            dict.Add("not", ExpressionType.Not, p);

            p += 10;
            dict.Add("<<", ExpressionType.LeftShift, p);
            dict.Add(">>", ExpressionType.RightShift, p);

            p += 10;
            dict.Add("+", ExpressionType.AddChecked, p);
            dict.Add("-", ExpressionType.SubtractChecked, p);

            p += 10;
            dict.Add("*", ExpressionType.MultiplyChecked, p);
            dict.Add("/", ExpressionType.Divide, p);
            dict.Add("%", ExpressionType.Modulo, p);
            dict.Add("**", ExpressionType.Power, p);

            p += 10;
            dict.Add("??", ExpressionType.Coalesce, p);
            dict.Add("?", ExpressionType.Conditional, p);

            return dict;
        }

    }


}
