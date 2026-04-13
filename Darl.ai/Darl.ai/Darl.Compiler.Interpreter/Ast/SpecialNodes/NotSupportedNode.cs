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
// <copyright file="NotSupportedNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{
    //A substitute node to use on constructs that are not yet supported by language implementation.
    // The script would compile Ok but on attempt to evaluate the node would throw a runtime exception
    /// <summary>
    /// Class NotSupportedNode.
    /// </summary>
    public class NotSupportedNode : AstNode
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name;
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Name = treeNode.Term.ToString();
            AsString = Name + " (not supported)";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            thread.ThrowScriptError("Error Construct Not Supported", Name);
            return Task.FromResult<object>(null);
        }

    }
}
