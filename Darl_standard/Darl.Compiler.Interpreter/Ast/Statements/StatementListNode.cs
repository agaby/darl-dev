// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="StatementListNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class StatementListNode.
    /// </summary>
    public class StatementListNode : AstNode
    {
        /// <summary>
        /// The _single child
        /// </summary>
        AstNode _singleChild; //stores a single child when child count == 1, for fast access

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                //don't add if it is null; it can happen that "statement" is a comment line and statement's node is null.
                // So to make life easier for language creator, we just skip if it is null
                if (child.AstNode != null)
                    AddChild(string.Empty, child);
            }
            AsString = "Statement List";
            if (ChildNodes.Count == 0)
            {
                AsString += " (Empty)";
            }
            else
                ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            lock (LockObject)
            {
                switch (ChildNodes.Count)
                {
                    case 0:
                        Evaluate = EvaluateEmpty;
                        break;
                    case 1:
                        _singleChild = ChildNodes[0];
                        Evaluate = EvaluateOne;
                        break;
                    default:
                        Evaluate = EvaluateMultiple;
                        break;
                }//switch
            }//lock
            var result = Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Evaluates the empty.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateEmpty(ScriptThread thread)
        {
            return null;
        }

        /// <summary>
        /// Evaluates the one.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateOne(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            object result = _singleChild.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Evaluates the multiple.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateMultiple(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            object result = null;
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                result = ChildNodes[i].Evaluate(thread);
            }
            thread.CurrentNode = Parent; //standard epilogue
            return result; //return result of last statement
        }

        /// <summary>
        /// Sets a flag indicating that the node is in tail position. The value is propagated from parent to children.
        /// Should propagate this call to appropriate children.
        /// </summary>
        public override void SetIsTail()
        {
            base.SetIsTail();
            if (ChildNodes.Count > 0)
                ChildNodes[ChildNodes.Count - 1].SetIsTail();
        }


    }

}
