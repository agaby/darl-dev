// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="InterpretedMetaLanguageGrammar.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.Thinkbase.Meta;
using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter
{
    /// <summary>
    /// Base class for languages that use Darl Interpreter to execute scripts.
    /// </summary>
    public abstract class InterpretedMetaLanguageGrammar : InterpretedLanguageGrammar
    {
        // making the class abstract so it won't load into Grammar Explorer
        /// <summary>
        /// Initializes a new instance of the <see cref="InterpretedMetaLanguageGrammar"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public InterpretedMetaLanguageGrammar(bool caseSensitive)
            : base(caseSensitive)
        {
            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        /// <summary>
        /// Runs the sample.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        public virtual new async Task<DarlMetaActivity?> RunSample(RunSampleArgs args)
        {
            if (_app == null || args.ParsedSample != _prevSample)
                _app = new ScriptApp(args.Language);
            _prevSample = args.ParsedSample;
            return await _app.Evaluate(args.ParsedSample);
        }

    }

}
