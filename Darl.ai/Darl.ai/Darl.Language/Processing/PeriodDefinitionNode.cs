/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;

namespace DarlLanguage.Processing
{
    /// Implements a Constant definition
    /// </summary>
    public class DurationDefinitionNode : DarlNode
    {
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public TimeSpan Value { get; private set; }

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
            TimeSpan parsedVal;
            if (TimeSpan.TryParse(nodes[1].Token.Value as string, out parsedVal))
                Value = parsedVal;
            else
                context.AddMessage(DarlCompiler.ErrorLevel.Error, this.ErrorAnchor, $"Could not parse period {nodes[1].Token.Value} bad format.", null);
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
                return $"duration {name} {Value};\n";
            }
        }
    }
}
