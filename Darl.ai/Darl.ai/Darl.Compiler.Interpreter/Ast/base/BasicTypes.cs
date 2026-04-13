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

    /// Delegate EvaluateMethod
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <returns>System.Object.</returns>
    public delegate Task<object> EvaluateMethod(ScriptThread thread);
    /// Delegate ValueSetterMethod
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="value">The value.</param>
    public delegate Task ValueSetterMethod(ScriptThread thread, object value);

    /// Enum AstNodeFlags
    /// </summary>
    [Flags]
    public enum AstNodeFlags
    {
        /// The none
        /// </summary>
        None = 0x0,
        /// The is tail
        /// </summary>
        IsTail = 0x01,     //the node is in tail position
        //IsScope = 0x02,     //node defines scope for local variables
    }

    /// Enum NodeUseType
    /// </summary>
    [Flags]
    public enum NodeUseType
    {
        /// The unknown
        /// </summary>
        Unknown,
        /// The name
        /// </summary>
        Name, //identifier used as a Name container - system would not use it's Evaluate method directly
        /// The call target
        /// </summary>
        CallTarget,
        /// The value read
        /// </summary>
        ValueRead,
        /// The value write
        /// </summary>
        ValueWrite,
        /// The value read write
        /// </summary>
        ValueReadWrite,
        /// The parameter
        /// </summary>
        Parameter,
        /// The keyword
        /// </summary>
        Keyword,
        /// The special symbol
        /// </summary>
        SpecialSymbol,
    }

}
