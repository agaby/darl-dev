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
// <copyright file="BinaryOperationNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{
    /// <summary>
    /// Class BinaryOperationNode.
    /// </summary>
    public class BinaryOperationNode : AstNode
    {
        /// <summary>
        /// The left
        /// </summary>
        public AstNode Left, Right;
        /// <summary>
        /// The op symbol
        /// </summary>
        public string OpSymbol;
        /// <summary>
        /// The op
        /// </summary>
        public ExpressionType Op;
        /// <summary>
        /// The _last used
        /// </summary>
        private OperatorImplementation _lastUsed;
        /// <summary>
        /// The _const value
        /// </summary>
        private object _constValue;
        /// <summary>
        /// The _failure count
        /// </summary>
        private int _failureCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationNode"/> class.
        /// </summary>
        public BinaryOperationNode() { }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = AddChild("Arg", nodes[0]);
            Right = AddChild("Arg", nodes[2]);
            var opToken = nodes[1].FindToken();
            OpSymbol = opToken.Text;
            var ictxt = context as InterpreterAstContext;
            Op = ictxt.OperatorHandler.GetOperatorExpressionType(OpSymbol);
            // Set error anchor to operator, so on error (Division by zero) the explorer will point to 
            // operator node as location, not to the very beginning of the first operand.
            ErrorAnchor = opToken.Location;
            AsString = Op + "(operator)";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            //assign implementation method
            switch (Op)
            {
                case ExpressionType.AndAlso:
                    this.Evaluate = EvaluateAndAlso;
                    break;
                case ExpressionType.OrElse:
                    this.Evaluate = EvaluateOrElse;
                    break;
                default:
                    this.Evaluate = DefaultEvaluateImplementation;
                    break;
            }
            // actually evaluate and get the result.
            var result = Evaluate(thread);
            // Check if result is constant - if yes, save the value and switch to method that directly returns the result.
            if (IsConstant())
            {
                _constValue = result;
                AsString = Op + "(operator) Const=" + _constValue;
                this.Evaluate = EvaluateConst;
            }
            thread.CurrentNode = Parent; //standard epilogue
            return Task.FromResult<object>(result);
        }

        /// <summary>
        /// Evaluates the and also.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private async Task<object> EvaluateAndAlso(ScriptThread thread)
        {
            var leftValue = await Left.Evaluate(thread);
            if (!thread.Runtime.IsTrue(leftValue)) return leftValue; //if false return immediately
            return await Right.Evaluate(thread);
        }
        /// <summary>
        /// Evaluates the or else.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private async Task<object> EvaluateOrElse(ScriptThread thread)
        {
            var leftValue = await Left.Evaluate(thread);
            if (thread.Runtime.IsTrue(leftValue)) return leftValue;
            return await Right.Evaluate(thread);
        }

        /// <summary>
        /// Evaluates the fast.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected object EvaluateFast(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var arg1 = Left.Evaluate(thread);
            var arg2 = Right.Evaluate(thread);
            //If we have _lastUsed, go straight for it; if types mismatch it will throw
            if (_lastUsed != null)
            {
                try
                {
                    var res = _lastUsed.EvaluateBinary(arg1, arg2);
                    thread.CurrentNode = Parent; //standard epilogue
                    return res;
                }
                catch
                {
                    _lastUsed = null;
                    _failureCount++;
                    // if failed 3 times, change to method without direct try
                    if (_failureCount > 3)
                        Evaluate = DefaultEvaluateImplementation;
                } //catch
            }// if _lastUsed
            // go for normal evaluation
            var result = thread.Runtime.ExecuteBinaryOperator(this.Op, arg1, arg2, ref _lastUsed);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Defaults the evaluate implementation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected async Task<object> DefaultEvaluateImplementation(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var arg1 = await Left.Evaluate(thread);
            var arg2 = await Right.Evaluate(thread);
            var result = thread.Runtime.ExecuteBinaryOperator(this.Op, arg1, arg2, ref _lastUsed);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Evaluates the constant.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> EvaluateConst(ScriptThread thread)
        {
            return Task.FromResult<object>(_constValue);
        }

        /// <summary>
        /// Determines whether this instance is constant.
        /// </summary>
        /// <returns><c>true</c> if this instance is constant; otherwise, <c>false</c>.</returns>
        public override bool IsConstant()
        {
            if (_isConstant) return true;
            _isConstant = Left.IsConstant() && Right.IsConstant();
            return _isConstant;
            /// <summary>
            /// The _is constant
            /// </summary>
        }
        bool _isConstant;
    }
}
