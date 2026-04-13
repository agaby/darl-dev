/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a rule set body
    /// </summary>
    public class RuleSetNode : DarlNode
    {
        /// <summary>
        /// The type of processing to apply
        /// </summary>
        public enum ProcessType
        {
            /// <summary>
            /// manual rule creation (default)
            /// </summary>
            manual,
            /// <summary>
            /// supervised rule creation
            /// </summary>
            supervised,
            /// <summary>
            /// unsupervised rule creation
            /// </summary>
            unsupervised,
            /// <summary>
            /// reinforcement rule creation
            /// </summary>
            reinforcement,
            /// <summary>
            /// Association rule creation
            /// </summary>
            association
        };

        /// <summary>
        /// Gets the type of the ruleset.
        /// </summary>
        /// <value>
        /// The type of the ruleset.
        /// </value>
        public ProcessType pType { get; private set; }

        /// <summary>
        /// Gets the rulesetname.
        /// </summary>
        /// <value>
        /// The rulesetname.
        /// </value>
        public string rulesetname { get; private set; }

        /// <summary>
        /// Gets the rule root.
        /// </summary>
        /// <value>
        /// The rule root.
        /// </value>
        public RuleRootNode ruleRoot { get; private set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            var rsn = AddChild("-", nodes[0]);
            var r1 = AddChild("-", nodes[1]);
            pType = ProcessType.manual;
            if (r1 is RuleRootNode)
            {
                ruleRoot = (RuleRootNode)r1;
            }
            else
            {
                ProcessType p;
                if (Enum.TryParse<ProcessType>(r1.Term.Name, out p))
                    pType = p;
                ruleRoot = (RuleRootNode)AddChild("-", nodes[2]);
            }
            rulesetname = nodes[0].Token.ValueString;
            ruleRoot.ruleset = rulesetname;
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
            await ruleRoot.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return null;
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
                return $"ruleset {rulesetname}{(pType == ProcessType.manual ? string.Empty : " " + pType.ToString())}\n{{\n";
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
                return "}\n";
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
            ruleRoot.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
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
                return "<span class=\"text-primary\">ruleset </span> " + rulesetname + (pType == ProcessType.manual ? string.Empty : " <span class=\"text-info\">" + pType.ToString() + "</span>") + "<br/><span class=\"text-primary\">{</span><br/>";
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
                return "<span class=\"text-primary\">}</span><br/>";
            }
        }
    }
}
