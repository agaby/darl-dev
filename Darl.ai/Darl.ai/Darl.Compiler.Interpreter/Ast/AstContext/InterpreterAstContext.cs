/// <summary>
/// InterpreterAstContext.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="InterpreterAstContext.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System.Collections.Generic;

namespace DarlCompiler.Interpreter.Ast
{
    //Extension of AstContext
    /// <summary>
    /// Class InterpreterAstContext.
    /// </summary>
    public class InterpreterAstContext : AstContext
    {
        /// <summary>
        /// The operator handler
        /// </summary>
        public readonly OperatorHandler OperatorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterAstContext"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="operatorHandler">The operator handler.</param>
        public InterpreterAstContext(LanguageData language, Dictionary<string, ILocalStore>? stores = null, OperatorHandler? operatorHandler = null)
            : base(language)
        {
            OperatorHandler = operatorHandler ?? new OperatorHandler(language.Grammar.CaseSensitive);
            base.DefaultIdentifierNodeType = typeof(IdentifierNode);
            base.DefaultLiteralNodeType = typeof(LiteralValueNode);
            base.DefaultNodeType = null;
            base.Stores = stores;
        }

    }
}
