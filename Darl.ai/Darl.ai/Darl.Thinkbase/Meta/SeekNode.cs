using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class SeekNode : LineageMetaNode
    {

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            if (treeNode.ChildNodes[0].ChildNodes.Count < 1)
            {
                context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.ChildNodes[0].FindToken().Location, $"Seek takes one or more connection lineages to determine which connections to seek through.");
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
            var res =  new DarlResult("seek", DarlResult.DataType.seek);
            res.sequence = new List<List<string>> { new List<string> { objLineage} };
            res.SetWeight(1.0);
            return Task.FromResult<object>(res);
        }
    }
}
