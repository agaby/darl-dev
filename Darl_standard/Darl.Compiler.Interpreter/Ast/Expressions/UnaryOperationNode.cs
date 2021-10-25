// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="UnaryOperationNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class UnaryOperationNode.
    /// </summary>
    public class UnaryOperationNode : AstNode
    {
        /// <summary>
        /// The op symbol
        /// </summary>
        public string OpSymbol;
        /// <summary>
        /// The argument
        /// </summary>
        public AstNode Argument;
        /// <summary>
        /// The _last used
        /// </summary>
        private OperatorImplementation _lastUsed;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            OpSymbol = nodes[0].FindTokenAndGetText();
            Argument = AddChild("Arg", nodes[1]);
            base.AsString = OpSymbol + "(unary op)";
            var interpContext = (InterpreterAstContext)context;
            base.ExpressionType = interpContext.OperatorHandler.GetUnaryOperatorExpressionType(OpSymbol);
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var arg = Argument.Evaluate(thread);
            var result = thread.Runtime.ExecuteUnaryOperator(base.ExpressionType, arg, ref _lastUsed);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Sets a flag indicating that the node is in tail position. The value is propagated from parent to children.
        /// Should propagate this call to appropriate children.
        /// </summary>
        public override void SetIsTail()
        {
            base.SetIsTail();
            Argument.SetIsTail();
        }

    }
}
