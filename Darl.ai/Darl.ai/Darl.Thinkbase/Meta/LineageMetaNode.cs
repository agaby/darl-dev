using DarlCompiler.Ast;
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


        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[0].ChildNodes.Count != 3)
            {
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.ChildNodes[0].FindToken().Location, $"This operator takes 3 lineages, either the source and the attribute, or the source, the connection and the attribute.");
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

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
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
                        dependencies.Add(new IntraSetDependency { dependentObject = l, output = currentOutput.GetName(), attributeLineage = attLineage });
                    }
                }
            }
        }
    }
}
