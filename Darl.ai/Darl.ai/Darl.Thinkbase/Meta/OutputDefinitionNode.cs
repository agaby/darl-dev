/// <summary>
/// OutputDefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase.Meta
{
    public class OutputDefinitionNode : IOSequenceDefinitionNode
    {
        /// <summary>
        /// Enumerates the possible output types
        /// </summary>
        public enum OutputTypes
        {
            /// <summary>
            /// is numeric
            /// </summary>
            numeric_output,
            /// <summary>
            /// is categorical
            /// </summary>
            categorical_output,
            /// <summary>
            /// is textual
            /// </summary>
            textual_output,
            /// <summary>
            /// is temporal
            /// </summary>
            temporal_output,
            /// <summary>
            /// Is network
            /// </summary>
            network_output
        };

        /// <summary>
        /// Gets the type of the output.
        /// </summary>
        /// <value>
        /// The type of the output.
        /// </value>
        public OutputTypes oType { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDefinitionNode"/> class.
        /// </summary>
        public OutputDefinitionNode()
        {
            result = new DarlResult(0.0, true);
        }

        public string nodeId { get; set; }



        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            oType = (OutputTypes)Enum.Parse(typeof(OutputTypes), nodes[0].Term.Name, true);
            name = (string)nodes[0].Token.Value;
            SetLineagesInInit(context, nodes);
            switch (oType)
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
                        if (nodes[1].ChildNodes.Any() && nodes[1].ChildNodes[0].AstNode is AttributeNode)
                        {
                            var attributeNode = nodes[1].ChildNodes[0].AstNode as AttributeNode;
                            CatLineageNode = attributeNode.ChildNodes[0] as DarlMetaNode;
                        }
                        else
                        {
                            foreach (var catdef in nodes[1].ChildNodes)
                            {
                                string name = (string)catdef.Token.Value;
                                categories.Add(name);
                                catsAsIdentifiers.Add(name, (catdef.AstNode is DarlMetaIdentifierNode));
                            }
                        }
                    }
                    break;
                case OutputTypes.network_output:
                    {
                        nodeId = (string)nodes[1].Token.Value;
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
                switch (oType)
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
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append("output categorical " + name);
                        }
                        break;
                    case OutputTypes.temporal_output:
                    case OutputTypes.numeric_output:
                        var t = oType == OutputTypes.numeric_output ? "numeric" : "temporal";
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
                                    sb.Append(Math.Round(d, 4).ToString(System.Globalization.CultureInfo.InvariantCulture) + (valCount == sets[set].values.Count ? "" : ","));
                                }
                                sb.Append("}");
                                setindex++;
                                if (setindex != sets.Count)
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append($"output {t} {name}");
                        }
                        break;
                    case OutputTypes.textual_output:
                        sb.Append($"output textual {name}");
                        break;

                }
                if (lineageNode != null)
                {
                    sb.Append(" " + lineageNode.TermToDarl());
                }
                sb.AppendLine(";");
                return sb.ToString();
            }
        }

        public void SetLineage(ScriptThread thread)
        {
            if (lineageNode != null)
                lineage = lineageNode is LineageLiteral ? ((LineageLiteral)lineageNode).literal : ((DarlResult)lineageNode.Evaluate(thread).Result).Value as string;
            if (CatLineageNode != null)
            {
                var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
                if (grammar == null || grammar.currentNode == null || grammar.currentNode.properties == null)
                    return;
                var lin = CatLineageNode is LineageLiteral ? ((LineageLiteral)CatLineageNode).literal : ((DarlResult)CatLineageNode.Evaluate(thread).Result).Value as string;
                var att = grammar.currentModel.FindDataAttribute(grammar.currentNode.id, lin, grammar.state);
                if (att == null) //assume content is a comma delimited list of strings.
                    return;
                var cats = att.Value.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in cats)
                {
                    categories.Add(c.Replace('"', ' ').Trim());
                }
            }
        }

        /// <summary>
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
                    if (oType == OutputTypes.categorical_output)
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

        /// <summary>
        /// Calculates the membership.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="set">The set.</param>
        /// <returns>
        /// The membership
        /// </returns>
        internal override double CalculateMembership(int index, int set)
        {
            switch (oType)
            {
                case OutputTypes.categorical_output:
                    return learningSource[index] == set ? 1.0 : 0.0;
                default:
                    return CalculateSetMembership(learningSource[index], set);
            }
        }

    }
}