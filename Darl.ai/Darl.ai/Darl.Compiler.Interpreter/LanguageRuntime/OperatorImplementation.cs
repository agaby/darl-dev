/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="OperatorImplementation.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Delegate UnaryOperatorMethod
    /// </summary>
    /// <param name="arg">The argument.</param>
    /// <returns>System.Object.</returns>
    public delegate object UnaryOperatorMethod(object arg);
    /// <summary>
    /// Delegate BinaryOperatorMethod
    /// </summary>
    /// <param name="arg1">The arg1.</param>
    /// <param name="arg2">The arg2.</param>
    /// <returns>System.Object.</returns>
    public delegate object BinaryOperatorMethod(object arg1, object arg2);

    #region OperatorDispatchKey class
    /// <summary>
    /// The struct is used as a key for the dictionary of operator implementations.
    /// Contains types of arguments for a method or operator implementation.
    /// </summary>
    public struct OperatorDispatchKey
    {
        /// <summary>
        /// The comparer
        /// </summary>
        public static readonly OperatorDispatchKeyComparer Comparer = new OperatorDispatchKeyComparer();
        /// <summary>
        /// The op
        /// </summary>
        public readonly ExpressionType Op;
        /// <summary>
        /// The arg1 type
        /// </summary>
        public readonly Type Arg1Type;
        /// <summary>
        /// The arg2 type
        /// </summary>
        public readonly Type Arg2Type;
        /// <summary>
        /// The hash code
        /// </summary>
        public readonly int HashCode;

        //For binary operators
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorDispatchKey"/> struct.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="arg1Type">Type of the arg1.</param>
        /// <param name="arg2Type">Type of the arg2.</param>
        public OperatorDispatchKey(ExpressionType op, Type arg1Type, Type arg2Type)
        {
            Op = op;
            Arg1Type = arg1Type;
            Arg2Type = arg2Type;
            int h0 = (int)Op;
            int h1 = Arg1Type.GetHashCode();
            int h2 = Arg2Type.GetHashCode();
            HashCode = unchecked(h0 << 8 ^ h1 << 4 ^ h2);
        }

        //For unary operators
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorDispatchKey"/> struct.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="arg1Type">Type of the arg1.</param>
        public OperatorDispatchKey(ExpressionType op, Type arg1Type)
        {
            Op = op;
            Arg1Type = arg1Type;
            Arg2Type = null;
            int h0 = (int)Op;
            int h1 = Arg1Type.GetHashCode();
            int h2 = 0;
            HashCode = unchecked(h0 << 8 ^ h1 << 4 ^ h2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return HashCode;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Op + "(" + Arg1Type + ", " + Arg2Type + ")";
        }
    }
    #endregion

    #region OperatorDispatchKeyComparer class
    // Note: I believe (guess) that a custom Comparer provided to a Dictionary is a bit more efficient 
    // than implementing IComparable on the key itself
    /// <summary>
    /// Class OperatorDispatchKeyComparer.
    /// </summary>
    public class OperatorDispatchKeyComparer : IEqualityComparer<OperatorDispatchKey>
    {
        /// <summary>
        /// Equalses the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Equals(OperatorDispatchKey x, OperatorDispatchKey y)
        {
            return x.HashCode == y.HashCode && x.Op == y.Op && x.Arg1Type == y.Arg1Type && x.Arg2Type == y.Arg2Type;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(OperatorDispatchKey obj)
        {
            return obj.HashCode;
        }
    }
    #endregion

    /// <summary>
    /// Class TypeConverterTable.
    /// </summary>
    [Serializable]
    public class TypeConverterTable : Dictionary<OperatorDispatchKey, UnaryOperatorMethod>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConverterTable"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public TypeConverterTable(int capacity) : base(capacity, OperatorDispatchKey.Comparer) { }

    }

    /// <summary>
    /// Class OperatorImplementationTable.
    /// </summary>
    [Serializable]
    public class OperatorImplementationTable : Dictionary<OperatorDispatchKey, OperatorImplementation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorImplementationTable"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public OperatorImplementationTable(int capacity) : base(capacity, OperatorDispatchKey.Comparer) { }
    }

    /// <summary>
    /// The OperatorImplementation class represents an implementation of an operator for specific argument types.
    /// </summary>
    /// <remarks>The OperatorImplementation is used for holding implementation for binary operators, unary operators,
    /// and type converters (special case of unary operators)
    /// it holds 4 method references for binary operators:
    /// converters for both arguments, implementation method and converter for the result.
    /// For unary operators (and type converters) the implementation is in Arg1Converter
    /// operator (arg1 is used); the converter method is stored in Arg1Converter; the target type is in CommonType</remarks>
    public sealed class OperatorImplementation
    {
        /// <summary>
        /// The key
        /// </summary>
        public readonly OperatorDispatchKey Key;
        // The type to which arguments are converted and no-conversion method for this type. 
        /// <summary>
        /// The common type
        /// </summary>
        public readonly Type CommonType;
        /// <summary>
        /// The base binary method
        /// </summary>
        public readonly BinaryOperatorMethod BaseBinaryMethod;
        //converters
        /// <summary>
        /// The arg1 converter
        /// </summary>
        internal UnaryOperatorMethod Arg1Converter;
        /// <summary>
        /// The arg2 converter
        /// </summary>
        internal UnaryOperatorMethod Arg2Converter;
        /// <summary>
        /// The result converter
        /// </summary>
        internal UnaryOperatorMethod ResultConverter;
        //A reference to the actual binary evaluator method - one of EvaluateConvXXX 
        /// <summary>
        /// The evaluate binary
        /// </summary>
        public BinaryOperatorMethod EvaluateBinary;
        // An overflow handler - the implementation to handle arithmetic overflow
        /// <summary>
        /// The overflow handler
        /// </summary>
        public OperatorImplementation OverflowHandler;
        // No-box counterpart for implementations with auto-boxed output. If this field <> null, then this is 
        // implementation with auto-boxed output
        /// <summary>
        /// The no box implementation
        /// </summary>
        public OperatorImplementation NoBoxImplementation;

        //constructor for binary operators
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorImplementation"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="baseBinaryMethod">The base binary method.</param>
        /// <param name="arg1Converter">The arg1 converter.</param>
        /// <param name="arg2Converter">The arg2 converter.</param>
        /// <param name="resultConverter">The result converter.</param>
        public OperatorImplementation(OperatorDispatchKey key, Type resultType, BinaryOperatorMethod baseBinaryMethod,
            UnaryOperatorMethod arg1Converter, UnaryOperatorMethod arg2Converter, UnaryOperatorMethod resultConverter)
        {
            Key = key;
            CommonType = resultType;
            Arg1Converter = arg1Converter;
            Arg2Converter = arg2Converter;
            ResultConverter = resultConverter;
            BaseBinaryMethod = baseBinaryMethod;
            SetupEvaluationMethod();
        }

        //constructor  for unary operators and type converters
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorImplementation"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public OperatorImplementation(OperatorDispatchKey key, Type type, UnaryOperatorMethod method)
        {
            Key = key;
            CommonType = type;
            Arg1Converter = method;
            Arg2Converter = null;
            ResultConverter = null;
            BaseBinaryMethod = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "[OpImpl for " + Key.ToString() + "]";
        }

        /// <summary>
        /// Setups the evaluation method.
        /// </summary>
        public void SetupEvaluationMethod()
        {
            if (BaseBinaryMethod == null)
                //special case - it is unary method, the method itself in Arg1Converter; LanguageRuntime.ExecuteUnaryOperator will handle this properly
                return;
            // Binary operator
            if (ResultConverter == null)
            {
                //without ResultConverter
                if (Arg1Converter == null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvNone;
                else if (Arg1Converter != null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvLeft;
                else if (Arg1Converter == null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvRight;
                else // if (Arg1Converter != null && arg2Converter != null)
                    EvaluateBinary = EvaluateConvBoth;
            }
            else
            {
                //with result converter
                if (Arg1Converter == null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvNoneConvResult;
                else if (Arg1Converter != null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvLeftConvResult;
                else if (Arg1Converter == null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvRightConvResult;
                else // if (Arg1Converter != null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvBothConvResult;
            }
        }

        /// <summary>
        /// Evaluates the conv none.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvNone(object arg1, object arg2)
        {
            return BaseBinaryMethod(arg1, arg2);
        }
        /// <summary>
        /// Evaluates the conv left.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvLeft(object arg1, object arg2)
        {
            return BaseBinaryMethod(Arg1Converter(arg1), arg2);
        }
        /// <summary>
        /// Evaluates the conv right.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvRight(object arg1, object arg2)
        {
            return BaseBinaryMethod(arg1, Arg2Converter(arg2));
        }
        /// <summary>
        /// Evaluates the conv both.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvBoth(object arg1, object arg2)
        {
            return BaseBinaryMethod(Arg1Converter(arg1), Arg2Converter(arg2));
        }

        /// <summary>
        /// Evaluates the conv none conv result.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvNoneConvResult(object arg1, object arg2)
        {
            return ResultConverter(BaseBinaryMethod(arg1, arg2));
        }
        /// <summary>
        /// Evaluates the conv left conv result.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvLeftConvResult(object arg1, object arg2)
        {
            return ResultConverter(BaseBinaryMethod(Arg1Converter(arg1), arg2));
        }
        /// <summary>
        /// Evaluates the conv right conv result.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvRightConvResult(object arg1, object arg2)
        {
            return ResultConverter(BaseBinaryMethod(arg1, Arg2Converter(arg2)));
        }
        /// <summary>
        /// Evaluates the conv both conv result.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateConvBothConvResult(object arg1, object arg2)
        {
            return ResultConverter(BaseBinaryMethod(Arg1Converter(arg1), Arg2Converter(arg2)));
        }
    }



}
