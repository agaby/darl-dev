/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarlLanguage.Processing
{
    /// Implements an output definition
    /// </summary>
    public class OutputDefinitionNode : IOSequenceDefinitionNode
    {

        /// Enumerates the possible output types
        /// </summary>
        public enum OutputTypes
        {
            /// is numeric
            /// </summary>
            numeric_output,
            /// is categorical
            /// </summary>
            categorical_output,
            /// is textual
            /// </summary>
            textual_output,
            /// is temporal
            /// </summary>
            temporal_output
        };

        /// Gets the type of the output.
        /// </summary>
        /// <value>
        /// The type of the output.
        /// </value>
        public OutputTypes iType { get; private set; }

        /// Initializes a new instance of the <see cref="OutputDefinitionNode"/> class.
        /// </summary>
        public OutputDefinitionNode()
        {
            result = new DarlResult(0.0, true);
        }


        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            iType = (OutputTypes)Enum.Parse(typeof(OutputTypes), nodes[0].Term.Name, true);
            name = (string)nodes[0].Token.Value;
            switch (iType)
            {
                case OutputTypes.temporal_output:
                case OutputTypes.numeric_output:
                    if (context.Values.Count > 0)
                    {
                        foreach (object key in context.Values.Keys)
                        {
                            sets.Add((string)key, (DarlResult)context.Values[key]);
                        }
                        context.Values.Clear();
                    }
                    break;
                case OutputTypes.categorical_output:
                    if (nodes.Count > 1)
                    {
                        foreach (var catdef in nodes[1].ChildNodes)
                        {
                            string name = (string)catdef.Token.Value;
                            categories.Add(name);
                            catsAsIdentifiers.Add(name, (catdef.AstNode is DarlIdentifierNode));
                        }
                    }
                    break;
            }
            sequence = 0;
        }

        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                switch (iType)
                {
                    case OutputTypes.categorical_output:
                        if (categories.Count > 0)
                        {
                            sb.Append("output categorical " + name + " {");
                            int catindex = 0;
                            foreach (string cat in categories)
                            {
                                sb.Append(catsAsIdentifiers[cat] ? cat : $"\"{cat}\"");
                                catindex++;
                                if (catindex != categories.Count)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.AppendLine("};");
                        }
                        else
                        {
                            sb.AppendLine("output categorical " + name + ";");
                        }
                        break;
                    case OutputTypes.temporal_output:
                    case OutputTypes.numeric_output:
                        var t = iType == OutputTypes.numeric_output ? "numeric" : "temporal";
                        if (sets.Count > 0)
                        {

                            sb.Append($"output {t} {name} {{");
                            int setindex = 0;
                            foreach (string set in sets.Keys)
                            {
                                sb.Append($"{{ {set}, ");
                                int valCount = 0;
                                foreach (double d in sets[set].values)
                                {
                                    valCount++;
                                    sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture) + (valCount == sets[set].values.Count ? "" : ","));
                                }
                                sb.Append("}");
                                setindex++;
                                if (setindex != sets.Count)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.AppendLine("};");
                        }
                        else
                        {
                            sb.AppendLine($"output {t} {name};");
                        }
                        break;
                    case OutputTypes.textual_output:
                        sb.AppendLine($"output textual {name};");
                        break;

                }
                return sb.ToString();
            }
        }

        /// get a table of the partitions found in the list of indices, derived from a decision node
        /// ignoring minor results. the 2nd element is the confidence.
        /// </summary>
        /// <param name="indices">The indices.</param>
        /// <returns>The table</returns>
        public Dictionary<object, double> ChoosePartitions(List<int> indices)
        {
            // called from a leaf node
            // find the dominant partition of the data pointed to by the indices array
            Dictionary<object, double> classMap = new Dictionary<object, double>();
            double dSumOfScores = 0.0;
            foreach (int index in indices)
            {
                if (learningSource[index] != -1)
                {
                    if (iType == OutputTypes.categorical_output)
                    {
                        int val = learningSource[index];
                        if (classMap.ContainsKey(val))
                        {
                            double dTemp = (double)classMap[val];
                            classMap[val] = dTemp + 1.0;
                        }
                        else
                        {
                            classMap.Add(val, 1.0);
                        }
                        dSumOfScores += 1.0;
                    }
                    else
                    {
                        for (int nSet = 0; nSet < sets.Count; nSet++)
                        {
                            double dRes = this.CalculateSetMembership(learningSource[index], nSet);
                            if (dRes > 0.0)
                            {
                                //			string setName = ((Result)sets[nSet]).identifier;
                                if (classMap.ContainsKey(nSet))
                                {
                                    double dTemp = (double)classMap[nSet];
                                    classMap[nSet] = dTemp + dRes;
                                }
                                else
                                {
                                    classMap.Add(nSet, dRes);
                                }
                                dSumOfScores += dRes;
                            }
                        }
                    }
                }
            }
            Dictionary<object, double> newMap = new Dictionary<object, double>();
            if (dSumOfScores > 0.0)
            {
                foreach (Object temp in classMap.Keys)
                {
                    double dTemp = (double)classMap[temp];
                    newMap.Add(temp, dTemp / dSumOfScores);
                }
            }
            return newMap;
        }

        /// Calculates the membership.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="set">The set.</param>
        /// <returns>
        /// THe membership
        /// </returns>
        internal override double CalculateMembership(int index, int set)
        {
            switch (iType)
            {
                case OutputTypes.categorical_output:
                    return learningSource[index] == set ? 1.0 : 0.0;
                default:
                    return CalculateSetMembership(learningSource[index], set);
            }
        }

        public override string GetName()
        {
            return name;
        }

    }
}
