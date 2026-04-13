/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="IdentifierNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    /// Class IdentifierNode.
    /// </summary>
    public class IdentifierNode : AstNode
    {
        /// The symbol
        /// </summary>
        public string Symbol;
        /// The _accessor
        /// </summary>
        private Binding _accessor;

        /// Initializes a new instance of the <see cref="IdentifierNode"/> class.
        /// </summary>
        public IdentifierNode() { }

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Symbol = treeNode.Token.ValueString;
            AsString = Symbol;
        }

        //Executed only once, on the first call
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            _accessor = thread.Bind(Symbol, BindingRequestFlags.Read);
            this.Evaluate = _accessor.GetValueRef; // Optimization - directly set method ref to accessor's method. EvaluateReader;
            var result = this.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// Does the set value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        public override void DoSetValue(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;  //standard prologue
            if (_accessor == null)
            {
                _accessor = thread.Bind(Symbol, BindingRequestFlags.Write | BindingRequestFlags.ExistingOrNew);
            }
            _accessor.SetValueRef(thread, value);
            thread.CurrentNode = Parent;  //standard epilogue
        }

    }
}
