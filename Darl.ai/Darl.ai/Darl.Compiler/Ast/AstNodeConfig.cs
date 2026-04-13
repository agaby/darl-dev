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
// <copyright file="AstNodeConfig.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using System;

namespace DarlCompiler.Ast
{

    /// <summary>
    /// Class AstNodeEventArgs.
    /// </summary>
    public class AstNodeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AstNodeEventArgs"/> class.
        /// </summary>
        /// <param name="parseTreeNode">The parse tree node.</param>
        public AstNodeEventArgs(ParseTreeNode parseTreeNode)
        {
            ParseTreeNode = parseTreeNode;
        }
        /// <summary>
        /// The parse tree node
        /// </summary>
        public readonly ParseTreeNode ParseTreeNode;
        /// <summary>
        /// Gets the ast node.
        /// </summary>
        /// <value>The ast node.</value>
        public object AstNode
        {
            get { return ParseTreeNode.AstNode; }
        }
    }

    /// <summary>
    /// Delegate AstNodeCreator
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="parseNode">The parse node.</param>
    public delegate void AstNodeCreator(AstContext context, ParseTreeNode parseNode);
    /// <summary>
    /// Delegate DefaultAstNodeCreator
    /// </summary>
    /// <returns>System.Object.</returns>
    public delegate object DefaultAstNodeCreator();

    /// <summary>
    /// Class AstNodeConfig.
    /// </summary>
    public class AstNodeConfig
    {

        /// <summary>
        /// The node type
        /// </summary>
        public Type NodeType;
        /// <summary>
        /// The data
        /// </summary>
        public object Data; //config data passed to AstNode
        /// <summary>
        /// The node creator
        /// </summary>
        public AstNodeCreator NodeCreator; // a custom method for creating AST nodes
        /// <summary>
        /// The default node creator
        /// </summary>
        public DefaultAstNodeCreator DefaultNodeCreator; //default method for creating AST nodes; compiled dynamic method, wrapper around "new nodeType();"

        // An optional map (selector, filter) of child AST nodes. This facility provides a way to adjust the "map" of child nodes in various languages to 
        // the structure of a standard AST nodes (that can be shared between languages). 
        // ParseTreeNode object has two properties containing list nodes: ChildNodes and MappedChildNodes.
        //  If term.AstPartsMap is null, these two child node lists are identical and contain all child nodes. 
        // If AstParts is not null, then MappedChildNodes will contain child nodes identified by indexes in the map. 
        // For example, if we set  
        //           term.AstPartsMap = new int[] {1, 4, 2}; 
        // then MappedChildNodes will contain 3 child nodes, which are under indexes 1, 4, 2 in ChildNodes list.
        // The mapping is performed in CoreParser.cs, method CheckCreateMappedChildNodeList.
        /// <summary>
        /// The parts map
        /// </summary>
        public int[] PartsMap;


        /// <summary>
        /// Determines whether this instance [can create node].
        /// </summary>
        /// <returns><c>true</c> if this instance [can create node]; otherwise, <c>false</c>.</returns>
        public bool CanCreateNode()
        {
            return NodeCreator != null || NodeType != null;
        }

    }
}
