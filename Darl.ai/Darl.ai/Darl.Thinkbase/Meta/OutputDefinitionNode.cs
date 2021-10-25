using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
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
        public OutputTypes iType { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDefinitionNode"/> class.
        /// </summary>
        public OutputDefinitionNode()
        {
            result = new DarlResult(0.0, true);
        }

        public string lineage { get; set; }

        public string typeword { get; set; }

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
            iType = (OutputTypes)Enum.Parse(typeof(OutputTypes), nodes[0].Term.Name, true);
            name = (string)nodes[0].Token.Value;
            var lin = nodes.Last();
            if (lin.Term.Name == "lineageLiteral")
            {
                //get lineage and check it.
                var linLineage = (string)lin.Token.Value;
                var validity = Darl.Lineage.LineageLibrary.CheckLineageWithTypeWord(linLineage);
                if (!validity.Item1)
                {
                    context.AddMessage(DarlCompiler.ErrorLevel.Error, lin.Token.Location, $"'{linLineage}' is not a valid lineage.");
                }
                else
                {
                    typeword = validity.Item2;
                    lineageNode = lin.AstNode as DarlMetaNode;
                }
            }
            else if (lin.Term.Name == "lineage_constant")
            {
                lineageNode = lin.AstNode as DarlMetaNode;
                //can't check lineage validity
            }
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

        public void SetLineage(ScriptThread thread)
        {
            if (lineageNode != null)
                lineage = lineageNode is LineageLiteral ? ((LineageLiteral)lineageNode).literal : ((DarlResult)lineageNode.Evaluate(thread).Result).Value as string;
            if (CatLineageNode != null)
            {
                var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
                if (grammar.currentNode == null || grammar.currentNode.properties == null)
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

    }
}