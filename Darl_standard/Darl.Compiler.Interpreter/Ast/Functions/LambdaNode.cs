// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LambdaNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    //A node representing an anonymous function
    /// <summary>
    /// Class LambdaNode.
    /// </summary>
    public class LambdaNode : AstNode
    {
        /// <summary>
        /// The parameters
        /// </summary>
        public AstNode Parameters;
        /// <summary>
        /// The body
        /// </summary>
        public AstNode Body;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaNode"/> class.
        /// </summary>
        public LambdaNode() { }

        //Used by FunctionDefNode
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaNode"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="node">The node.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="body">The body.</param>
        public LambdaNode(AstContext context, ParseTreeNode node, ParseTreeNode parameters, ParseTreeNode body)
        {
            InitImpl(context, node, parameters, body);
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parseNode">The parse node.</param>
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var mappedNodes = parseNode.GetMappedChildNodes();
            InitImpl(context, parseNode, mappedNodes[0], mappedNodes[1]);
        }

        /// <summary>
        /// Initializes the implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parseNode">The parse node.</param>
        /// <param name="parametersNode">The parameters node.</param>
        /// <param name="bodyNode">The body node.</param>
        private void InitImpl(AstContext context, ParseTreeNode parseNode, ParseTreeNode parametersNode, ParseTreeNode bodyNode)
        {
            base.Init(context, parseNode);
            Parameters = AddChild("Parameters", parametersNode);
            Body = AddChild("Body", bodyNode);
            AsString = "Lambda[" + Parameters.ChildNodes.Count + "]";
            Body.SetIsTail(); //this will be propagated to the last statement
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            DependentScopeInfo = null;
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
            lock (LockObject)
            {
                if (DependentScopeInfo == null)
                {
                    var langCaseSensitive = thread.App.Language.Grammar.CaseSensitive;
                    DependentScopeInfo = new ScopeInfo(this, langCaseSensitive);
                }
                // In the first evaluation the parameter list will add parameter's SlotInfo objects to Scope.ScopeInfo
                thread.PushScope(DependentScopeInfo, null);
                Parameters.Evaluate(thread);
                thread.PopScope();
                //Set Evaluate method and invoke it later
                this.Evaluate = EvaluateAfter;
            }
            var result = Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Evaluates the after.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateAfter(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var closure = new Closure(thread.CurrentScope, this);
            thread.CurrentNode = Parent; //standard epilogue
            return closure;
        }

        /// <summary>
        /// Calls the specified creator scope.
        /// </summary>
        /// <param name="creatorScope">The creator scope.</param>
        /// <param name="thread">The thread.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public object Call(Scope creatorScope, ScriptThread thread, object[] parameters)
        {
            var save = thread.CurrentNode; //prologue, not standard - the caller is NOT target node's parent
            thread.CurrentNode = this;
            thread.PushClosureScope(DependentScopeInfo, creatorScope, parameters);
            Parameters.Evaluate(thread); // pre-process parameters
            var result = Body.Evaluate(thread);
            thread.PopScope();
            thread.CurrentNode = save; //epilogue, restoring caller 
            return result;
        }


        /// <summary>
        /// Sets a flag indicating that the node is in tail position. The value is propagated from parent to children.
        /// Should propagate this call to appropriate children.
        /// </summary>
        public override void SetIsTail()
        {
            //ignore this call, do not mark this node as tail, it is meaningless
        }
    }

}
