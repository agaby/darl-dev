// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LanguageRuntime_OpDispatch.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq.Expressions;
using Darl_standard;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Class LanguageRuntime.
    /// </summary>
    public partial class LanguageRuntime
    {
        /// <summary>
        /// The operator implementations
        /// </summary>
        public readonly OperatorImplementationTable OperatorImplementations = new OperatorImplementationTable(2000);


        /// <summary>
        /// Executes the binary operator.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="previousUsed">The previous used.</param>
        /// <returns>System.Object.</returns>
        public object ExecuteBinaryOperator(ExpressionType op, object arg1, object arg2, ref OperatorImplementation previousUsed)
        {
            // 1. Get arg types
            Type arg1Type, arg2Type;
            try
            {
                arg1Type = arg1.GetType();
                arg2Type = arg2.GetType();
            }
            catch (NullReferenceException)
            {
                // arg1 or arg2 is null - which means never assigned.
                CheckUnassigned(arg1);
                CheckUnassigned(arg2);
                throw;
            }

            // 2. If we had prev impl, check if current args types match it; first copy it into local variable
            // Note: BinaryExpression node might already have tried it directly, without any checks, and 
            // apparently failed. At some point this attempt in BinaryExpressionNode can become disabled.
            // But we might still try it here, with proper checks
            var currentImpl = previousUsed;
            if (currentImpl != null && (arg1Type != currentImpl.Key.Arg1Type || arg2Type != currentImpl.Key.Arg2Type))
                currentImpl = null;

            // 3. Find implementation for arg types
            OperatorDispatchKey key;
            if (currentImpl == null)
            {
                key = new OperatorDispatchKey(op, arg1Type, arg2Type);
                if (!OperatorImplementations.TryGetValue(key, out currentImpl))
                    ThrowScriptError(Resources.ErrOpNotDefinedForTypes, op, arg1Type, arg2Type);
            }

            // 4. Actually call 
            try
            {
                previousUsed = currentImpl;
                return currentImpl.EvaluateBinary(arg1, arg2);
            }
            catch (OverflowException)
            {
                if (currentImpl.OverflowHandler == null) throw;
                previousUsed = currentImpl.OverflowHandler; //set previousUsed to overflowHandler, so it will be used next time
                return ExecuteBinaryOperator(op, arg1, arg2, ref previousUsed); //call self recursively
            }
            catch (IndexOutOfRangeException)
            {
                //We can get here only if we use SmartBoxing - the result is out of range of pre-allocated boxes, 
                // so attempt to lookup a boxed value in _boxes dictionary fails with outOfRange exc
                if (currentImpl.NoBoxImplementation == null) throw;
                // If NoBoxImpl is not null, then it is implementation with auto-boxing. 
                // Auto-boxing failed - the result is outside the range of our boxes array. Let's call no-box version.
                // we also set previousUsed to no-box implementation, so we use it in the future calls
                previousUsed = currentImpl.NoBoxImplementation;
                return ExecuteBinaryOperator(op, arg1, arg2, ref previousUsed); //call self recursively
            }

        }

        /// <summary>
        /// Executes the unary operator.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="previousUsed">The previous used.</param>
        /// <returns>System.Object.</returns>
        public object ExecuteUnaryOperator(ExpressionType op, object arg1, ref OperatorImplementation previousUsed)
        {
            // 1. Get arg type
            Type arg1Type;
            try
            {
                arg1Type = arg1.GetType();
            }
            catch (NullReferenceException)
            {
                CheckUnassigned(arg1);
                throw;
            }

            // 2. If we had prev impl, check if current args types match it; first copy it into local variable
            OperatorDispatchKey key;
            var currentImpl = previousUsed;
            if (currentImpl != null && arg1Type != currentImpl.Key.Arg1Type)
                currentImpl = null;

            // 3. Find implementation for arg type
            if (currentImpl == null)
            {
                key = new OperatorDispatchKey(op, arg1Type);
                if (!OperatorImplementations.TryGetValue(key, out currentImpl))
                    ThrowError(Resources.ErrOpNotDefinedForType, op, arg1Type);
            }

            // 4. Actually call 
            try
            {
                previousUsed = currentImpl; //set previousUsed so next time we'll try this impl first
                return currentImpl.Arg1Converter(arg1);
            }
            catch (OverflowException)
            {
                if (currentImpl.OverflowHandler == null)
                    throw;
                previousUsed = currentImpl.OverflowHandler; //set previousUsed to overflowHandler, so it will be used next time
                return ExecuteUnaryOperator(op, arg1, ref previousUsed); //call self recursively
            }
        }


        /// <summary>
        /// Checks the unassigned.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">Variable unassigned.</exception>
        private void CheckUnassigned(object value)
        {
            if (value == null)
                throw new Exception("Variable unassigned.");
        }

    }
}
