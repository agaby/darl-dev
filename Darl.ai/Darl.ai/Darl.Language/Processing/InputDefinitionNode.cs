/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements an input definition
    /// </summary>
    public class InputDefinitionNode : IODefinitionNode
    {

        /// <summary>
        /// Permissible input types
        /// </summary>
        public enum InputTypes
        {
            /// <summary>
            /// The numeric_input
            /// </summary>
            numeric_input,
            /// <summary>
            /// The categorical_input
            /// </summary>
            categorical_input,
            /// <summary>
            /// The textual_input
            /// </summary>
            textual_input,
            /// <summary>
            /// The arity_input
            /// </summary>
            arity_input,
            /// <summary>
            /// The presence_input
            /// </summary>
            presence_input,
            /// <summary>
            /// temporal input
            /// </summary>
            temporal_input,
            ///<summary>
            /// dynamic categorical input
            ///</summary>
            dynamic_categorical_input
        };

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        /// <value>
        /// The type of the input.
        /// </value>
        public InputTypes iType { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public DarlResult Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputDefinitionNode"/> class.
        /// </summary>
        public InputDefinitionNode()
        {
            Value = new DarlResult(0.0, true);
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            //edit to store node tree as well, so that toDarl can differentiate betwen string literals and identifiers, since both are permitted.
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            iType = (InputTypes)Enum.Parse(typeof(InputTypes), nodes[0].Term.Name, true);
            name = (string)nodes[0].Token.Value;
            switch (iType)
            {
                case InputTypes.numeric_input:
                case InputTypes.arity_input:
                case InputTypes.temporal_input:
                    if (context.Values.Count > 0)
                    {
                        foreach (object key in context.Values.Keys)
                        {
                            sets.Add((string)key, (DarlResult)context.Values[key]);
                        }
                        context.Values.Clear();
                    }
                    break;
                case InputTypes.categorical_input:
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
                case InputTypes.dynamic_categorical_input:
                    var store = ((StoreNode)nodes[1].AstNode);
                    var storeName = ((DarlIdentifierNode)store.ChildNodes[0]).name;
                    if (context.Stores != null)
                    {
                        if (context.Stores.ContainsKey(storeName))
                        {
                            store.storeDefinition = new StoreDefinitionNode();
                            store.storeDefinition.storeInterface = context.Stores[storeName];
                            foreach (var c in store.GetChildNodes())
                            {
                                if (c is StoreNode)
                                {
                                    StoreNode child = c as StoreNode;
                                    var childStoreName = ((DarlIdentifierNode)child.ChildNodes[0]).name;
                                    if (context.Stores.ContainsKey(childStoreName))
                                    {
                                        child.storeDefinition = new StoreDefinitionNode();
                                        child.storeDefinition.storeInterface = context.Stores[childStoreName];
                                    }
                                }
                            }
                            DarlResult result = (DarlResult)store.Evaluate(new ScriptThread(new ScriptApp(context.Language))).Result;
                            foreach (var name in result.categories.Keys)
                            {
                                categories.Add(name);
                            }
                        }
                    }
                    break;
            }
        }


        /// <summary>
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
                    case InputTypes.arity_input:
                        sb.AppendLine($"input arity {name};");
                        break;
                    case InputTypes.categorical_input:
                        if (categories.Count > 0)
                        {
                            sb.Append($"input categorical {name} {{ ");
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
                            sb.AppendLine($"input categorical {name};");
                        }
                        break;
                    case InputTypes.numeric_input:
                    case InputTypes.temporal_input:
                        var t = iType == InputTypes.numeric_input ? "numeric" : "temporal";
                        if (sets.Count > 0)
                        {
                            sb.Append($"input {t} {name} {{ ");
                            int setindex = 0;
                            foreach (string set in sets.Keys)
                            {
                                sb.Append("{" + set + ", ");
                                int valCount = 0;
                                foreach (double d in sets[set].values)
                                {
                                    valCount++;
                                    sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture) + (valCount == sets[set].values.Count ? "" : ","));
                                }
                                sb.Append("}");
                                setindex++;
                                if (setindex < sets.Count)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.AppendLine("};");
                        }
                        else
                        {
                            sb.AppendLine($"input {t} {name};");
                        }
                        break;
                    case InputTypes.presence_input:
                        sb.AppendLine($"input presence {name};");
                        break;
                    case InputTypes.textual_input:
                        sb.AppendLine($"input textual {name};");
                        break;
                }
                return sb.ToString();
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
        public double CalculateInformation(List<int> indices, OutputDefinitionNode output, ref double dSplit)
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
        /// Calculates the membership.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="set">The set.</param>
        /// <returns>The membership</returns>
        internal override double CalculateMembership(int index, int set)
        {
            switch (iType)
            {
                case InputTypes.categorical_input:
                case InputTypes.presence_input:
                    return learningSource[index] == set ? 1.0 : 0.0;
                default:
                    return CalculateSetMembership(learningSource[index], set);
            }
        }

    }
}
