// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AssignmentNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq.Expressions;

using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{
    /// <summary>
    /// Class AssignmentNode.
    /// </summary>
    public class AssignmentNode : AstNode
    {
        /// <summary>
        /// The target
        /// </summary>
        public AstNode Target;
        /// <summary>
        /// The assignment op
        /// </summary>
        public string AssignmentOp;
        /// <summary>
        /// The is augmented
        /// </summary>
        public bool IsAugmented; // true if it is augmented operation like "+="
        /// <summary>
        /// The binary expression type
        /// </summary>
        public ExpressionType BinaryExpressionType;
        /// <summary>
        /// The expression
        /// </summary>
        public AstNode Expression;
        /// <summary>
        /// The _last used
        /// </summary>
        private OperatorImplementation _lastUsed;
        /// <summary>
        /// The _failure count
        /// </summary>
        private int _failureCount;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Target = AddChild(NodeUseType.ValueWrite, "To", nodes[0]);
            //Get Op and baseOp if it is combined assignment
            AssignmentOp = nodes[1].FindTokenAndGetText();
            if (string.IsNullOrEmpty(AssignmentOp))
                AssignmentOp = "=";
            BinaryExpressionType = CustomExpressionTypes.NotAnExpression;
            //There maybe an "=" sign in the middle, or not - if it is marked as punctuation; so we just take the last node in child list
            Expression = AddChild(NodeUseType.ValueRead, "Expr", nodes[nodes.Count - 1]);
            AsString = AssignmentOp + " (assignment)";
            IsAugmented = AssignmentOp.Length > 1;
            if (IsAugmented)
            {

                var ictxt = context as InterpreterAstContext;
                base.ExpressionType = ictxt.OperatorHandler.GetOperatorExpressionType(AssignmentOp);
                BinaryExpressionType = ictxt.OperatorHandler.GetBinaryOperatorForAugmented(this.ExpressionType);
                Target.UseType = NodeUseType.ValueReadWrite;
            }
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            if (IsAugmented)
                Evaluate = EvaluateAugmentedFast;
            else
                Evaluate = EvaluateSimple; //non-augmented
            //call self-evaluate again, now to call real methods
            var result = this.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }


        /// <summary>
        /// Evaluates the simple.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateSimple(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var value = Expression.Evaluate(thread);
            Target.SetValue(thread, value);
            thread.CurrentNode = Parent; //standard epilogue
            return value;
        }

        /// <summary>
        /// Evaluates the augmented fast.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateAugmentedFast(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var value = Target.Evaluate(thread);
            var exprValue = Expression.Evaluate(thread);
            object result = null;
            if (_lastUsed != null)
            {
                try
                {
                    result = _lastUsed.EvaluateBinary(value, exprValue);
                }
                catch
                {
                    _failureCount++;
                    // if failed 3 times, change to method without direct try
                    if (_failureCount > 3)
                        Evaluate = EvaluateAugmented;
                } //catch
            }// if _lastUsed
            if (result == null)
                result = thread.Runtime.ExecuteBinaryOperator(BinaryExpressionType, value, exprValue, ref _lastUsed);
            Target.SetValue(thread, result);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Evaluates the augmented.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateAugmented(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var value = Target.Evaluate(thread);
            var exprValue = Expression.Evaluate(thread);
            var result = thread.Runtime.ExecuteBinaryOperator(BinaryExpressionType, value, exprValue, ref _lastUsed);
            Target.SetValue(thread, result);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }


    }
}
