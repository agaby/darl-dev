using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DecorationNode : DarlMetaNode
    {
        public ConfidenceNode confidence;

        public LifetimeNode lifetime;
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var node in nodes)
            {
                var a = (DarlMetaNode)AddChild("-", node);
                switch (a.AsString)
                {
                    case "lifetime":
                        lifetime = a as LifetimeNode;
                        break;
                    case "confidence":
                        confidence = a as ConfidenceNode;
                        break;
                }
            }
        }

        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return base.DoEvaluate(thread);
        }

        public override string preamble => base.preamble;
    }
}
