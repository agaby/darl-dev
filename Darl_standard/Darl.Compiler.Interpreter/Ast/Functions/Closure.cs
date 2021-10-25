// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="Closure.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace DarlCompiler.Interpreter.Ast
{
    /// <summary>
    /// Class Closure.
    /// </summary>
    public class Closure : ICallTarget
    {
        //The scope that created closure; is used to find Parents (enclosing scopes) 
        /// <summary>
        /// The parent scope
        /// </summary>
        public Scope ParentScope;
        /// <summary>
        /// The lambda
        /// </summary>
        public LambdaNode Lambda;
        /// <summary>
        /// Initializes a new instance of the <see cref="Closure"/> class.
        /// </summary>
        /// <param name="parentScope">The parent scope.</param>
        /// <param name="targetNode">The target node.</param>
        public Closure(Scope parentScope, LambdaNode targetNode)
        {
            ParentScope = parentScope;
            Lambda = targetNode;
        }

        /// <summary>
        /// Calls the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public object Call(ScriptThread thread, object[] parameters)
        {
            return Lambda.Call(ParentScope, thread, parameters);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Lambda.ToString(); //returns nice string like "<function add>"
        }

    } 
}
