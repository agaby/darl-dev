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
// <copyright file="LanguageRuntime.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Class ConsoleWriteEventArgs.
    /// </summary>
    public class ConsoleWriteEventArgs : EventArgs
    {
        /// <summary>
        /// The text
        /// </summary>
        public string Text;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWriteEventArgs"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public ConsoleWriteEventArgs(string text)
        {
            Text = text;
        }
    }


    //Note: mark the derived language-specific class as sealed - important for JIT optimizations
    // details here: http://www.codeproject.com/KB/dotnet/JITOptimizations.aspx
    /// <summary>
    /// Class LanguageRuntime.
    /// </summary>
    public partial class LanguageRuntime
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// <summary>
        /// The operator handler
        /// </summary>
        public OperatorHandler OperatorHandler;
        //Converter of the result for comparison operation; converts bool value to values
        // specific for the language
        /// <summary>
        /// The bool result converter
        /// </summary>
        public UnaryOperatorMethod BoolResultConverter = null;
        //An unassigned reserved object for a language implementation
        /// <summary>
        /// Gets or sets the none value.
        /// </summary>
        /// <value>The none value.</value>
        public NoneClass NoneValue { get; protected set; }

        //Built-in binding sources
        /// <summary>
        /// The built ins
        /// </summary>
        public BindingSourceTable BuiltIns;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageRuntime"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public LanguageRuntime(LanguageData language)
        {
            Language = language;
            NoneValue = NoneClass.Value;
            BuiltIns = new BindingSourceTable(Language.Grammar.CaseSensitive);
            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Init()
        {
            InitOperatorImplementations();
        }

        /// <summary>
        /// Determines whether the specified value is true.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified value is true; otherwise, <c>false</c>.</returns>
        public virtual bool IsTrue(object value)
        {
            if (value is bool)
                return (bool)value;
            if (value is int)
                return ((int)value != 0);
            if (value == NoneValue)
                return false;
            return value != null;
        }

        /// <summary>
        /// Throws the error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.Exception"></exception>
        protected internal void ThrowError(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            throw new Exception(message);
        }

        /// <summary>
        /// Throws the script error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="DarlCompiler.Interpreter.ScriptException"></exception>
        protected internal void ThrowScriptError(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            throw new ScriptException(message);
        }

    }

}

