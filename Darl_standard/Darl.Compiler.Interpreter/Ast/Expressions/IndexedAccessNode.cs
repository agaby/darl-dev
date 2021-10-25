// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="IndexedAccessNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class IndexedAccessNode.
    /// </summary>
    public class IndexedAccessNode : AstNode
    {
        /// <summary>
        /// The _target
        /// </summary>
        AstNode _target, _index;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            _target = AddChild("Target", nodes.First());
            _index = AddChild("Index", nodes.Last());
            AsString = "[" + _index + "]";
        }

        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            object result = null;
            var targetValue = await _target.Evaluate(thread);
            if (targetValue == null)
                thread.ThrowScriptError("Target object is null.");
            var type = targetValue.GetType();
            var indexValue = _index.Evaluate(thread);
            //string and array are special cases
            if (type == typeof(string))
            {
                var sTarget = targetValue as string;
                var iIndex = Convert.ToInt32(indexValue);
                result = sTarget[iIndex];
            }
            else if (type.IsArray)
            {
                var arr = targetValue as Array;
                var iIndex = Convert.ToInt32(indexValue);
                result = arr.GetValue(iIndex);
            }
            else if (targetValue is IDictionary)
            {
                var dict = (IDictionary)targetValue;
                result = dict[indexValue];
            }
            else
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.InvokeMethod;
                result = type.InvokeMember("get_Item", flags, null, targetValue, new object[] { indexValue });
            }
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        /// <summary>
        /// Does the set value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        public override async Task DoSetValue(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;  //standard prologue
            var targetValue = await _target.Evaluate(thread);
            if (targetValue == null)
                thread.ThrowScriptError("Target object is null.");
            var type = targetValue.GetType();
            var indexValue = _index.Evaluate(thread);
            //string and array are special cases
            if (type == typeof(string))
            {
                thread.ThrowScriptError("String is read-only.");
            }
            else if (type.IsArray)
            {
                var arr = targetValue as Array;
                var iIndex = Convert.ToInt32(indexValue);
                arr.SetValue(value, iIndex);
            }
            else if (targetValue is IDictionary)
            {
                var dict = (IDictionary)targetValue;
                dict[indexValue] = value;
            }
            else
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.InvokeMethod;
                type.InvokeMember("set_Item", flags, null, targetValue, new object[] { indexValue, value });
            }
            thread.CurrentNode = Parent; //standard epilogue
        }

    }


}
