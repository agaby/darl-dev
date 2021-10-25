// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="IfNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{
    /// <summary>
    /// Class IfNode.
    /// </summary>
    public class IfNode : AstNode
    {
        /// <summary>
        /// The test
        /// </summary>
        public AstNode Test;
        /// <summary>
        /// If true
        /// </summary>
        public AstNode IfTrue;
        /// <summary>
        /// If false
        /// </summary>
        public AstNode IfFalse;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Test = AddChild("Test", nodes[0]);
            IfTrue = AddChild("IfTrue", nodes[1]);
            if (nodes.Count > 2)
                IfFalse = AddChild("IfFalse", nodes[2]);
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            object result = null;
            var test = Test.Evaluate(thread);
            var isTrue = thread.Runtime.IsTrue(test);
            if (isTrue)
            {
                if (IfTrue != null)
                    result = IfTrue.Evaluate(thread);
            }
            else
            {
                if (IfFalse != null)
                    result = IfFalse.Evaluate(thread);
            }
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
            if (IfTrue != null)
                IfTrue.SetIsTail();
            if (IfFalse != null)
                IfFalse.SetIsTail();
        }

    }

}
