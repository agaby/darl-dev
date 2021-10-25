// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParamListNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class ParamListNode.
    /// </summary>
    public class ParamListNode : AstNode
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
                AddChild(NodeUseType.Parameter, "param", child);
            AsString = "param_list[" + ChildNodes.Count + "]";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            // Is called once, at first evaluation of FunctionDefNode
            // Creates parameter slots
            foreach (var child in this.ChildNodes)
            {
                var idNode = child as IdentifierNode;
                if (idNode != null)
                {
                    thread.CurrentScope.Info.AddSlot(idNode.Symbol, SlotType.Parameter);
                }
            }
            this.Evaluate = EvaluateAfter;
            thread.CurrentNode = Parent; //standard epilogue
            return null;
        }

        /// <summary>
        /// Evaluates the after.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateAfter(ScriptThread thread)
        {
            return null;
        }
    }

}
