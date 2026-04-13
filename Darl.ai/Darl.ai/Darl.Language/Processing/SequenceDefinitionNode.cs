/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Text;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Placeholder for concept matching extension
    /// </summary>
    public class SequenceDefinitionNode : DarlNode
    {

        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public List<List<string>> Value { get; private set; }

        /// <summary>
        /// Gets the name of the constant.
        /// </summary>
        /// <value>
        /// The name of the constant.
        /// </value>
        public string name { get; private set; }


        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Value = new List<List<string>>();
            foreach (var child in nodes[1].ChildNodes)//nodes[1] is sequence_list
            {
                var list = new List<string>();
                if (child.Term.Name == "stringLiteral")
                {
                    list.Add(child.Token.ValueString);
                }
                else if (child.Term.Name == "subsequence_list")
                {
                    foreach (var subchild in child.ChildNodes[0].ChildNodes)
                    {
                        list.Add(subchild.Token.ValueString);
                    }
                }
                Value.Add(list);
            }
            name = nodes[0].Token.Text;
        }

        private string SequenceToString(List<List<string>> seq)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach (var ss in seq)
            {
                sb.Append("{");
                sb.Append(string.Join(",", ss));
                sb.Append("}");
            }
            sb.Append("}");
            return sb.ToString();
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
                return $"sequence {name} {SequenceToString(Value)};\n";
            }
        }


    }
}
