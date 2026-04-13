/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScriptThread.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.Thinkbase.Meta;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DarlCompiler.Interpreter
{
    /// Represents a running thread in script application.
    /// </summary>
    public sealed class ScriptThread : IBindingSource
    {
        /// The application
        /// </summary>
        public readonly ScriptApp App;
        /// The runtime
        /// </summary>
        public readonly LanguageRuntime Runtime;

        /// The current scope
        /// </summary>
        public Scope CurrentScope;
        /// The current node
        /// </summary>
        public AstNode CurrentNode;

        // Tail call parameters
        /// The tail
        /// </summary>
        public ICallTarget Tail;
        /// The tail arguments
        /// </summary>
        public object[] TailArgs;

        public Stack<(DarlMetaNode, double)> ExecutionStack = new();
        public int currentExecutionStackDepth { get; set; } = 0;

        /// Initializes a new instance of the <see cref="ScriptThread"/> class.
        /// </summary>
        /// <param name="app">The application.</param>
        public ScriptThread(ScriptApp app)
        {
            App = app;
            Runtime = App.Runtime;
            CurrentScope = app.MainScope;
        }



        /// Pushes the scope.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        /// <param name="parameters">The parameters.</param>
        public void PushScope(ScopeInfo scopeInfo, object[] parameters)
        {
            CurrentScope = new Scope(scopeInfo, CurrentScope, CurrentScope, parameters);
        }



        /// Pushes the closure scope.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        /// <param name="closureParent">The closure parent.</param>
        /// <param name="parameters">The parameters.</param>
        public void PushClosureScope(ScopeInfo scopeInfo, Scope closureParent, object[] parameters)
        {
            CurrentScope = new Scope(scopeInfo, CurrentScope, closureParent, parameters);
        }

        /// Pops the scope.
        /// </summary>
        public void PopScope()
        {
            CurrentScope = CurrentScope.Caller;
        }

        /// Binds the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="options">The options.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(string symbol, BindingRequestFlags options)
        {
            var request = new BindingRequest(this, CurrentNode, symbol, options);
            var binding = Bind(request);
            if (binding == null)
                ThrowScriptError("Unknown symbol '{0}'.", symbol);
            return binding;
        }

        public void CheckpointExecution()
        {
            currentExecutionStackDepth = ExecutionStack.Count;
        }

        public void RecordExecution(DarlResult res, DarlMetaNode node)
        {
            if(!res.IsUnknown())
            {
                ExecutionStack.Push((node, res.GetWeight()));
            }
            else
            {
                while(ExecutionStack.Count > currentExecutionStackDepth)
                    ExecutionStack.Pop();
            }
        }

        #region Exception handling
        /// Handles the error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.Object.</returns>
        public object HandleError(Exception exception)
        {
            if (exception is ScriptException)
                throw exception;
            var stack = GetStackTrace();
            var rex = new ScriptException(exception.Message, exception, CurrentNode.ErrorAnchor, stack);
            throw rex;
        }

        // Throws ScriptException exception.
        /// Throws the script error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="DarlCompiler.Interpreter.ScriptException">null</exception>
        public void ThrowScriptError(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            var loc = GetCurrentLocation();
            var stack = GetStackTrace();
            throw new ScriptException(message, null, loc, stack);
        }

        /// Gets the stack trace.
        /// </summary>
        /// <returns>ScriptStackTrace.</returns>
        public ScriptStackTrace GetStackTrace()
        {
            return new ScriptStackTrace();
        }

        /// Gets the current location.
        /// </summary>
        /// <returns>SourceLocation.</returns>
        private SourceLocation GetCurrentLocation()
        {
            return this.CurrentNode == null ? new SourceLocation() : CurrentNode.Location;
        }

        #endregion


        #region IBindingSource Members

        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(BindingRequest request)
        {
            return Runtime.Bind(request);
        }

        #endregion

        

    }
}
