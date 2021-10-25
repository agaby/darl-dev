// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ClrInteropBindings.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Reflection;
using DarlCompiler.Interpreter.Ast;

namespace DarlCompiler.Interpreter
{

    //Unfinished, work in progress, file disabled for now

    /// <summary>
    /// Enum ClrTargetType
    /// </summary>
    public enum ClrTargetType
    {
        /// <summary>
        /// The namespace
        /// </summary>
        Namespace,
        /// <summary>
        /// The type
        /// </summary>
        Type,
        /// <summary>
        /// The method
        /// </summary>
        Method,
        /// <summary>
        /// The property
        /// </summary>
        Property,
        /// <summary>
        /// The field
        /// </summary>
        Field,
    }

    /// <summary>
    /// Class ClrInteropBindingTargetInfo.
    /// </summary>
    public class ClrInteropBindingTargetInfo : BindingTargetInfo, IBindingSource
    {
        /// <summary>
        /// The target sub type
        /// </summary>
        public ClrTargetType TargetSubType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrInteropBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="targetSubType">Type of the target sub.</param>
        public ClrInteropBindingTargetInfo(string symbol, ClrTargetType targetSubType)
            : base(symbol, BindingTargetType.ClrInterop)
        {
            TargetSubType = targetSubType;
        }

        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Binding Bind(BindingRequest request)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class ClrNamespaceBindingTargetInfo.
    /// </summary>
    public class ClrNamespaceBindingTargetInfo : ClrInteropBindingTargetInfo
    {
        /// <summary>
        /// The _binding
        /// </summary>
        ConstantBinding _binding;
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrNamespaceBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="ns">The ns.</param>
        public ClrNamespaceBindingTargetInfo(string ns)
            : base(ns, ClrTargetType.Namespace)
        {
            _binding = new ConstantBinding(ns, this);
        }
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public override Binding Bind(BindingRequest request)
        {
            return _binding;
        }
    }

    /// <summary>
    /// Class ClrTypeBindingTargetInfo.
    /// </summary>
    public class ClrTypeBindingTargetInfo : ClrInteropBindingTargetInfo
    {
        /// <summary>
        /// The _binding
        /// </summary>
        ConstantBinding _binding;
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrTypeBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ClrTypeBindingTargetInfo(Type type)
            : base(type.Name, ClrTargetType.Type)
        {
            _binding = new ConstantBinding(type, this);
        }
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public override Binding Bind(BindingRequest request)
        {
            return _binding;
        }
    }

    /// <summary>
    /// Class ClrMethodBindingTargetInfo.
    /// </summary>
    public class ClrMethodBindingTargetInfo : ClrInteropBindingTargetInfo, ICallTarget
    { //The object works as ICallTarget itself
        /// <summary>
        /// The instance
        /// </summary>
        public object Instance;
        /// <summary>
        /// The declaring type
        /// </summary>
        public Type DeclaringType;
        /// <summary>
        /// The _invoke flags
        /// </summary>
        BindingFlags _invokeFlags;
        /// <summary>
        /// The _binding
        /// </summary>
        Binding _binding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMethodBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="declaringType">Type of the declaring.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="instance">The instance.</param>
        public ClrMethodBindingTargetInfo(Type declaringType, string methodName, object instance = null)
            : base(methodName, ClrTargetType.Method)
        {
            DeclaringType = declaringType;
            Instance = instance;
            _invokeFlags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic;
            if (Instance == null)
                _invokeFlags |= BindingFlags.Static;
            else
                _invokeFlags |= BindingFlags.Instance;
            _binding = new ConstantBinding(target: this as ICallTarget, targetInfo: this);
            //The object works as CallTarget itself; the "as" conversion is not needed in fact, we do it just to underline the role
        }

        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public override Binding Bind(BindingRequest request)
        {
            return _binding;
        }

        #region ICalllable.Call implementation
        /// <summary>
        /// Calls the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Object.</returns>
        public object Call(ScriptThread thread, object[] args)
        {
            if (args != null && args.Length == 0)
                args = null;
            var result = DeclaringType.InvokeMember(base.Symbol, _invokeFlags, null, Instance, args);
            return result;
        }
        #endregion
    }

    /// <summary>
    /// Class ClrPropertyBindingTargetInfo.
    /// </summary>
    public class ClrPropertyBindingTargetInfo : ClrInteropBindingTargetInfo
    {
        /// <summary>
        /// The instance
        /// </summary>
        public object Instance;
        /// <summary>
        /// The property
        /// </summary>
        public PropertyInfo Property;
        /// <summary>
        /// The _binding
        /// </summary>
        Binding _binding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrPropertyBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="instance">The instance.</param>
        public ClrPropertyBindingTargetInfo(PropertyInfo property, object instance)
            : base(property.Name, ClrTargetType.Property)
        {
            Property = property;
            Instance = instance;
            _binding = new Binding(this);
            _binding.GetValueRef = GetPropertyValue;
            _binding.SetValueRef = SetPropertyValue;
        }
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public override Binding Bind(BindingRequest request)
        {
            return _binding;
        }
        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object GetPropertyValue(ScriptThread thread)
        {
            var result = Property.GetValue(Instance, null);
            return result;
        }
        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private void SetPropertyValue(ScriptThread thread, object value)
        {
            Property.SetValue(Instance, value, null);
        }
    }

    /// <summary>
    /// Class ClrFieldBindingTargetInfo.
    /// </summary>
    public class ClrFieldBindingTargetInfo : ClrInteropBindingTargetInfo
    {
        /// <summary>
        /// The instance
        /// </summary>
        public object Instance;
        /// <summary>
        /// The field
        /// </summary>
        public FieldInfo Field;
        /// <summary>
        /// The _binding
        /// </summary>
        Binding _binding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrFieldBindingTargetInfo"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="instance">The instance.</param>
        public ClrFieldBindingTargetInfo(FieldInfo field, object instance)
            : base(field.Name, ClrTargetType.Field)
        {
            Field = field;
            Instance = instance;
            _binding = new Binding(this);
            _binding.GetValueRef = GetPropertyValue;
            _binding.SetValueRef = SetPropertyValue;
        }
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public override Binding Bind(BindingRequest request)
        {
            return _binding;
        }
        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        private object GetPropertyValue(ScriptThread thread)
        {
            var result = Field.GetValue(Instance);
            return result;
        }
        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        private void SetPropertyValue(ScriptThread thread, object value)
        {
            Field.SetValue(Instance, value);
        }
    }

    // Method for adding methods to BuiltIns table in Runtime
    /// <summary>
    /// Class BindingSourceTableExtensions.
    /// </summary>
    public static partial class BindingSourceTableExtensions
    {
        /// <summary>
        /// Imports the static members.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="fromType">From type.</param>
        public static void ImportStaticMembers(this BindingSourceTable targets, Type fromType)
        {
            var members = fromType.GetMembers(BindingFlags.Public | BindingFlags.Static);
            foreach (var member in members)
            {
                if (targets.ContainsKey(member.Name)) continue; //do not import overloaded methods several times
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        targets.Add(member.Name, new ClrMethodBindingTargetInfo(fromType, member.Name));
                        break;
                    case MemberTypes.Property:
                        targets.Add(member.Name, new ClrPropertyBindingTargetInfo(member as PropertyInfo, null));
                        break;
                    case MemberTypes.Field:
                        targets.Add(member.Name, new ClrFieldBindingTargetInfo(member as FieldInfo, null));
                        break;
                }//switch
            }//foreach
        }
    }



}
