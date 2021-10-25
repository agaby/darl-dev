using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    public class AnyIOListNode : DarlNode
    {

        /// <summary>
        /// The list of arguments
        /// </summary>
        protected List<DarlNode> arguments = new List<DarlNode>();

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var node in nodes)
            {
                arguments.Add((DarlNode)AddChild("-", node));
            }
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            foreach (var node in arguments)
                node.WalkDependencies(dependencies, currentOutput, context);
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            foreach (var node in arguments)
                node.WalkSaliences(saliency, root, currentRuleSet, currentOutput);

        }

        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            var results = new Dictionary<string, string>();
            foreach(DarlIdentifierNode arg in arguments)
            {
                var res = await arg.Evaluate(thread) as DarlResult;
                if(!results.ContainsKey(arg.name)) //ignore duplicates
                    results.Add(arg.name, res.ToStringContent());
            }
            return results;
        }

        public override string preamble
        {
            get
            {
                return "{ ";
            }
        }

        public override string midamble
        {
            get
            {
                return ", ";
            }
        }

        public override string postamble
        {
            get
            {
                return "} ";
            }
        }
    }
}
