/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AstInterfaces.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using System.Collections;


namespace DarlCompiler.Ast
{
    // Grammar Explorer uses this interface to discover and display the AST tree after parsing the input
    // (Grammar Explorer additionally uses ToString method of the node to get the text representation of the node)
    /// <summary>
    /// Interface IBrowsableAstNode
    /// </summary>
    public interface IBrowsableAstNode
    {
        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        int Position { get; }
        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <returns>IEnumerable.</returns>
        IEnumerable GetChildNodes();
    }


    /// <summary>
    /// Interface IAstNodeInit
    /// </summary>
    public interface IAstNodeInit
    {
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parseNode">The parse node.</param>
        void Init(AstContext context, ParseTreeNode parseNode);
    }
}
