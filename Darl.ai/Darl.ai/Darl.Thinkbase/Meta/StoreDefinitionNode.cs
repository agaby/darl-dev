/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace Darl.Thinkbase.Meta
{
    public class StoreDefinitionNode : DarlMetaNode
    {
        public string name { get; private set; }
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            name = nodes[0].Token.Text;
        }
    }
}