/// <summary>
/// AcceptParserAction.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AcceptParserAction.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class AcceptParserAction.
    /// </summary>
    public class AcceptParserAction : ParserAction
    {

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(ParsingContext context)
        {
            context.CurrentParseTree.Root = context.ParserStack.Pop(); //Pop root
            context.Status = ParserStatus.Accepted;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Resources.LabelActionAccept;
        }
    }
}
