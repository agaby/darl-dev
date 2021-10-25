// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="BuiltInObjectBinding.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using DarlCompiler.Interpreter.Ast;

namespace DarlCompiler.Interpreter
{
    // A general delegate representing a built-in method implementation. 
    /// <summary>
    /// Delegate BuiltInMethod
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>System.Object.</returns>
    public delegate object BuiltInMethod(ScriptThread thread, object[] args);

    //A wrapper to convert BuiltInMethod delegate (referencing some custom method in LanguageRuntime) into an ICallTarget instance (expected by FunctionCallNode)
    /// <summary>
    /// Class BuiltInCallTarget.
    /// </summary>
    public class BuiltInCallTarget : ICallTarget
    {
        /// <summary>
        /// The name
        /// </summary>
        public string Name;
        /// <summary>
        /// The method
        /// </summary>
        public readonly BuiltInMethod Method;
        /// <summary>
        /// The minimum parameter count
        /// </summary>
        public readonly int MinParamCount, MaxParamCount;
        /// <summary>
        /// The parameter names
        /// </summary>
        public string[] ParameterNames; //Just for information purpose
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInCallTarget"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="name">The name.</param>
        /// <param name="minParamCount">The minimum parameter count.</param>
        /// <param name="maxParamCount">The maximum parameter count.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public BuiltInCallTarget(BuiltInMethod method, string name, int minParamCount = 0, int maxParamCount = 0, string parameterNames = null)
        {
            Method = method;
            Name = name;
            MinParamCount = minParamCount;
            MaxParamCount = Math.Max(MinParamCount, maxParamCount);
            if (!string.IsNullOrEmpty(parameterNames))
                ParameterNames = parameterNames.Split(',');
        }

        #region ICallTarget Members
        /// <summary>
        /// Calls the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public object Call(ScriptThread thread, object[] parameters)
        {
            return Method(thread, parameters);
        }
        #endregion
    }

    // The class contains information about built-in function. It has double purpose. 
    // First, it is used as a BindingTargetInfo instance (meta-data) for a binding to a built-in function. 
    // Second, we use it as a reference to a custom built-in method that we store in LanguageRuntime.BuiltIns table. 
    // For this, we make it implement IBindingSource - we can add it to BuiltIns table of LanguageRuntime, which is a table of IBindingSource instances.
    // Being IBindingSource, it can produce a binding object to the target method - singleton in fact; 
    // the same binding object is used for all calls to the method from all function-call AST nodes. 
    /// <summary>
    /// Class BuiltInCallableTargetInfo.
    /// </summary>
    public class BuiltInCallableTargetInfo : BindingTargetInfo, IBindingSource
    {
        /// <summary>
        /// The binding instance
        /// </summary>
        public Binding BindingInstance; //A singleton binding instance; we share it for all AST nodes (function call nodes) that call the method. 

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInCallableTargetInfo"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="minParamCount">The minimum parameter count.</param>
        /// <param name="maxParamCount">The maximum parameter count.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public BuiltInCallableTargetInfo(BuiltInMethod method, string methodName, int minParamCount = 0, int maxParamCount = 0, string parameterNames = null) :
            this(new BuiltInCallTarget(method, methodName, minParamCount, maxParamCount, parameterNames))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInCallableTargetInfo"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public BuiltInCallableTargetInfo(BuiltInCallTarget target)
            : base(target.Name, BindingTargetType.BuiltInObject)
        {
            BindingInstance = new ConstantBinding(target, this);
        }

        //Implement IBindingSource.Bind
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(BindingRequest request)
        {
            return BindingInstance;
        }

    }

    // Method for adding methods to BuiltIns table in Runtime
    /// <summary>
    /// Class BindingSourceTableExtensions.
    /// </summary>
    public static partial class BindingSourceTableExtensions
    {
        /// <summary>
        /// Adds the method.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="method">The method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="minParamCount">The minimum parameter count.</param>
        /// <param name="maxParamCount">The maximum parameter count.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <returns>BindingTargetInfo.</returns>
        public static BindingTargetInfo AddMethod(this BindingSourceTable targets, BuiltInMethod method, string methodName,
              int minParamCount = 0, int maxParamCount = 0, string parameterNames = null)
        {
            var callTarget = new BuiltInCallTarget(method, methodName, minParamCount, maxParamCount, parameterNames);
            var targetInfo = new BuiltInCallableTargetInfo(callTarget);
            targets.Add(methodName, targetInfo);
            return targetInfo;
        }
    }

} 
