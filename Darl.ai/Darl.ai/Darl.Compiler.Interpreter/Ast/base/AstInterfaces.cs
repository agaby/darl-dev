/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AstInterfaces.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq.Expressions;

namespace DarlCompiler.Interpreter.Ast
{

    //This interface is expected by Darl's Grammar Explorer. 
    /// Interface ICallTarget
    /// </summary>
    public interface ICallTarget
    {
        /// Calls the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        object Call(ScriptThread thread, object[] parameters);
    }

    //Simple visitor interface
    /// Interface IAstVisitor
    /// </summary>
    public interface IAstVisitor
    {
        /// Begins the visit.
        /// </summary>
        /// <param name="node">The node.</param>
        void BeginVisit(IVisitableNode node);
        /// Ends the visit.
        /// </summary>
        /// <param name="node">The node.</param>
        void EndVisit(IVisitableNode node);
    }

    /// Interface IVisitableNode
    /// </summary>
    public interface IVisitableNode
    {
        /// Accepts the visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void AcceptVisitor(IAstVisitor visitor);
    }

    /// Interface IOperatorHelper
    /// </summary>
    public interface IOperatorHelper
    {
        /// Gets the type of the operator expression.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>ExpressionType.</returns>
        ExpressionType GetOperatorExpressionType(string symbol);
        /// Gets the type of the unary operator expression.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>ExpressionType.</returns>
        ExpressionType GetUnaryOperatorExpressionType(string symbol);

    }
}
