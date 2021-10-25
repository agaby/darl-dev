// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="NullNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{
    //A stub to use when AST node was not created (type not specified on NonTerminal, or error on creation)
    // The purpose of the stub is to throw a meaningful message when interpreter tries to evaluate null node.
    /// <summary>
    /// Class NullNode.
    /// </summary>
    public class NullNode : AstNode
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="NullNode"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        public NullNode(BnfTerm term)
        {
            this.Term = term;
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            thread.ThrowScriptError(Resources.ErrNullNodeEval, this.Term);
            return Task.FromResult((object)null); //never happens
        }
    }
}
