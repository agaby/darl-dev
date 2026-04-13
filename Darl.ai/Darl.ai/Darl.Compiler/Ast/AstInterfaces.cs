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
    /// Interface IBrowsableAstNode
    /// </summary>
    public interface IBrowsableAstNode
    {
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        int Position { get; }
        /// Gets the child nodes.
        /// </summary>
        /// <returns>IEnumerable.</returns>
        IEnumerable GetChildNodes();
    }


    /// Interface IAstNodeInit
    /// </summary>
    public interface IAstNodeInit
    {
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parseNode">The parse node.</param>
        void Init(AstContext context, ParseTreeNode parseNode);
    }
}
