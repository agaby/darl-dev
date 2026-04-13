/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;

namespace DarlLanguage.Processing
{
    /// Implements a Constant definition
    /// </summary>
    public class ConstantDefinitionNode : DarlNode
    {
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public double Value { get; private set; }

        /// Gets the name of the constant.
        /// </summary>
        /// <value>
        /// The name of the constant.
        /// </value>
        public string name { get; private set; }


        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Value = Convert.ToDouble(nodes[1].Token.Value);
            name = nodes[0].Token.Text;
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
                return $"constant {name} {Value};\n";
            }
        }
    }
}
