// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ExpressionEvaluatorRuntime.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq;
using DarlCompiler.Parsing;

namespace DarlCompiler.Interpreter.Evaluator
{
    /// <summary>
    /// Class ExpressionEvaluatorRuntime.
    /// </summary>
    public class ExpressionEvaluatorRuntime : LanguageRuntime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluatorRuntime"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public ExpressionEvaluatorRuntime(LanguageData language)
            : base(language)
        {
        }
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Init()
        {
            base.Init();
            //add built-in methods, special form IIF, import Math and Environment methods
            BuiltIns.AddMethod(BuiltInPrintMethod, "print");
            BuiltIns.AddMethod(BuiltInFormatMethod, "format");
            BuiltIns.AddSpecialForm(SpecialFormsLibrary.Iif, "iif", 3, 3);
            BuiltIns.ImportStaticMembers(typeof(System.Math));
            BuiltIns.ImportStaticMembers(typeof(Environment));
        }

        //Built-in methods
        /// <summary>
        /// Builts the in print method.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Object.</returns>
        private object BuiltInPrintMethod(ScriptThread thread, object[] args)
        {
            string text = string.Empty;
            switch (args.Length)
            {
                case 1:
                    text = string.Empty + args[0]; //compact and safe conversion ToString()
                    break;
                case 0:
                    break;
                default:
                    text = string.Join(" ", args);
                    break;
            }
            thread.App.WriteLine(text);
            return null;
        }
        /// <summary>
        /// Builts the in format method.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Object.</returns>
        private object BuiltInFormatMethod(ScriptThread thread, object[] args)
        {
            if (args == null || args.Length == 0) return null;
            var template = args[0] as string;
            if (template == null)
                this.ThrowScriptError("Format template must be a string.");
            if (args.Length == 1) return template;
            //create formatting args array
            var formatArgs = args.Skip(1).ToArray();
            var text = string.Format(template, formatArgs);
            return text;

        }
    }
}
