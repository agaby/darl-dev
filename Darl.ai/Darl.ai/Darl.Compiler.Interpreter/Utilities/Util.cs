/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Util.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Interpreter
{
    /// Class Util.
    /// </summary>
    public static class Util
    {
        /// Safes the format.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        public static string SafeFormat(this string template, params object[] args)
        {
            if (args == null || args.Length == 0) return template;
            try
            {
                template = string.Format(template, args);
            }
            catch (Exception ex)
            {
                template = template + "(message formatting failed: " + ex.Message + " Args: " + string.Join(",", args) + ")";
            }
            return template;
        }

        /// Checks the specified condition.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.Exception"></exception>
        public static void Check(bool condition, string messageTemplate, params object[] args)
        {
            if (condition) return;
            throw new Exception(messageTemplate.SafeFormat(args));
        }

    }
}
