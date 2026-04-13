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
// <copyright file="Extensions.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
namespace DarlCompiler.Interpreter
{
    /// <summary>
    /// Class InterpreterEnumExtensions.
    /// </summary>
    public static class InterpreterEnumExtensions
    {

        /// <summary>
        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this BindingRequestFlags enumValue, BindingRequestFlags flag)
        {
            return (enumValue & flag) != 0;
        }
        /// <summary>
        /// Determines whether the specified flag is set.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="flag">The flag.</param>
        /// <returns><c>true</c> if the specified flag is set; otherwise, <c>false</c>.</returns>
        public static bool IsSet(this AstNodeFlags enumValue, AstNodeFlags flag)
        {
            return (enumValue & flag) != 0;
        }

    }


}
