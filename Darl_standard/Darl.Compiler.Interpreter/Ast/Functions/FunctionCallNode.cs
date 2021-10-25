// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="FunctionCallNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Ast
{

    //A node representing function call. Also handles Special Forms
    /// <summary>
    /// Class FunctionCallNode.
    /// </summary>
    public class FunctionCallNode : AstNode
    {
        /// <summary>
        /// The target reference
        /// </summary>
        AstNode TargetRef;
        /// <summary>
        /// The arguments
        /// </summary>
        AstNode Arguments;
        /// <summary>
        /// The _target name
        /// </summary>
        string _targetName;
        /// <summary>
        /// The _special form
        /// </summary>
        SpecialForm _specialForm;
        /// <summary>
        /// The _special form arguments
        /// </summary>
        AstNode[] _specialFormArgs;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            TargetRef = AddChild("Target", nodes[0]);
            TargetRef.UseType = NodeUseType.CallTarget;
            _targetName = nodes[0].FindTokenAndGetText();
            Arguments = AddChild("Args", nodes[1]);
            AsString = "Call " + _targetName;
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            SetupEvaluateMethod(thread);
            var result = Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Setups the evaluate method.
        /// </summary>
        /// <param name="thread">The thread.</param>
        private void SetupEvaluateMethod(ScriptThread thread)
        {
            var languageTailRecursive = thread.Runtime.Language.Grammar.LanguageFlags.IsSet(LanguageFlags.TailRecursive);
            lock (this.LockObject)
            {
                var target = TargetRef.Evaluate(thread);
                if (target is SpecialForm)
                {
                    _specialForm = target as SpecialForm;
                    _specialFormArgs = Arguments.ChildNodes.ToArray();
                    this.Evaluate = EvaluateSpecialForm;
                }
                else
                {
                    if (languageTailRecursive)
                    {
                        var isTail = Flags.IsSet(AstNodeFlags.IsTail);
                        if (isTail)
                            this.Evaluate = EvaluateTail;
                        else
                            this.Evaluate = EvaluateWithTailCheck;
                    }
                    else
                        this.Evaluate = EvaluateNoTail;
                }
            }//lock 
        }

        // Evaluation for special forms
        /// <summary>
        /// Evaluates the special form.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateSpecialForm(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var result = _specialForm(thread, _specialFormArgs);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }


        // Evaluation for non-tail languages
        /// <summary>
        /// Evaluates the no tail.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateNoTail(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var target = TargetRef.Evaluate(thread);
            var iCall = target as ICallTarget;
            if (iCall == null)
                thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
            var args = (object[])Arguments.Evaluate(thread);
            object result = iCall.Call(thread, args);
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        //Evaluation for tailed languages
        /// <summary>
        /// Evaluates the tail.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateTail(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var target = TargetRef.Evaluate(thread);
            var iCall = target as ICallTarget;
            if (iCall == null)
                thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
            var args = (object[])Arguments.Evaluate(thread);
            thread.Tail = iCall;
            thread.TailArgs = args;
            thread.CurrentNode = Parent; //standard epilogue
            return null;
        }

        /// <summary>
        /// Evaluates the with tail check.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object EvaluateWithTailCheck(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var target = TargetRef.Evaluate(thread);
            var iCall = target as ICallTarget;
            if (iCall == null)
                thread.ThrowScriptError(Resources.ErrVarIsNotCallable, _targetName);
            var args = (object[])Arguments.Evaluate(thread);
            object result = null;
            result = iCall.Call(thread, args);
            //Note that after invoking tail we can get another tail. 
            // So we need to keep calling tails while they are there.
            while (thread.Tail != null)
            {
                var tail = thread.Tail;
                var tailArgs = thread.TailArgs;
                thread.Tail = null;
                thread.TailArgs = null;
                result = tail.Call(thread, tailArgs);
            }
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

    }

}
