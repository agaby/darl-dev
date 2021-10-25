using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace Darl.Thinkbase.Meta
{
    public abstract class MultipleDarlMetaNode : DarlMetaNode
    {
        protected List<DarlMetaNode> arguments = new List<DarlMetaNode>();

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            if (nodes.Count == 1)
            {
                foreach (var node in nodes[0].ChildNodes)
                {
                    arguments.Add((DarlMetaNode)AddChild("-", node));
                }
            }
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            foreach (var node in arguments)
                node.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            foreach (var node in arguments)
                node.WalkSaliences(saliency, root);
        }
    }
}