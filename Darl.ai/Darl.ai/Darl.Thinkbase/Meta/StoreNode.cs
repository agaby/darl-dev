/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Thinkbase.Meta
{
    public class StoreNode : DarlMetaNode
    {
        public enum StoreType { sink, source }
        /// <summary>
        /// The left side
        /// </summary>
        public DarlMetaIdentifierNode Left { get; private set; }

        protected List<DarlMetaNode> arguments = new List<DarlMetaNode>();


        public List<string> address { get; set; } = new List<string>();

        public StoreDefinitionNode storeDefinition { get; set; }

        readonly int writeIndex = 0;

        /// <summary>
        /// Data direction
        /// </summary>
        public StoreType storeType { get; set; } = StoreType.source;
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlMetaIdentifierNode)AddChild("-", nodes[0]);
            foreach (var node in nodes[1].ChildNodes)
            {
                arguments.Add((DarlMetaNode)AddChild("-", node));
            }
        }

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            if (context.stores.ContainsKey(Left.name))
            {
                storeDefinition = context.stores[Left.name];
            }
            if (!context.storeInputs.ContainsKey(GetName()))
            {
                context.storeInputs.Add(GetName(), this);
            }
            foreach (var node in arguments)
                node.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
            Left.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
        }

        /// <summary>
        /// return the local name
        /// </summary>
        /// <returns></returns>
        public override string GetName()
        {
            return $"{Left.GetName()}.{string.Join("_", arguments.Select(x => x.GetName()))}";
        }
    }
}