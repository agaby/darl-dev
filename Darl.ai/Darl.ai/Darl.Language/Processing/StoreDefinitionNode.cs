/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace DarlLanguage.Processing
{
    public class StoreDefinitionNode : DarlNode
    {
        /// Gets the name of the string.
        /// </summary>
        /// <value>
        /// The name of the string.
        /// </value>
        public string name { get; private set; }

        public ILocalStore storeInterface { get; set; }

        /// store address combinations in use as outputs of rules for salience calcs
        /// </summary>
        public HashSet<string> storeOutputs { get; set; } = new HashSet<string>();

        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
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
                return $"store {name};\n";
            }
        }
    }
}