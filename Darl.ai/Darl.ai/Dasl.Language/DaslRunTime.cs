/// </summary>

﻿// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslRunTime.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaslLanguage
{
    /// Extends the DArlRuntime to perform simulations
    /// </summary>
    public class DaslRunTime : DarlRunTime
    {

        /// Initializes a new instance of the <see cref="DaslRunTime" /> class.
        /// </summary>
        public DaslRunTime() : base()
        {
        }

        /// Simulates the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cycles">The cycles.</param>
        /// <param name="tree">The tree.</param>
        /// <returns>Simulation results</returns>
        /// <exception cref="System.ArgumentNullException">data
        /// or
        /// source</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">cycles</exception>
        /// <exception cref="System.ArgumentException">Compilation errors.</exception>
        public async Task<List<List<DarlResult>>> Simulate(List<List<DarlResult>> data, int cycles, ParseTree tree)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (cycles < 1)
                throw new ArgumentOutOfRangeException("cycles");

            var simResults = new List<List<DarlResult>>();
            if (tree.HasErrors())
                throw new ArgumentException("Compilation errors.");

            //connect up delays to the historic data
            var root = tree.Root.AstNode as MapRootNode;
            foreach (var delay in root.delays)
                delay.History = simResults; //all delays view the same sim results
            //start simulation
            for (int count = 0; count < cycles; count++)
            {
                if (count < data.Count)
                {
                    simResults.Add(await Evaluate(tree, data[count]));
                }
                else
                {
                    simResults.Add(await Evaluate(tree, new List<DarlResult>()));
                }
            }

            return simResults;
        }
    }
}
