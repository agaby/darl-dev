/// <summary>
/// LanguageDataBuilder.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="LanguageDataBuilder.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System.Diagnostics;

namespace DarlCompiler.Parsing.Construction
{
    /// <summary>
    /// Class LanguageDataBuilder.
    /// </summary>
    internal class LanguageDataBuilder
    {

        /// <summary>
        /// The language
        /// </summary>
        internal LanguageData Language;

        /// <summary>
        /// The _grammar
        /// </summary>
        readonly Grammar _grammar;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageDataBuilder"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public LanguageDataBuilder(LanguageData language)
        {
            Language = language;
            _grammar = Language.Grammar;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Build()
        {
            var sw = new Stopwatch();
            try
            {
                if (_grammar.Root == null)
                    Language.Errors.AddAndThrow(GrammarErrorLevel.Error, null, Resources.ErrRootNotSet);
                sw.Start();
                var gbld = new GrammarDataBuilder(Language);
                gbld.Build();
                //Just in case grammar author wants to customize something...
                _grammar.OnGrammarDataConstructed(Language);
                var sbld = new ScannerDataBuilder(Language);
                sbld.Build();
                var pbld = new ParserDataBuilder(Language);
                pbld.Build();
                Validate();
                //call grammar method, a chance to tweak the automaton
                _grammar.OnLanguageDataConstructed(Language);
                return true;
            }
            catch (GrammarErrorException)
            {
                return false; //grammar error should be already added to Language.Errors collection
            }
            finally
            {
                Language.ErrorLevel = Language.Errors.GetMaxLevel();
                sw.Stop();
                Language.ConstructionTime = sw.ElapsedMilliseconds;
            }

        }

        #region Language Data Validation
        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {

        }
        #endregion


    }
}
