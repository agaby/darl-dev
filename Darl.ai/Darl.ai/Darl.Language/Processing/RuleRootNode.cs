using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a set of rules
    /// </summary>
    public class RuleRootNode : DarlNode
    {

        /// <summary>
        /// The outputs
        /// </summary>
        public Dictionary<string, IOSequenceDefinitionNode> outputs = new Dictionary<string, IOSequenceDefinitionNode>();

        /// <summary>
        /// The inputs
        /// </summary>
        public Dictionary<string, InputDefinitionNode> inputs = new Dictionary<string, InputDefinitionNode>();

        /// <summary>
        /// The strings
        /// </summary>
        public Dictionary<string, StringDefinitionNode> strings = new Dictionary<string, StringDefinitionNode>();

        /// <summary>
        /// The constants
        /// </summary>
        public Dictionary<string, ConstantDefinitionNode> constants = new Dictionary<string, ConstantDefinitionNode>();

        /// <summary>
        /// The periods
        /// </summary>
        public Dictionary<string, DurationDefinitionNode> durations = new Dictionary<string, DurationDefinitionNode>();


        /// <summary>
        /// The sequences
        /// </summary>
        public Dictionary<string, SequenceDefinitionNode> sequences = new Dictionary<string, SequenceDefinitionNode>();

        /// <summary>
        /// The stores
        /// </summary>
        public Dictionary<string, StoreDefinitionNode> stores = new Dictionary<string, StoreDefinitionNode>();

        /// <summary>
        /// The rules
        /// </summary>
        public Dictionary<string, List<RuleNode>> rules = new Dictionary<string, List<RuleNode>>();


        public Dictionary<string, StoreNode> storeInputs = new Dictionary<string, StoreNode>();

        /// <summary>
        /// Gets or sets the ruleset name.
        /// </summary>
        /// <value>
        /// The ruleset name.
        /// </value>
        public string ruleset { get; set; }

        /// <summary>
        /// The ordered outputs in execution order
        /// </summary>
        List<IOSequenceDefinitionNode> orderedOutputs;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        /// <exception cref="DarlLanguage.Processing.RuleException">Loop found in rules:  + sb.ToString()</exception>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                if (child.AstNode is InputDefinitionNode)
                    inputs.Add(((InputDefinitionNode)child.AstNode).name, (InputDefinitionNode)child.AstNode);
                else if (child.AstNode is OutputDefinitionNode)
                    outputs.Add(((OutputDefinitionNode)child.AstNode).name, (IOSequenceDefinitionNode)child.AstNode);
                else if (child.AstNode is StringDefinitionNode)
                    strings.Add(((StringDefinitionNode)child.AstNode).name, (StringDefinitionNode)child.AstNode);
                else if (child.AstNode is ConstantDefinitionNode)
                    constants.Add(((ConstantDefinitionNode)child.AstNode).name, (ConstantDefinitionNode)child.AstNode);
                else if (child.AstNode is SequenceDefinitionNode)
                    sequences.Add(((SequenceDefinitionNode)child.AstNode).name, (SequenceDefinitionNode)child.AstNode);
                else if (child.AstNode is DurationDefinitionNode)
                    durations.Add(((DurationDefinitionNode)child.AstNode).name, (DurationDefinitionNode)child.AstNode);
                else if (child.AstNode is StoreDefinitionNode)
                    stores.Add(((StoreDefinitionNode)child.AstNode).name, (StoreDefinitionNode)child.AstNode);

                else if (child.AstNode is RuleNode)
                {
                    AddChild("-", child);
                    string name = ((RuleNode)child.AstNode).GetName();
                    if (name.Contains("."))//if output is a store
                    {
                        var storename = name.Split(new Char[] { '.' }).First(); //take the store name part
                        if (stores.ContainsKey(storename)) //always should
                        {
                            stores[storename].storeOutputs.Add(name); //record all store/address combinations used as outputs
                        }
                    }
                    if (!rules.ContainsKey(name))
                    {
                        rules.Add(name, new List<RuleNode>());
                    }
                    rules[name].Add((RuleNode)child.AstNode);
                }
            }
            AsString = "Rule root";
            if (ChildNodes.Count == 0)
            {
                AsString += " (No rules)";
            }
            else
                ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
            //determine sequence - report loops
            //get dependency tree
            List<IntraSetDependency> dependencies = new List<IntraSetDependency>();
            ConstantContext ccontext = new ConstantContext { inputs = inputs, outputs = outputs, constants = constants, strings = strings, sequences = sequences, controllingIO = string.Empty, storeInputs = storeInputs, storeOutputs = new Dictionary<string, StoreNode>(), stores = stores, durations = durations };
            foreach (string outName in rules.Keys)
            {
                foreach (RuleNode rule in rules[outName])
                {
                    rule.WalkDependencies(dependencies, rule.ruleOutput, ccontext);
                }
            }
            //Add all stores as outputs found to the list of outputs.
            foreach (var s in ccontext.storeOutputs.Keys)
            {
                outputs.Add(s, ccontext.storeOutputs[s]);
            }
            //all inputs are initially set to sequence 0
            int currentSequence = 1;
            bool complete = false;
            while (!complete)
            {
                List<IntraSetDependency> deletions = new List<IntraSetDependency>();
                foreach (IntraSetDependency dep in dependencies)
                {
                    outputs[dep.output].sequence = currentSequence;
                    //if outputasinput does not match any outputs
                    //remove that link
                    bool match = false;
                    foreach (IntraSetDependency otherDep in dependencies)
                    {
                        if (otherDep.output == dep.outputAsInput)
                            match = true;
                    }
                    if (!match)
                        deletions.Add(dep);
                }
                if (deletions.Count == 0 && dependencies.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IntraSetDependency dep in dependencies)
                        sb.Append(dep.ToString() + ", ");
                    throw new RuleException("Loop found in rules: " + sb.ToString());
                }
                foreach (IntraSetDependency del in deletions)
                {
                    dependencies.Remove(del);
                }
                currentSequence++;
                complete = dependencies.Count == 0;
            }
            orderedOutputs = new List<IOSequenceDefinitionNode>(outputs.Values);
            orderedOutputs.Sort();
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
            foreach (InputDefinitionNode input in inputs.Values)
            {
                string compName = ruleset + "." + input.name;
                input.Value = grammar.ResultByName(compName);
                if (((object)input.Value) != null)
                {
                    //sanity checks
                    if (input.iType == InputDefinitionNode.InputTypes.arity_input || input.iType == InputDefinitionNode.InputTypes.numeric_input && !input.Value.IsNumeric())
                    {
                        //attempt conversion
                        bool success = true;
                        var vals = new List<double>();
                        foreach (string cat in input.Value.categories.Keys)
                        {
                            if (input.sets.ContainsKey(cat)) // handle set name returned instead of double
                            {
                                if (input.sets[cat].leftUnbounded || input.sets[cat].rightUnbounded)
                                {
                                    vals.Add((double)input.sets[cat].values[1]);
                                }
                                else
                                {
                                    foreach (double d in input.sets[cat].values)
                                        vals.Add(d);
                                }
                                success = true;
                                break;
                            }
                            else
                            {
                                try
                                {
                                    vals.Add(Convert.ToDouble(cat));
                                }
                                catch
                                {
                                    success = false;
                                }
                            }
                        }
                        if (!success)
                            input.Value = new DarlResult(0.0, false);
                        else
                        {
                            input.Value = new DarlResult(vals);
                        }
                    }
                }
                else
                {
                    input.Value = new DarlResult(input.name, 0.0, true);
                }
                var binding = thread.Bind(ruleset + "." + input.name, DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                await binding.SetValueRef(thread, input.Value);
            }
            //Add all store 
            foreach (var s in storeInputs.Values)
            {
                var binding = thread.Bind(s.GetCompName(), DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                await binding.SetValueRef(thread, s.Value);
            }
            foreach (IOSequenceDefinitionNode output in outputs.Values)
            {
                var binding = thread.Bind(ruleset + "." + output.GetName(), DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                await binding.SetValueRef(thread, output.result);
            }
            //perform the processing
            foreach (IOSequenceDefinitionNode outNode in orderedOutputs)
            {
                if (rules.ContainsKey(outNode.GetName()) && outNode is OutputDefinitionNode)
                {
                    string compName = ruleset + "." + outNode.name;
                    var ty = ConvertOType(((OutputDefinitionNode)outNode).iType);
                    DarlResult result = new DarlResult(compName, ty);
                    DarlResult other = new DarlResult(compName, ty);
                    foreach (RuleNode rnode in rules[outNode.name])
                    {
                        if (rnode is OtherwiseNode)
                            other.FuzzyAggregate((DarlResult)await rnode.Evaluate(thread));
                        else
                            result.FuzzyAggregate((DarlResult)await rnode.Evaluate(thread));
                    }
                    result.Simplify(outNode, other);

                    grammar.results.RemoveAll(a => a.name == compName);
                    grammar.results.Add(result);
                    outNode.result = result;
                    var binding = thread.Bind(compName, DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                    await binding.SetValueRef(thread, outNode.result);
                }
                //handle stores as output here.
                if (rules.ContainsKey(outNode.GetName()) && outNode is StoreNode)
                {
                    string compName = ruleset + "." + outNode.GetName();
                    DarlResult result = new DarlResult(compName, true, 0.0);
                    DarlResult other = new DarlResult(compName, true, 0.0);
                    foreach (RuleNode rnode in rules[outNode.GetName()])
                    {
                        if (rnode is OtherwiseNode)
                            other.FuzzyAggregate((DarlResult)await rnode.Evaluate(thread));
                        else
                            result.FuzzyAggregate((DarlResult)await rnode.Evaluate(thread));
                    }
                    result.PolymorphicSimplify(outNode, other);

                    grammar.results.RemoveAll(a => a.name == compName);
                    grammar.results.Add(result);
                    outNode.result = result;
                    var binding = thread.Bind(compName, DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                    await binding.SetValueRef(thread, outNode.result);
                }
            }
            thread.CurrentNode = Parent; //standard epilogue
            return null;
        }

        private DarlResult.DataType ConvertOType(OutputDefinitionNode.OutputTypes otype)
        {
            switch (otype)
            {
                case OutputDefinitionNode.OutputTypes.categorical_output:
                    return DarlResult.DataType.categorical;
                case OutputDefinitionNode.OutputTypes.numeric_output:
                    return DarlResult.DataType.numeric;
                case OutputDefinitionNode.OutputTypes.textual_output:
                    return DarlResult.DataType.textual;
                case OutputDefinitionNode.OutputTypes.temporal_output:
                    return DarlResult.DataType.temporal;

            }
            return DarlResult.DataType.numeric;
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
                StringBuilder sb = new StringBuilder();
                foreach (var input in inputs.Values)
                {
                    sb.Append("\t" + input.preamble);
                }
                if (inputs.Any())
                    sb.Append("\n");
                foreach (var output in outputs.Values)
                {
                    sb.Append("\t" + output.preamble);
                }
                if (outputs.Any())
                    sb.Append("\n");
                foreach (var constant in constants.Values)
                {
                    sb.Append("\t" + constant.preamble);
                }
                if (constants.Any())
                    sb.Append("\n");
                foreach (var sconstant in strings.Values)
                {
                    sb.Append("\t" + sconstant.preamble);
                }
                if (strings.Any())
                    sb.Append("\n");
                foreach (var pconstant in durations.Values)
                {
                    sb.Append("\t" + pconstant.preamble);
                }
                if (durations.Any())
                    sb.Append("\n");
                foreach (var seq in sequences.Values)
                {
                    sb.Append("\t" + seq.preamble);
                }
                if (strings.Any())
                    sb.Append("\n");
                foreach (var st in stores.Values)
                {
                    sb.Append("\t" + st.preamble);
                }
                if (stores.Any())
                    sb.Append("\n");
                return sb.ToString();
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
            if (rules.ContainsKey(currentOutput)) //can be unused output
            {
                if (outputs[currentOutput].result.IsUnknown() || outputs[currentOutput].result.GetWeight() < 1.0)
                {
                    var unsatisfiedRules = rules[currentOutput].Where(a => a.IsUnknown || a.confidenceNode != null && a.confidenceNode.weight < 1.0).ToList();
                    if (unsatisfiedRules.Count > 0)
                    {
                        double childSaliency = saliency / unsatisfiedRules.Count;
                        foreach (RuleNode r in unsatisfiedRules)
                        {
                            r.WalkSaliences(childSaliency, root, currentRuleSet, currentOutput);
                        }
                    }
                }
            }
            else if (stores.ContainsKey(currentOutput)) //it's a store; walk saliences for each store.address combination that occurs as an output
            {
                foreach (var combiName in stores[currentOutput].storeOutputs)
                {
                    var unsatisfiedRules = rules[combiName].Where(a => a.IsUnknown || a.confidenceNode != null && a.confidenceNode.weight < 1.0).ToList();
                    if (unsatisfiedRules.Count > 0)
                    {
                        double childSaliency = saliency / unsatisfiedRules.Count;
                        foreach (RuleNode r in rules[combiName])
                        {
                            r.WalkSaliences(childSaliency, root, currentRuleSet, combiName);
                        }
                    }
                }
            }
        }
    }
}
