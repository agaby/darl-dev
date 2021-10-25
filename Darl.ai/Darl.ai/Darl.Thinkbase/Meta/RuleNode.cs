using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class RuleNode : DarlMetaNode
    {
        /// <summary>
        /// Gets the rule output or store sink.
        /// </summary>
        /// <value>
        /// The rule output or store sink.
        /// </value>
        public DarlMetaNode ruleOutput { get; protected set; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        public DarlMetaNode conditions { get; protected set; }

        /// <summary>
        /// Gets the RHS.
        /// </summary>
        /// <value>
        /// The RHS.
        /// </value>
        public DarlMetaNode rhs { get; set; }

        /// <summary>
        /// Gets the confidence node.
        /// </summary>
        /// <value>
        /// The confidence node.
        /// </value>
        public ConfidenceNode confidenceNode { get; protected set; }
        public LifetimeNode lifetimeNode { get; protected set; }

        private int writeSeq = 0;

        public bool IsUnknown { get; internal set; }

        /// <summary>
        /// Gets the rule output or store sink.
        /// </summary>
        /// <value>
        /// The rule output or store sink.
        /// </value>



        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            conditions = AddChild("-", nodes[0]) as DarlMetaNode;
            ruleOutput = AddChild("-", nodes[1]) as DarlMetaNode;
            rhs = AddChild("-", nodes[2]) as DarlMetaNode;
            var decorationNode = nodes.Count > 3 ? AddChild("-", nodes[3]) as DecorationNode : null;
            if (decorationNode != null)
            {
                confidenceNode = decorationNode.confidence ?? new ConfidenceNode();
                lifetimeNode = decorationNode.lifetime ?? new LifetimeNode();
            }
            else
            {
                confidenceNode = new ConfidenceNode();
                lifetimeNode = new LifetimeNode();
            }
            AsString = "Rule";
            ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
            IsUnknown = true;
            if (ruleOutput is StoreNode)
            {
                ((StoreNode)ruleOutput).storeType = StoreNode.StoreType.sink;
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
            if (currentOutput is StoreNode)
            {
                var c = currentOutput as StoreNode;
                if (context.stores.ContainsKey(c.Left.name))
                {
                    c.storeDefinition = context.stores[c.Left.name];
                }
            }
            conditions.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
            context.controllingIO = currentOutput.GetName();
            rhs.WalkDependencies(dependencies, currentOutput, context, model, currentNode);
            if (currentOutput is StoreNode)
            {
                if (!context.storeOutputs.ContainsKey(currentOutput.GetName()))
                {
                    context.storeOutputs.Add(currentOutput.GetName(), currentOutput as StoreNode);
                }
            }
        }

        public override string GetName()
        {
            if (ruleOutput is DarlMetaIdentifierNode)
            {
                return ((DarlMetaIdentifierNode)ruleOutput).name;
            }
            else
            {
                return ((StoreNode)ruleOutput).GetName();
            }
        }

        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "\tif ";
            }
        }

        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                switch (writeSeq)
                {
                    case 0:
                        writeSeq++;
                        return "then ";
                    case 1:
                        writeSeq++;
                        return "will be ";
                    default:
                        writeSeq++;
                        return "";
                }
            }
        }

        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public override string postamble
        {
            get
            {
                writeSeq = 0;
                return ";\n";
            }
        }

        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlResult condition = (DarlResult)await conditions.Evaluate(thread);
            if (condition.IsUnknown())
            {
                thread.CurrentNode = Parent; //standard epilogue
                return condition; // if condition part unknown don't continue.
            }
            if ((double)condition.values[0] == 0.0)
            {
                thread.CurrentNode = Parent; //standard epilogue
                IsUnknown = false;
                return new DarlResult(-1.0, true); // if condition part unknown don't continue.
            }
            if (ruleOutput is StoreNode)
            {
                await ruleOutput.Evaluate(thread); //sets the address to receive the result
            }
            DarlResult result = (DarlResult)await rhs.Evaluate(thread);
            result.Normalise(true);
            if (result.IsUnknown())
                return new DarlResult(-1.0, true); // if result part unknown don't continue.
            DarlResult confidence = (DarlResult)await confidenceNode.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            var r = new DarlResult(condition, result, confidence);
            IsUnknown = false;
            return r;
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            conditions.WalkSaliences(saliency, root);
            rhs.WalkSaliences(saliency, root);
        }
    }
}