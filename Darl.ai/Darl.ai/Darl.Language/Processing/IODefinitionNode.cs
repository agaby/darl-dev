/// <summary>
/// IODefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Common IO items
    /// </summary>
    public class IODefinitionNode : DarlNode
    {
        /// <summary>
        /// Sets defined for this IO
        /// </summary>
        public Dictionary<string, DarlResult> sets = new Dictionary<string, DarlResult>();

        /// <summary>
        /// Categories defined for this IO
        /// </summary>
        public List<string> categories = new List<string>();

        public Dictionary<string, bool> catsAsIdentifiers = new Dictionary<string, bool>();


        /// <summary>
        /// Name of the I/O
        /// </summary>
        public string name { get; protected set; }

        /// <summary>
        /// Contains the crisp ind of each sample as categorized
        /// </summary>
        /// <remarks>
        /// if numeric, arity, contains the set ind, where the categories list holds the set names.
        /// So, with sets small, medium, large, 1 indicates medium.
        /// For categorical, the ind of the category, for presence 0 = present, 1 = absent.
        /// </remarks>
        public List<int> learningSource = new List<int>();

        /// <summary>
        /// Calculate the information in this input/output. 
        /// </summary>
        /// <param name="indices">indices to data to consider in this calculation.</param>
        /// <returns>information, or NaN if all one class</returns>
        public double CalculateInformation(List<int> indices)
        {
            int partitions = categories.Count;
            double membershipSum = 0.0;
            double[] memberships = new double[partitions];
            double result = 0.0;
            for (int n = 0; n < partitions; n++)
            {
                memberships[n] = 0.0;
                foreach (int index in indices)
                {
                    if (learningSource[index] != -1)
                    {
                        double res = CalculateMembership(index, n);
                        memberships[n] += res;
                        membershipSum += res;
                    }
                }
            }
            if (membershipSum == 0.0)
                return 0.0;
            for (int n = 0; n < partitions; n++)
            {
                double fraction = memberships[n] / membershipSum;
                if (fraction > 0.99) // i.e this is all one partition return nan
                    return double.NaN;
                if (fraction != 0.0)
                    result += fraction * Math.Log10(fraction) * 3.321928094887;
            }
            return result * -1.0;
        }

        /// <summary>
        /// Calculates the membership during machine learning
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="set">The set.</param>
        /// <returns>The membership</returns>
        internal virtual double CalculateMembership(int index, int set)
        {
            return 0.0;
        }

        /// <summary>
        /// Calculates set membership during machine learning
        /// </summary>
        /// <param name="index">The index of the value</param>
        /// <param name="set">The set to determine membership of</param>
        /// <returns>The membership degree of truth</returns>
        /// <remarks>To speed up processing, rank information is used to determine whether there is any match
        /// between sets and values at all, but linear, rather than rank interpolation is used if an index falls within a set's range.</remarks>
        public double CalculateSetMembership(int index, int set)
        {
            if (index == -1)//null data
                return 0.0;
            int rampNumber = index / 1000;
            if (rampNumber != set && set != rampNumber + 1)
                return 0.0; // outside of the sets
            if (rampNumber != set)
            {
                return (index % 1000 / 1000.0);
            }
            else
            {
                double offset = index % 1000.0;
                return 1.0 - (offset / 1000.0);
            }
        }

    }
}
