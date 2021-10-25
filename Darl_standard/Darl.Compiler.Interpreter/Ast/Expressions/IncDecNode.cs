// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="IncDecNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq.Expressions;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class IncDecNode.
    /// </summary>
    public class IncDecNode : AstNode
    {
        /// <summary>
        /// The is postfix
        /// </summary>
        public bool IsPostfix;
        /// <summary>
        /// The op symbol
        /// </summary>
        public string OpSymbol;
        /// <summary>
        /// The binary op symbol
        /// </summary>
        public string BinaryOpSymbol; //corresponding binary operation: + for ++, - for --
        /// <summary>
        /// The binary op
        /// </summary>
        public ExpressionType BinaryOp;
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
            FindOpAndDetectPostfix(nodes);
            int argIndex = IsPostfix ? 0 : 1;
            Argument = AddChild(NodeUseType.ValueReadWrite, "Arg", nodes[argIndex]);
            BinaryOpSymbol = OpSymbol[0].ToString(); //take a single char out of ++ or --
            var interpContext = (InterpreterAstContext)context;
            BinaryOp = interpContext.OperatorHandler.GetOperatorExpressionType(BinaryOpSymbol);
            base.AsString = OpSymbol + (IsPostfix ? "(postfix)" : "(prefix)");
        }

        /// <summary>
        /// Finds the op and detect postfix.
        /// </summary>
        /// <param name="mappedNodes">The mapped nodes.</param>
        private void FindOpAndDetectPostfix(ParseTreeNodeList mappedNodes)
        {
            IsPostfix = false; //assume it 
            OpSymbol = mappedNodes[0].FindTokenAndGetText();
            if (OpSymbol == "--" || OpSymbol == "++") return;
            IsPostfix = true;
            OpSymbol = mappedNodes[1].FindTokenAndGetText();
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var oldValue = await Argument.Evaluate(thread);
            var newValue = thread.Runtime.ExecuteBinaryOperator(BinaryOp, oldValue, 1, ref _lastUsed);
            Argument.SetValue(thread, newValue);
            var result = IsPostfix ? oldValue : newValue;
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
