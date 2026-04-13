/// <summary>
/// LineageDefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace Darl.Thinkbase.Meta
{
    public class LineageDefinitionNode : DarlMetaNode
    {
        /// <summary>
        /// Gets the value of the string.
        /// </summary>
        /// <value>
        /// The value of the string.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets the name of the string.
        /// </summary>
        /// <value>
        /// The name of the string.
        /// </value>
        public string name { get; set; }

        public string typeword { get; set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Value = (string)nodes[1].Token.Value;
            name = nodes[0].Token.Text;
            var validity = LineageLibrary.CheckLineageWithTypeWord(Value);
            if (!validity.Item1)
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.Span.Location, $"'{Value}' is not a valid lineage.");
            else
            {
                typeword = validity.Item2;
                var str = ((DarlMetaGrammar)context.Language.Grammar).structure;
                if (str != null)
                {
                    if (!str.CommonLineages.ContainsKey(name))
                    {
                        str.CommonLineages.TryAdd(name, Value);
                    }
                }
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
                return $"lineage {name} \"{Value}\";\n";
            }
        }
    }
}
