/// <summary>
/// </summary>

﻿using Darl.Thinkbase.Meta;
using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter
{
    public class InterpretedLanguageGrammar : Grammar, ICanRunSample
    {
        // making the class abstract so it won't load into Grammar Explorer
        /// <summary>
        /// Initializes a new instance of the <see cref="InterpretedLanguageGrammar"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public InterpretedLanguageGrammar(bool caseSensitive)
            : base(caseSensitive)
        {
            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        // This method allows custom implementation of running a sample in Grammar Explorer
        // By default it evaluates a parse tree using default interpreter.
        // Darl's interpreter has one restriction: once a script (represented by AST node) is evaluated in ScriptApp, 
        // its internal fields in AST nodes become tied to this particular instance of ScriptApp (more precisely DataMap).
        // If you want to evaluate the AST tree again, you have to do it in the context of the same DataMap. 
        // Grammar Explorer may call RunSample method repeatedly for evaluation of the same parsed script. So we keep ScriptApp instance in 
        // the field, and if we get the same script node, then we reuse the ScriptApp thus satisfying the requirement. 
        /// <summary>
        /// The _app
        /// </summary>
        protected ScriptApp _app;
        /// <summary>
        /// The _prev sample
        /// </summary>
        protected ParseTree _prevSample;

        /// <summary>
        /// Runs the sample.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        public virtual async Task<string> RunSample(RunSampleArgs args)
        {
            if (_app == null || args.ParsedSample != _prevSample)
                _app = new ScriptApp(args.Language);
            _prevSample = args.ParsedSample;
            var res =  await _app.Evaluate(args.ParsedSample);
            return res == null ? "" : res.ToString();
        }

        /// <summary>
        /// Creates the runtime.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>LanguageRuntime.</returns>
        public virtual LanguageRuntime CreateRuntime(LanguageData language)
        {
            return new LanguageRuntime(language);
        }

        /// <summary>
        /// Builds the ast.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="parseTree">The parse tree.</param>
        public override void BuildAst(LanguageData language, ParseTree parseTree, Dictionary<string, ILocalStore> stores)
        {
            var opHandler = new OperatorHandler(language.Grammar.CaseSensitive);
            Util.Check(!parseTree.HasErrors(), "ParseTree has errors, cannot build AST.");
            var astContext = new InterpreterAstContext(language, stores, opHandler);
            var astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        Task<string> ICanRunSample.RunSample(RunSampleArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}
