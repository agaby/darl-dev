/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="BindingRequest.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
using System;

namespace DarlCompiler.Interpreter
{

    /// Enum BindingRequestFlags
    /// </summary>
    [Flags]
    public enum BindingRequestFlags
    {
        /// The read
        /// </summary>
        Read = 0x01,
        /// The write
        /// </summary>
        Write = 0x02,
        /// The invoke
        /// </summary>
        Invoke = 0x04,
        /// The existing or new
        /// </summary>
        ExistingOrNew = 0x10,
        /// The new only
        /// </summary>
        NewOnly = 0x20,  // for new variable, for ex, in JavaScript "var x..." - introduces x as new variable
    }

    //Binding request is a container for information about requested binding. Binding request goes from an Ast node to language runtime. 
    // For example, identifier node would request a binding for an identifier. 
    /// Class BindingRequest.
    /// </summary>
    public class BindingRequest
    {
        /// The thread
        /// </summary>
        public ScriptThread Thread;
        /// From node
        /// </summary>
        public AstNode FromNode;
        /// From module
        /// </summary>
        public ModuleInfo FromModule;
        /// The flags
        /// </summary>
        public BindingRequestFlags Flags;
        /// The symbol
        /// </summary>
        public string Symbol;
        /// From scope information
        /// </summary>
        public ScopeInfo FromScopeInfo;
        /// The ignore case
        /// </summary>
        public bool IgnoreCase;
        /// Initializes a new instance of the <see cref="BindingRequest"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="fromNode">From node.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="flags">The flags.</param>
        public BindingRequest(ScriptThread thread, AstNode fromNode, string symbol, BindingRequestFlags flags)
        {
            Thread = thread;
            FromNode = fromNode;
            FromModule = thread.App.DataMap.GetModule(fromNode.ModuleNode);
            Symbol = symbol;
            Flags = flags;
            FromScopeInfo = thread.CurrentScope.Info;
            IgnoreCase = !thread.Runtime.Language.Grammar.CaseSensitive;
        }
    }

}
