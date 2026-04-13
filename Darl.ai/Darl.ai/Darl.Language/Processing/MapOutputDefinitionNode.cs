/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    /// Implements a map output definition
    /// </summary>
    public class MapOutputDefinitionNode : DarlNode
    {
        /// Gets the name of the output.
        /// </summary>
        /// <value>
        /// The name of the output.
        /// </value>
        public string Name { get; set; }
        /// Gets the path of the output.
        /// </summary>
        /// <value>
        /// The path of the output.
        /// </value>
        public string Path { get; private set; }

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Name = (string)nodes[0].Token.Value;
            if (nodes.Count == 2)
                Path = (string)nodes[1].Token.Value;
        }

        /// prototype output for GP
        /// </summary>
        public OutputDefinitionNode outputPrototype { get; set; }

        public override string preamble
        {
            get
            {
                return $"mapoutput {Name};";
            }
        }

    }

}
