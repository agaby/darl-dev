/// <summary>
/// ExpressionListNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ExpressionListNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    //A node representing expression list - for example, list of argument expressions in function call
    /// <summary>
    /// Class ExpressionListNode.
    /// </summary>
    public class ExpressionListNode : AstNode
    {

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                AddChild(NodeUseType.Parameter, "expr", child);
            }
            AsString = "Expression list";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var values = new object[ChildNodes.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = ChildNodes[i].Evaluate(thread);
            }
            thread.CurrentNode = Parent; //standard epilogue
            return Task.FromResult<object>(values);
        }

    }

}
