/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ICanRunSample.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Threading.Tasks;

namespace DarlCompiler.Parsing
{
    // Should be implemented by Grammar class to be able to run samples in Grammar Explorer.
    /// Interface ICanRunSample
    /// </summary>
    public interface ICanRunSample
    {
        /// Runs the sample.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        Task<string> RunSample(RunSampleArgs args);
    }

    /// Class RunSampleArgs.
    /// </summary>
    public class RunSampleArgs
    {
        /// The language
        /// </summary>
        public LanguageData Language;
        /// The sample
        /// </summary>
        public string Sample;
        /// The parsed sample
        /// </summary>
        public ParseTree ParsedSample;
        /// Initializes a new instance of the <see cref="RunSampleArgs"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="sample">The sample.</param>
        /// <param name="parsedSample">The parsed sample.</param>
        public RunSampleArgs(LanguageData language, string sample, ParseTree parsedSample)
        {
            Language = language;
            Sample = sample;
            ParsedSample = parsedSample;
        }
    }

}
