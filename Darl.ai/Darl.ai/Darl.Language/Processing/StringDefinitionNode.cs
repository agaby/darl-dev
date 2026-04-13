/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Impliments a string definition
    /// </summary>
    public class StringDefinitionNode : DarlNode
    {
        /// <summary>
        /// Gets the value of the string.
        /// </summary>
        /// <value>
        /// The value of the string.
        /// </value>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the name of the string.
        /// </summary>
        /// <value>
        /// The name of the string.
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
            Value = (string)nodes[1].Token.Value;
            name = nodes[0].Token.Text;
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
                return $"string {name} \"{Value}\";\n";
            }
        }

    }
}
