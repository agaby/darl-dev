// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="FunctionDefNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    //A node representing function definition (named lambda)
    /// <summary>
    /// Class FunctionDefNode.
    /// </summary>
    public class FunctionDefNode : AstNode
    {
        /// <summary>
        /// The name node
        /// </summary>
        public AstNode NameNode;
        /// <summary>
        /// The lambda
        /// </summary>
        public LambdaNode Lambda;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            //child #0 is usually a keyword like "def"
            var nodes = treeNode.GetMappedChildNodes();
            NameNode = AddChild("Name", nodes[1]);
            Lambda = new LambdaNode(context, treeNode, nodes[2], nodes[3]); //node, params, body
            Lambda.Parent = this;
            AsString = "<Function " + NameNode.AsString + ">";
            //Lambda will set treeNode.AstNode to itself, we need to set it back to "this" here
            treeNode.AstNode = this; //
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            DependentScopeInfo = null;
            Lambda.Reset();
            base.Reset();
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var closure = Lambda.Evaluate(thread); //returns closure
            NameNode.SetValue(thread, closure);
            thread.CurrentNode = Parent; //standard epilogue
            return closure;
        }

    }

}
