// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 09-10-2014
//
// Last Modified By : Andrew
// Last Modified On : 02-18-2015
// ***********************************************************************
// <copyright file="ExpressionEvaluator.cs" company="Dr Andy's IP LLC">
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using DarlCompiler.Parsing;


namespace DarlCompiler.Interpreter.Evaluator
{
    /// <summary>
    /// Class ExpressionEvaluator.
    /// </summary>
    public class ExpressionEvaluator
    {
        /// <summary>
        /// Gets the grammar.
        /// </summary>
        /// <value>The grammar.</value>
        public ExpressionEvaluatorGrammar Grammar { get; private set; }
        /// <summary>
        /// Gets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public Parser Parser { get; private set; }
        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>The language.</value>
        public LanguageData Language { get; private set; }
        /// <summary>
        /// Gets the runtime.
        /// </summary>
        /// <value>The runtime.</value>
        public LanguageRuntime Runtime { get; private set; }
        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        public ScriptApp App { get; private set; }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        /// <value>The globals.</value>
        public IDictionary<string, object> Globals
        {
            get { return App.Globals; }
        }

        //Default constructor, creates default evaluator 
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        public ExpressionEvaluator()
            : this(new ExpressionEvaluatorGrammar())
        {
        }

        //Default constructor, creates default evaluator 
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        public ExpressionEvaluator(ExpressionEvaluatorGrammar grammar)
        {
            Grammar = grammar;
            Language = new LanguageData(Grammar);
            Parser = new Parser(Language);
            Runtime = Grammar.CreateRuntime(Language);
            App = new ScriptApp(Runtime);
        }

        /// <summary>
        /// Evaluates the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>System.Object.</returns>
        public object Evaluate(string script)
        {
            var result = App.Evaluate(script);
            return result;
        }

        /// <summary>
        /// Evaluates the specified parsed script.
        /// </summary>
        /// <param name="parsedScript">The parsed script.</param>
        /// <returns>System.Object.</returns>
        public object Evaluate(ParseTree parsedScript)
        {
            var result = App.Evaluate(parsedScript);
            return result;
        }

        //Evaluates again the previously parsed/evaluated script
        /// <summary>
        /// Evaluates this instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public object Evaluate()
        {
            return App.Evaluate();
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void ClearOutput()
        {
            App.ClearOutputBuffer();
        }
        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetOutput()
        {
            return App.GetOutput();
        }

    }
}
