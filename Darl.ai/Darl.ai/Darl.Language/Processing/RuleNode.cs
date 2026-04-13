/// <summary>
/// RuleNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements the root of a rule
    /// </summary>
    public class RuleNode : DarlNode
    {
        private int writeSeq = 0;

        public bool IsUnknown { get; internal set; }

        /// <summary>
        /// Gets the rule output or store sink.
        /// </summary>
        /// <value>
        /// The rule output or store sink.
        /// </value>
        public DarlNode ruleOutput { get; protected set; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        public DarlNode conditions { get; protected set; }

        /// <summary>
        /// Gets the RHS.
        /// </summary>
        /// <value>
        /// The RHS.
        /// </value>
        public DarlNode rhs { get; set; }

        /// <summary>
        /// Gets the confidence node.
        /// </summary>
        /// <value>
        /// The confidence node.
        /// </value>
        public ConfidenceNode confidenceNode { get; protected set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            conditions = AddChild("-", nodes[0]) as DarlNode;
            ruleOutput = AddChild("-", nodes[1]) as DarlNode;
            rhs = AddChild("-", nodes[2]) as DarlNode;
            confidenceNode = AddChild("-", nodes[3]) as ConfidenceNode;
            AsString = "Rule";
            ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
            IsUnknown = true;
            if (ruleOutput is StoreNode)
            {
                ((StoreNode)ruleOutput).storeType = StoreNode.StoreType.sink;
            }
        }

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
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
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            if (currentOutput is StoreNode)
            {
                var c = currentOutput as StoreNode;
                if (context.stores.ContainsKey(c.Left.name))
                {
                    c.storeDefinition = context.stores[c.Left.name];
                }
            }
            conditions.WalkDependencies(dependencies, currentOutput, context);
            context.controllingIO = currentOutput.GetName();
            rhs.WalkDependencies(dependencies, currentOutput, context);
            if (currentOutput is StoreNode)
            {
                if (!context.storeOutputs.ContainsKey(currentOutput.GetName()))
                {
                    context.storeOutputs.Add(currentOutput.GetName(), currentOutput as StoreNode);
                }
            }
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
            conditions.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
            rhs.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
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

        /// <summary>
        /// Gets the HTML postamble.
        /// </summary>
        /// <value>
        /// The HTML postamble.
        /// </value>
        public override string htmlPostamble
        {
            get
            {
                return "<span class=\"text-primary\">;</span><br/>";
            }
        }

        /// <summary>
        /// Gets the HTML preamble.
        /// </summary>
        /// <value>
        /// The HTML preamble.
        /// </value>
        public override string htmlPreamble
        {
            get
            {
                return "<span class=\"text-primary\">if </span>";
            }
        }

        /// <summary>
        /// Gets the HTML midamble.
        /// </summary>
        /// <value>
        /// The HTML midamble.
        /// </value>
        public override string htmlMidamble
        {
            get
            {
                return "<span class=\"text-primary\">" + midamble + "</span>";
            }
        }

        public override string GetName()
        {
            if (ruleOutput is DarlIdentifierNode)
            {
                return ((DarlIdentifierNode)ruleOutput).name;
            }
            else
            {
                return ((StoreNode)ruleOutput).GetName();
            }
        }
    }
}
