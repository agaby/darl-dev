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
// <copyright file="LiteralValueNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{
    /// <summary>
    /// Class LiteralValueNode.
    /// </summary>
    public class LiteralValueNode : AstNode
    {
        /// <summary>
        /// The value
        /// </summary>
        public object Value;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Value = treeNode.Token.Value;
            AsString = Value == null ? "null" : Value.ToString();
            if (Value is string)
                AsString = "\"" + AsString + "\"";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            return Task.FromResult<object>(Value);
        }

        /// <summary>
        /// Determines whether this instance is constant.
        /// </summary>
        /// <returns><c>true</c> if this instance is constant; otherwise, <c>false</c>.</returns>
        public override bool IsConstant()
        {
            return true;
        }
    }
}
