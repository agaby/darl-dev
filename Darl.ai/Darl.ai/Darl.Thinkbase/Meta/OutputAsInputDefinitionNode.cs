/// <summary>
/// OutputAsInputDefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;
using static Darl.Thinkbase.Meta.InputDefinitionNode;

namespace Darl.Thinkbase.Meta
{
    public class OutputAsInputDefinitionNode : OutputDefinitionNode
    {
        public InputTypes iType
        {
            get
            {
                switch (oType)
                {
                    case OutputTypes.textual_output:
                        return InputTypes.textual_input;
                    case OutputTypes.numeric_output:
                        return InputTypes.numeric_input;
                    case OutputTypes.categorical_output:
                        return InputTypes.categorical_input;
                    case OutputTypes.temporal_output:
                        return InputTypes.temporal_input;
                    default:
                        return InputTypes.numeric_input;
                }
            }
        }

        /// <summary>
        /// Calculate the mutual information between the data items indexed by the indexes in indices,
        /// from this input and the matching items in the output. dSplit is the partial result as in C4.
        /// </summary>
        /// <param name="indices">array of indexes of data items for this calc.</param>
        /// <param name="output">output to compare to.</param>
        /// <param name="dSplit">partial result</param>
        /// <returns>The mutual information</returns>
        public override double CalculateInformation(List<int> indices, OutputDefinitionNode output, ref double dSplit)
        {
            double dResult = 0.0;
            dSplit = 0.0;
            int nInputPartitions = categories.Count;
            int nOutputPartitions = output.categories.Count;
            double[] dEntropy = new double[nInputPartitions];
            double[] dSumOfMemberships = new double[nInputPartitions];
            double dSumOfSums = 0.0;
            for (int n = 0; n < nInputPartitions; n++)
            {
                double[] dInputMembership = new double[nOutputPartitions];
                dSumOfMemberships[n] = 0.0;
                for (int p = 0; p < nOutputPartitions; p++)
                {
                    dInputMembership[p] = 0.0;
                    foreach (int Index in indices)
                    {
                        double inmem = CalculateMembership(Index, n);
                        //                        Debug.WriteLine("input {0}, index {1}, set {2} val {3}",name, Index, n, inmem);
                        double outmem = output.CalculateMembership(Index, p);
                        dInputMembership[p] += inmem * outmem;
                    }
                    dSumOfMemberships[n] += dInputMembership[p];
                }
                dEntropy[n] = 0.0;
                for (int p = 0; p < nOutputPartitions; p++)
                {
                    if (dSumOfMemberships[n] != 0.0)
                    {
                        double dFraction = dInputMembership[p] / dSumOfMemberships[n];
                        if (dFraction != 0.0)
                            dEntropy[n] += dFraction * Math.Log10(dFraction) * 3.321928094887;
                    }
                }
                dSumOfSums += dSumOfMemberships[n];
            }
            for (int n = 0; n < nInputPartitions; n++)
            {
                if (dSumOfSums != 0.0)
                {
                    double dRatio = dSumOfMemberships[n] / dSumOfSums;
                    dResult += dRatio * dEntropy[n] * -1.0;
                    if (dRatio != 0.0)
                        dSplit += dRatio * Math.Log10(dRatio) * 3.321928094887;
                }
                else
                    dResult = -100.0;
            }
            dSplit = dSplit * -1.0;
            return dResult;
        }

        /// <summary>
        /// Writes out a single "is" term and asociated input and set/category/vocab
        /// </summary>
        /// <param name="middle">Receives text</param>
        /// <param name="currentIndex">Partition for this term.</param>
        public override void WriteTerm(ref string middle, string currentIndex)
        {
            middle = string.Concat(middle, name, " is ");
            if (iType == InputDefinitionNode.InputTypes.presence_input)
            {
                if (currentIndex[0] == 'a')
                    middle = string.Concat(middle, "absent ");
                else
                    middle = string.Concat(middle, "present ");
            }
            else if (iType == InputDefinitionNode.InputTypes.categorical_input)
            {
                middle += string.Format("\"{0}\" ", currentIndex);
            }
            else
            {
                middle = string.Concat(middle, currentIndex, " ");
            }
        }

        public override bool IsNumeric()
        {
            return iType == InputDefinitionNode.InputTypes.numeric_input || iType == InputDefinitionNode.InputTypes.arity_input;
        }

    }
}
