/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    public class MapStoreDefinitionNode : DarlNode
    {
        /// Gets the name of the output.
        /// </summary>
        /// <value>
        /// The name of the output.
        /// </value>
        public string Name { get; set; }

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Name = (string)nodes[0].Token.Value;
        }
    }
}