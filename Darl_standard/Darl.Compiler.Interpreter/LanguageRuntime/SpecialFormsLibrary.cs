// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SpecialFormsLibrary.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;

namespace DarlCompiler.Interpreter
{
    /// <summary>
    /// Delegate SpecialForm
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="childNodes">The child nodes.</param>
    /// <returns>System.Object.</returns>
    public delegate object SpecialForm(ScriptThread thread, AstNode[] childNodes);

    /// <summary>
    /// Class SpecialFormsLibrary.
    /// </summary>
    public static class SpecialFormsLibrary
    {
        /// <summary>
        /// Iifs the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="childNodes">The child nodes.</param>
        /// <returns>System.Object.</returns>
        public static object Iif(ScriptThread thread, AstNode[] childNodes)
        {
            var testValue = childNodes[0].Evaluate(thread);
            object result = thread.Runtime.IsTrue(testValue) ? childNodes[1].Evaluate(thread) : childNodes[2].Evaluate(thread);
            return result;

        }
    }
}
