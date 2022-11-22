using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
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
            Prologue(thread);
            var res = new DarlResult("seek", DarlResult.DataType.seek);
            res.sequence = new List<List<string>> { new List<string> { objLineage } };
            res.SetWeight(1.0);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
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
