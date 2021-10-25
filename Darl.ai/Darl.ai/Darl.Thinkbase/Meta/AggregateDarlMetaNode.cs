using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace Darl.Thinkbase.Meta
{
    public abstract class AggregateDarlMetaNode : MultipleDarlMetaNode
    {
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            var nodes = treeNode.GetMappedChildNodes();
            if (nodes.Count == 1)
            {
                if (nodes[0].AstNode is AttributesNode)
                {
                    arguments.Add((DarlMetaNode)AddChild("-", nodes[0]));
                }
                else
                {
                    foreach (var node in nodes[0].ChildNodes)
                    {
                        arguments.Add((DarlMetaNode)AddChild("-", node));
                    }
                }
            }
        }
    }
}
