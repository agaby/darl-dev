/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SlotBinding.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter
{

    // Implements fast access to a variable (local/global var or parameter) in local scope or in any enclosing scope
    // Important: the following code is very sensitive to even tiny changes - do not know exactly particular reasons. 
    /// <summary>
    /// Class SlotBinding. This class cannot be inherited.
    /// </summary>
    public sealed class SlotBinding : Binding
    {
        /// <summary>
        /// The slot
        /// </summary>
        public SlotInfo Slot;
        /// <summary>
        /// From scope
        /// </summary>
        public ScopeInfo FromScope;
        /// <summary>
        /// The slot index
        /// </summary>
        public int SlotIndex;
        /// <summary>
        /// The static scope index
        /// </summary>
        public int StaticScopeIndex;
        /// <summary>
        /// From node
        /// </summary>
        public AstNode FromNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotBinding"/> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="fromNode">From node.</param>
        /// <param name="fromScope">From scope.</param>
        public SlotBinding(SlotInfo slot, AstNode fromNode, ScopeInfo fromScope)
            : base(slot.Name, BindingTargetType.Slot)
        {
            Slot = slot;
            FromNode = fromNode;
            FromScope = fromScope;
            SlotIndex = slot.Index;
            StaticScopeIndex = Slot.ScopeInfo.StaticIndex;
            SetupAccessorMethods();
        }

        /// <summary>
        /// Setups the accessor methods.
        /// </summary>
        private void SetupAccessorMethods()
        {
            // Check module scope
            if (Slot.ScopeInfo.StaticIndex >= 0)
            {
                GetValueRef = FastGetStaticValue;
                SetValueRef = SetStatic;
                return;
            }
            var levelDiff = Slot.ScopeInfo.Level - FromScope.Level;
            switch (levelDiff)
            {
                case 0: // local scope 
                    if (Slot.Type == SlotType.Value)
                    {
                        base.GetValueRef = FastGetCurrentScopeValue;
                        base.SetValueRef = SetCurrentScopeValue;
                    }
                    else
                    {
                        base.GetValueRef = FastGetCurrentScopeParameter;
                        base.SetValueRef = SetCurrentScopeParameter;
                    }
                    return;
                case 1: //direct parent
                    if (Slot.Type == SlotType.Value)
                    {
                        base.GetValueRef = GetImmediateParentScopeValue;
                        base.SetValueRef = SetImmediateParentScopeValue;
                    }
                    else
                    {
                        base.GetValueRef = GetImmediateParentScopeParameter;
                        base.SetValueRef = SetImmediateParentScopeParameter;
                    }
                    return;
                default: // some enclosing scope
                    if (Slot.Type == SlotType.Value)
                    {
                        base.GetValueRef = GetParentScopeValue;
                        base.SetValueRef = SetParentScopeValue;
                    }
                    else
                    {
                        base.GetValueRef = GetParentScopeParameter;
                        base.SetValueRef = SetParentScopeParameter;
                    }
                    return;
            }
        }

        // Specific method implementations =======================================================================================================
        // Optimization: in most cases we go directly for Values array; if we fail, then we fallback to full method 
        // with proper exception handling. This fallback is expected to be extremely rare, so overall we have considerable perf gain
        // Note that in we expect the methods to be used directly by identifier node (like: IdentifierNode.EvaluateRef = Binding.GetValueRef; } - 
        // to save a few processor cycles. Therefore, we need to provide a proper context (thread.CurrentNode) in case of exception. 
        // In all "full-method" implementations we set current node to FromNode, so exception correctly points 
        // to the owner Identifier node as a location of error. 

        // Current scope
        /// <summary>
        /// Fasts the get current scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> FastGetCurrentScopeValue(ScriptThread thread)
        {
            try
            {
                //optimization: we go directly for values array; if we fail, then we fallback to regular "proper" method.
                return Task.FromResult<object>(thread.CurrentScope.Values[SlotIndex]);
            }
            catch
            {
                return Task.FromResult<object>(GetCurrentScopeValue(thread));
            }
        }

        /// <summary>
        /// Gets the current scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetCurrentScopeValue(ScriptThread thread)
        {
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.GetValue(SlotIndex));
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }


        /// <summary>
        /// Fasts the get current scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> FastGetCurrentScopeParameter(ScriptThread thread)
        {
            //optimization: we go directly for parameters array; if we fail, then we fallback to regular "proper" method.
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.Parameters[SlotIndex]);
            }
            catch
            {
                return Task.FromResult<object>(GetCurrentScopeParameter(thread));
            }
        }
        /// <summary>
        /// Gets the current scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetCurrentScopeParameter(ScriptThread thread)
        {
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.GetParameter(SlotIndex));
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }

        /// <summary>
        /// Sets the current scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetCurrentScopeValue(ScriptThread thread, object value)
        {
            thread.CurrentScope.SetValue(SlotIndex, value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the current scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetCurrentScopeParameter(ScriptThread thread, object value)
        {
            thread.CurrentScope.SetParameter(SlotIndex, value);
            return Task.CompletedTask;
        }

        // Static scope (module-level variables)
        /// <summary>
        /// Fasts the get static value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> FastGetStaticValue(ScriptThread thread)
        {
            try
            {
                return Task.FromResult<object>(thread.App.StaticScopes[StaticScopeIndex].Values[SlotIndex]);
            }
            catch
            {
                return Task.FromResult<object>(GetStaticValue(thread));
            }
        }
        /// <summary>
        /// Gets the static value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object GetStaticValue(ScriptThread thread)
        {
            try
            {
                return thread.App.StaticScopes[StaticScopeIndex].GetValue(SlotIndex);
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }


        /// <summary>
        /// Sets the static.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetStatic(ScriptThread thread, object value)
        {
            thread.App.StaticScopes[StaticScopeIndex].SetValue(SlotIndex, value);
            return Task.CompletedTask;
        }

        // Direct parent
        /// <summary>
        /// Gets the immediate parent scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetImmediateParentScopeValue(ScriptThread thread)
        {
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.Parent.Values[SlotIndex]);
            }
            catch { }
            //full method
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.Parent.GetValue(SlotIndex));
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }

        /// <summary>
        /// Gets the immediate parent scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetImmediateParentScopeParameter(ScriptThread thread)
        {
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.Parent.Parameters[SlotIndex]);
            }
            catch { }
            //full method
            try
            {
                return Task.FromResult<object>(thread.CurrentScope.Parent.GetParameter(SlotIndex));
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }

        /// <summary>
        /// Sets the immediate parent scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetImmediateParentScopeValue(ScriptThread thread, object value)
        {
            thread.CurrentScope.Parent.SetValue(SlotIndex, value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the immediate parent scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetImmediateParentScopeParameter(ScriptThread thread, object value)
        {
            thread.CurrentScope.Parent.SetParameter(SlotIndex, value);
            return Task.CompletedTask;
        }

        // Generic case
        /// <summary>
        /// Gets the parent scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetParentScopeValue(ScriptThread thread)
        {
            var targetScope = GetTargetScope(thread);
            return Task.FromResult<object>(targetScope.GetValue(SlotIndex));
        }
        /// <summary>
        /// Gets the parent scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private Task<object> GetParentScopeParameter(ScriptThread thread)
        {
            var targetScope = GetTargetScope(thread);
            return Task.FromResult<object>(targetScope.GetParameter(SlotIndex));
        }
        /// <summary>
        /// Sets the parent scope value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetParentScopeValue(ScriptThread thread, object value)
        {
            var targetScope = GetTargetScope(thread);
            targetScope.SetValue(SlotIndex, value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the parent scope parameter.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private Task SetParentScopeParameter(ScriptThread thread, object value)
        {
            var targetScope = GetTargetScope(thread);
            targetScope.SetParameter(SlotIndex, value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the target scope.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>Scope.</returns>
        private Scope GetTargetScope(ScriptThread thread)
        {
            var targetLevel = Slot.ScopeInfo.Level;
            var scope = thread.CurrentScope.Parent;
            while (scope.Info.Level > targetLevel)
                scope = scope.Parent;
            return scope;
        }


    }

}

