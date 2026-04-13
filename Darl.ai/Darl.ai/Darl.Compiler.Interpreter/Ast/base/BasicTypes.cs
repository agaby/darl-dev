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
// <copyright file="BasicTypes.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Delegate EvaluateMethod
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <returns>System.Object.</returns>
    public delegate Task<object> EvaluateMethod(ScriptThread thread);
    /// <summary>
    /// Delegate ValueSetterMethod
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="value">The value.</param>
    public delegate Task ValueSetterMethod(ScriptThread thread, object value);

    /// <summary>
    /// Enum AstNodeFlags
    /// </summary>
    [Flags]
    public enum AstNodeFlags
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The is tail
        /// </summary>
        IsTail = 0x01,     //the node is in tail position
        //IsScope = 0x02,     //node defines scope for local variables
    }

    /// <summary>
    /// Enum NodeUseType
    /// </summary>
    [Flags]
    public enum NodeUseType
    {
        /// <summary>
        /// The unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// The name
        /// </summary>
        Name, //identifier used as a Name container - system would not use it's Evaluate method directly
        /// <summary>
        /// The call target
        /// </summary>
        CallTarget,
        /// <summary>
        /// The value read
        /// </summary>
        ValueRead,
        /// <summary>
        /// The value write
        /// </summary>
        ValueWrite,
        /// <summary>
        /// The value read write
        /// </summary>
        ValueReadWrite,
        /// <summary>
        /// The parameter
        /// </summary>
        Parameter,
        /// <summary>
        /// The keyword
        /// </summary>
        Keyword,
        /// <summary>
        /// The special symbol
        /// </summary>
        SpecialSymbol,
    }

}
