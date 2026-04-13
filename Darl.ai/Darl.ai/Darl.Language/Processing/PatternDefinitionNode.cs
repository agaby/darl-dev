/// <summary>
/// PatternDefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Contains the optional pattern navigation string
    /// </summary>
    public class PatternDefinitionNode : DarlNode
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Value = (string)nodes[0].Token.Value;
        }

    }
}
