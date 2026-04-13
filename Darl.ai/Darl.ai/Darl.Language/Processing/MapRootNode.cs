/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// THe root node of the evaluation tree
    /// </summary>
    public class MapRootNode : DarlNode
    {
        /// <summary>
        /// The rulesets
        /// </summary>
        public Dictionary<string, RuleSetNode> rulesets = new Dictionary<string, RuleSetNode>();
        /// <summary>
        /// The inputs
        /// </summary>
        public Dictionary<string, MapInputDefinitionNode> inputs = new Dictionary<string, MapInputDefinitionNode>();
        /// <summary>
        /// The outputs
        /// </summary>
        public Dictionary<string, MapOutputDefinitionNode> outputs = new Dictionary<string, MapOutputDefinitionNode>();


        public Dictionary<string, MapStoreDefinitionNode> stores = new Dictionary<string, MapStoreDefinitionNode>();
        /// <summary>
        /// The wires
        /// </summary>
        public List<WireDefinitionNode> wires = new List<WireDefinitionNode>();

        /// <summary>
        /// The delays{CC2D43FA-BBC4-448A-9D0B-7B57ADF2655C}
        /// </summary>
        public List<DelayDefinitionNode> delays = new List<DelayDefinitionNode>();

        /// <summary>
        /// The ordered rulesets
        /// </summary>
        public List<RuleSetNode> orderedRulesets;

        /// <summary>
        /// Gets the pattern.
        /// </summary>
        /// <value>
        /// The pattern.
        /// </value>
        public string pattern { get; private set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        /// <exception cref="RuleException"></exception>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                var childAst = AddChild("-", child);
                if (childAst is RuleSetNode)
                    rulesets.Add(((RuleSetNode)child.AstNode).rulesetname, (RuleSetNode)child.AstNode);
                else if (childAst is PatternDefinitionNode)
                    pattern = ((PatternDefinitionNode)childAst).Value;
                else if (childAst is MapInputDefinitionNode)
                    inputs.Add(((MapInputDefinitionNode)child.AstNode).Name, (MapInputDefinitionNode)child.AstNode);
                else if (childAst is MapOutputDefinitionNode)
                    outputs.Add(((MapOutputDefinitionNode)child.AstNode).Name, (MapOutputDefinitionNode)child.AstNode);
                else if (childAst is MapStoreDefinitionNode)
                    stores.Add(((MapStoreDefinitionNode)child.AstNode).Name, (MapStoreDefinitionNode)child.AstNode);
                else if (childAst is WireDefinitionNode)
                    wires.Add((WireDefinitionNode)child.AstNode);
                else if (childAst is DelayDefinitionNode)
                    delays.Add((DelayDefinitionNode)child.AstNode);
            }
            // now determine the order of ruleset execution and detect loops
            // Only wires of wiretype wireinternal count.
            List<WireDefinitionNode> currentWires = new List<WireDefinitionNode>();
            orderedRulesets = new List<RuleSetNode>(rulesets.Values);
            foreach (var wire in wires)
            {
                if (wire.wiretype == WireDefinitionNode.WireType.wireinternal)
                {
                    currentWires.Add(wire);
                    if (orderedRulesets.Contains(rulesets[wire.destRuleset]))
                        orderedRulesets.Remove(rulesets[wire.destRuleset]);
                }
            }
            List<RuleSetNode> remainder = new List<RuleSetNode>(rulesets.Values.Where(a => !orderedRulesets.Contains(a)));
            //now the currentwires contains all internal wires and orderedrulesets contains all rulesets not dependant on other rule sets, while remainder contains the rest.
            while (remainder.Count > 0)//check dependencies and remove orphan wires
            {
                List<WireDefinitionNode> deletionList = new List<WireDefinitionNode>();
                foreach (var wire in currentWires)
                {
                    if (!remainder.Contains(rulesets[wire.sourceRuleset])) //wire is orphaned
                    {
                        deletionList.Add(wire);
                    }
                }
                foreach (var wire in deletionList)
                {
                    currentWires.Remove(wire);
                }
                var newRemainder = new List<RuleSetNode>();
                foreach (var wire in currentWires)//get list of rule sets still accessed by wires
                {
                    newRemainder.Add(rulesets[wire.destRuleset]);
                }
                foreach (var rset in remainder)
                {
                    if (!newRemainder.Contains(rset))
                        orderedRulesets.Add(rset); //it's the next in the execution sequence...
                }
                remainder = newRemainder;
                if (deletionList.Count == 0 && remainder.Count > 0)//no wires removed, but rule sets left, must be a loop.
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Loop found between rulesets involving the following: ");
                    foreach (var r in remainder)
                    {
                        sb.AppendFormat("rule set: {0}, ", r.rulesetname);
                    }
                    throw new RuleException(sb.ToString());
                }
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
            DarlGrammar grammar = thread.Runtime.Language.Grammar as DarlGrammar;
            // use wires to copy source values
            foreach (var wire in wires)
            {
                if (wire.wiretype == WireDefinitionNode.WireType.wirein || wire.wiretype == WireDefinitionNode.WireType.wirethrough)
                {
                    if (grammar.results.Any(a => a.name == wire.sourcename))
                    {
                        grammar.results.RemoveAll(a => a.name == wire.compDest);
                        grammar.results.Add(new DarlResult(wire.compDest, grammar.ResultByName(wire.compSource)));
                    }
                }
                else if (wire.wiretype == WireDefinitionNode.WireType.wirestore)
                {
                    if (grammar.stores.ContainsKey(wire.destname))
                    {
                        try
                        {
                            rulesets[wire.sourceRuleset].ruleRoot.stores[wire.sourcename].storeInterface = grammar.stores[wire.destname];
                        }
                        catch
                        {
                            //allow bad connections to fail.
                        }
                    }

                }
            }
            //handle delays
            foreach (var delay in delays)
                await delay.Evaluate(thread);

            foreach (var rs in orderedRulesets)
            {
                await rs.Evaluate(thread);
                // copy relevant internal and out values
                foreach (var wire in wires.Where(a => a.sourceRuleset == rs.rulesetname && (a.wiretype == WireDefinitionNode.WireType.wireinternal || a.wiretype == WireDefinitionNode.WireType.wireout)))
                {
                    grammar.results.RemoveAll(a => a.name == wire.compDest);
                    var srcVal = grammar.ResultByName(wire.compSource);
                    if (((object)srcVal) != null)
                    {
                        DarlResult newval = new DarlResult(srcVal)
                        {
                            name = wire.compDest,
                            Value = srcVal.Value
                        };
                        if (!grammar.results.Any(a => a.name == wire.compDest))
                            grammar.results.Add(newval);
                    }
                }
            }
            //handle delays
            foreach (var delay in delays)
                await delay.Evaluate(thread);
            thread.CurrentNode = Parent; //standard epilogue
            return null;
        }

        /// <summary>
        /// Calculates the saliences.
        /// </summary>
        /// <param name="currentState">Current state of the inputs or outputs.</param>
        /// <returns>A collection of inputs not yet satisfied, but required given the current state.</returns>
        internal Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState)
        {
            var unsatisfiedInputSaliences = new Dictionary<string, double>();
            foreach (MapInputDefinitionNode input in inputs.Values) //clear saliences
                input.Salience = 0.0;
            foreach (string outName in outputs.Keys)
            {
                if (IsUnfilled(currentState, outName)) //output is unknown
                {
                    foreach (WireDefinitionNode wire in wires.Where(a => a.destname == outName && string.IsNullOrEmpty(a.destRuleset))) //wires connecting to that output
                    {
                        wire.WalkSaliences(1.0, this, string.Empty, string.Empty);
                    }
                }
            }
            foreach (string outName in stores.Keys)
            {
                foreach (WireDefinitionNode wire in wires.Where(a => a.destname == outName && string.IsNullOrEmpty(a.destRuleset))) //wires connecting to that store
                {
                    wire.WalkSaliences(1.0, this, string.Empty, string.Empty);
                }
            }
            foreach (MapInputDefinitionNode input in inputs.Values.Where(a => a.Salience > 0.0 && IsUnfilled(currentState, a.Name)))
            {
                unsatisfiedInputSaliences.Add(input.Name, input.Salience);
            }
            return unsatisfiedInputSaliences;
        }

        private static bool IsUnfilled(List<DarlResult> currentState, string name)
        {
            return !currentState.Any(a => a.name == name) || currentState.Any(a => a.name == name) && (currentState.First(a => a.name == name).IsUnknown() || currentState.First(a => a.name == name).GetWeight() < 1.0);
        }

        internal DarlNode NavigateSource(string sourcename, string sourceruleset)
        {
            if (string.IsNullOrEmpty(sourceruleset)) //must be a mapinput
                return inputs[sourcename];
            else
                return rulesets[sourceruleset];
        }
        internal DarlNode NavigateDest(string destname, string destruleset)
        {
            return wires.Where(a => a.destRuleset == destruleset && a.destname == destname).FirstOrDefault();
        }

        internal DarlNode NavigateInternal(string destname, string destruleset)
        {
            if (rulesets.ContainsKey(destruleset))
            {
                var rs = rulesets[destruleset];
                if (rs.ruleRoot != null)
                {
                    return rs.ruleRoot;
                }
            }
            return null;
        }

        public override string midamble
        {
            get
            {
                return "\n";
            }
        }

    }
}
