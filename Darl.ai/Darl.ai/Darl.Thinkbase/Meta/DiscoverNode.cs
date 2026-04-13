/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DiscoverNode : LineageMetaNode
    {
        readonly List<string> lineageStrings = new List<string>();
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            if (treeNode.ChildNodes[0].ChildNodes.Count < 1)
            {
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.ChildNodes[0].FindToken().Location, $"Discover takes one or more connection lineages to determine which connections to discover through.");
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
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var res = new DarlResult("discover", DarlResult.DataType.discover);
            res.sequence = new List<List<string>> { lineageStrings };
            res.SetWeight(1.0);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {
            foreach (var l in lineages)
            {
                lineageStrings.Add(l is LineageLiteral ? ((LineageLiteral)l).literal : (context.lineages.ContainsKey(l.GetName()) ? context.lineages[l.GetName()].Value : ""));
            }
        }

        public override string preamble
        {
            get
            {
                return "discover( ";
            }
        }

        public override string postamble
        {
            get
            {
                return ")";
            }
        }
    }
}
