/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Thinkbase.Meta
{
    public class LineageMetaNode : DarlMetaNode
    {

        protected List<DarlMetaNode> lineages = new List<DarlMetaNode>();
        protected string connLineage;
        protected string objLineage;
        protected string attLineage;

        protected List<IntraSetDependency> setDependencyList = new List<IntraSetDependency> { };


        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[0].ChildNodes.Count != 3)
            {
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.ChildNodes[0].FindToken().Location, $"This operator takes 3 lineages: the source, the connection and the attribute.");
            }
            var nodes = treeNode.GetMappedChildNodes();
            if (nodes.Count == 1)
            {
                foreach (var node in nodes[0].ChildNodes)
                {
                    lineages.Add((DarlMetaNode)AddChild("-", node));
                }
            }
        }

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            if (lineages.Any())
            {
                objLineage = lineages[0] is LineageLiteral ? ((LineageLiteral)lineages[0]).literal : (context.lineages.ContainsKey(lineages[0].GetName()) ? context.lineages[lineages[0].GetName()].Value : null);
                if (objLineage != null && lineages.Count == 3 && currentNode != null)
                {
                    connLineage = lineages[1] is LineageLiteral ? ((LineageLiteral)lineages[1]).literal : context.lineages[lineages[1].GetName()].Value;
                    attLineage = lineages[2] is LineageLiteral ? ((LineageLiteral)lineages[2]).literal : context.lineages[lineages[2].GetName()].Value;
                    foreach (var l in model.GetConnectedObjects(currentNode, connLineage, objLineage))
                    {
                        var isd = new IntraSetDependency { dependentObject = l, output = currentOutput.GetName(), attributeLineage = attLineage };
                        dependencies.Add(isd);
                        setDependencyList.Add(isd); //keep a local record
                    }
                }
            }
        }

        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            foreach (var isd in setDependencyList)
            {
                root.inputs.TryAdd(isd.dependentObject.externalId, new InputDefinitionNode { Salience = saliency, lineage = isd.attributeLineage, name = isd.dependentObject.externalId, networkNode = new NetworkComponentNode { lineage = isd.attributeLineage, nodeId = isd.dependentObject.id } });
            }
        }
    }
}
