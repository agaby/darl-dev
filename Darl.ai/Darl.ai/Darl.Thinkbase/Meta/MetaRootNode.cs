using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class MetaRootNode : DarlMetaNode
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
        /// The lineages
        /// </summary>
        public Dictionary<string, LineageDefinitionNode> lineages = new Dictionary<string, LineageDefinitionNode>();

        /// <summary>
        /// The stores
        /// </summary>
        public Dictionary<string, StoreDefinitionNode> stores = new Dictionary<string, StoreDefinitionNode>();

        /// <summary>
        /// The rules
        /// </summary>
        public Dictionary<string, List<RuleNode>> rules = new Dictionary<string, List<RuleNode>>();


        public Dictionary<string, StoreNode> storeInputs = new Dictionary<string, StoreNode>();

        public HashSet<GraphObject> dependentGraphObjects = new HashSet<GraphObject>();

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
            //add predefined lineages
            if (((DarlMetaGrammar)context.Language.Grammar).structure != null)
            {
                foreach (var c in ((DarlMetaGrammar)context.Language.Grammar).structure.PreloadLineages.Keys)
                {
                    lineages.Add(c, ((DarlMetaGrammar)context.Language.Grammar).structure.PreloadLineages[c]);
                }
            }
            //add network lineages
            var cm = ((DarlMetaGrammar)context.Language.Grammar).currentModel;
            if (cm != null)
            {
                foreach (var c in cm.GetLineages(GraphElementType.node))
                {
                    if (!lineages.ContainsKey(c.typeWord))
                    {
                        lineages.Add(c.typeWord, new LineageDefinitionNode { name = c.typeWord, Value = c.lineage, typeword = c.typeWord });
                    }
                }
                foreach (var c in cm.GetLineages(GraphElementType.connection))
                {
                    if (!lineages.ContainsKey(c.typeWord))
                    {
                        lineages.Add(c.typeWord, new LineageDefinitionNode { name = c.typeWord, Value = c.lineage, typeword = c.typeWord });
                    }
                }
                foreach (var c in cm.GetLineages(GraphElementType.attribute))
                {
                    if (!lineages.ContainsKey(c.typeWord))
                    {
                        lineages.Add(c.typeWord, new LineageDefinitionNode { name = c.typeWord, Value = c.lineage, typeword = c.typeWord });
                    }
                }
            }
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
                else if (child.AstNode is DurationDefinitionNode)
                    durations.Add(((DurationDefinitionNode)child.AstNode).name, (DurationDefinitionNode)child.AstNode);
                else if (child.AstNode is StoreDefinitionNode)
                    stores.Add(((StoreDefinitionNode)child.AstNode).name, (StoreDefinitionNode)child.AstNode);
                else if (child.AstNode is LineageDefinitionNode)
                    lineages[((LineageDefinitionNode)child.AstNode).name] = (LineageDefinitionNode)child.AstNode;
                //                    lineages.Add(((LineageDefinitionNode)child.AstNode).name, (LineageDefinitionNode)child.AstNode);
                else if (child.AstNode is RuleNode)
                {
                    AddChild("-", child);
                    string name = ((RuleNode)child.AstNode).GetName();
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
            ConstantContext ccontext = new ConstantContext { inputs = inputs, outputs = outputs, constants = constants, strings = strings, controllingIO = string.Empty, storeInputs = storeInputs, storeOutputs = new Dictionary<string, StoreNode>(), stores = stores, durations = durations, lineages = lineages, parseContext = context };
            foreach (var i in inputs.Values)
            {
                if (i.LineageNode != null && i.iType == InputDefinitionNode.InputTypes.categorical_input)
                {
                    i.LineageNode.WalkDependencies(dependencies, null, ccontext, ((DarlMetaGrammar)context.Language.Grammar).currentModel, ((DarlMetaGrammar)context.Language.Grammar).currentNode);
                    var grammar = context.Language.Grammar as DarlMetaGrammar;
                    if (grammar.currentNode == null || grammar.currentNode.properties == null)
                        break;
                    var lineage = i.LineageNode is LineageLiteral ? ((LineageLiteral)i.LineageNode).literal : ccontext.lineages[i.LineageNode.GetName()].Value;
                    var att = grammar.currentModel.FindDataAttribute(grammar.currentNode.id, lineage, grammar.state);
                    if (att == null) //assume content is a comma delimited list of strings.
                        break;
                    var cats = att.Value.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var c in cats)
                    {
                        i.categories.Add(c.Replace('"', ' ').Trim());
                    }
                }
            }

            foreach (string outName in rules.Keys)
            {
                if (outputs[outName].lineageNode != null)
                    outputs[outName].lineageNode.WalkDependencies(dependencies, null, ccontext, ((DarlMetaGrammar)context.Language.Grammar).currentModel, ((DarlMetaGrammar)context.Language.Grammar).currentNode);
                if (outputs[outName].CatLineageNode != null)
                    outputs[outName].CatLineageNode.WalkDependencies(dependencies, null, ccontext, ((DarlMetaGrammar)context.Language.Grammar).currentModel, ((DarlMetaGrammar)context.Language.Grammar).currentNode);
                foreach (RuleNode rule in rules[outName])
                {
                    rule.WalkDependencies(dependencies, rule.ruleOutput, ccontext, ((DarlMetaGrammar)context.Language.Grammar).currentModel, ((DarlMetaGrammar)context.Language.Grammar).currentNode);
                }
            }

            //all inputs are initially set to sequence 0
            int currentSequence = 1;
            bool complete = false;
            while (!complete)
            {
                List<IntraSetDependency> deletions = new List<IntraSetDependency>();
                foreach (IntraSetDependency dep in dependencies)
                {
                    if (dep.dependentObject != null)
                        dependentGraphObjects.Add(dep.dependentObject);
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
                    context.AddMessage(DarlCompiler.ErrorLevel.Error, treeNode.Token.Location, "Loop found in rules: " + sb.ToString());

                    throw new MetaRuleException();
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

        public SalienceRecord? CalculateKGSaliences(List<SalienceRecord> saliences, KnowledgeState ks)
        {
            //if(saliences.Any(a => a.gobj == ))
            return null;
        }

        public Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState)
        {
            var unsatisfiedInputSaliences = new Dictionary<string, double>();
            foreach (InputDefinitionNode input in inputs.Values) //clear saliences
                input.Salience = 0.0;
            foreach (string outName in outputs.Keys)
            {
                if (IsUnfilled(currentState, outName)) //output is unknown
                {
                    if (rules.ContainsKey(outName)) //can be unused output
                    {
                        if (outputs[outName].result.IsUnknown() || outputs[outName].result.GetWeight() < 1.0)
                        {
                            var unsatisfiedRules = rules[outName].Where(a => a.IsUnknown || a.confidenceNode != null && a.confidenceNode.weight < 1.0).ToList();
                            if (unsatisfiedRules.Count > 0)
                            {
                                double childSaliency = 1.0 / unsatisfiedRules.Count;
                                foreach (DarlMetaNode r in unsatisfiedRules)
                                {
                                    r.WalkSaliences(childSaliency, this);
                                }
                            }
                        }
                    }
                }
            }
            foreach (InputDefinitionNode input in inputs.Values.Where(a => a.Salience > 0.0 && IsUnfilled(currentState, a.name)))
            {
                unsatisfiedInputSaliences.Add(input.name, input.Salience);
            }
            return unsatisfiedInputSaliences;
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
                    sb.Append(input.preamble);
                }
                if (inputs.Any())
                    sb.Append("\n");
                foreach (var output in outputs.Values)
                {
                    sb.Append(output.preamble);
                }
                if (outputs.Any())
                    sb.Append("\n");
                foreach (var constant in constants.Values)
                {
                    sb.Append(constant.preamble);
                }
                if (constants.Any())
                    sb.Append("\n");
                foreach (var sconstant in strings.Values)
                {
                    sb.Append(sconstant.preamble);
                }
                if (strings.Any())
                    sb.Append("\n");
                foreach (var pconstant in durations.Values)
                {
                    sb.Append(pconstant.preamble);
                }
                if (durations.Any())
                    sb.Append("\n");
                if (strings.Any())
                    sb.Append("\n");
                foreach (var st in stores.Values)
                {
                    sb.Append(st.preamble);
                }
                if (stores.Any())
                    sb.Append("\n");
                return sb.ToString();
            }
        }

        private static bool IsUnfilled(List<DarlResult> currentState, string name)
        {
            return !currentState.Any(a => a.name == name) || currentState.Any(a => a.name == name) && (currentState.First(a => a.name == name).IsUnknown() || currentState.First(a => a.name == name).GetWeight() < 1.0);
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
            DarlMetaGrammar grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            foreach (InputDefinitionNode input in inputs.Values)
            {
                string compName = input.name;
                if (input.networkNode != null)
                {
                    input.Value = grammar.NetWorkResults(input.networkNode.nodeId, input.networkNode.lineage);
                }
                else
                {
                    input.Value = grammar.ResultByName(compName);
                }
                if (((object)input.Value) != null) //DarlResult overides !=
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
                var binding = thread.Bind(input.name, DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                await binding.SetValueRef(thread, input.Value);
            }

            foreach (IOSequenceDefinitionNode output in outputs.Values)
            {
                var binding = thread.Bind(output.GetName(), DarlCompiler.Interpreter.BindingRequestFlags.Write | DarlCompiler.Interpreter.BindingRequestFlags.ExistingOrNew);
                await binding.SetValueRef(thread, output.result);
            }
            //perform the processing
            foreach (IOSequenceDefinitionNode outNode in orderedOutputs)
            {

                if (rules.ContainsKey(outNode.GetName()) && outNode is OutputDefinitionNode)
                {
                    string compName = outNode.name;
                    var ty = ConvertOType(((OutputDefinitionNode)outNode).oType);
                    ((OutputDefinitionNode)outNode).SetLineage(thread);
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
                    //add output value to knowledgeState if lineage set.
                    if (!string.IsNullOrEmpty(((OutputDefinitionNode)outNode).lineage) && !outNode.result.IsUnknown() && grammar.currentNode != null) //possible confidence limit too?
                    {
                        var lineage = ((OutputDefinitionNode)outNode).lineage;
                        var att = new GraphAttribute { lineage = lineage, confidence = outNode.confidence, name = outNode.name, inferred = true, _virtual = false, id = Guid.NewGuid().ToString(), type = (GraphAttribute.DataType)Enum.Parse(typeof(GraphAttribute.DataType), outNode.result.dataType.ToString()), value = outNode.Value is null ? "" : outNode.Value.ToString() };
                        grammar.state.AddAttribute(grammar.currentNode.id, att);
                    }
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
                case OutputDefinitionNode.OutputTypes.network_output:
                    return DarlResult.DataType.network;
            }
            return DarlResult.DataType.numeric;
        }


    }
}
