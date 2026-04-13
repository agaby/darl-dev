/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    /// Used to determine how the attached ruleset is generated
    /// </summary>
    class MineTypeNode : DarlNode
    {

        /// Gets the type of the mine.
        /// </summary>
        /// <value>
        /// The type of the mine.
        /// </value>
        public string mineType { get; private set; }


        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            mineType = nodes[0].Token.Text;
        }

    }
}
