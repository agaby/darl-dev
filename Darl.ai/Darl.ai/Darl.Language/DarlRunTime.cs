/// <summary>
/// DarlRunTime.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Licensing;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DarlLanguage
{
    /// <summary>
    /// Methods operating on DARL code
    /// </summary>
    public class DarlRunTime
    {

        /// <summary>
        /// The DARL grammar object
        /// </summary>
        protected DarlGrammar grammar;
        /// <summary>
        /// The language data
        /// </summary>
        protected LanguageData language;
        /// <summary>
        /// The DARL parser
        /// </summary>
        protected Parser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlRunTime"/> class.
        /// </summary>
        public DarlRunTime()
        {
            if (!DarlLicense.licensed)
                throw new RuleException("license not set or invalid");
            grammar = new DarlGrammar();
            language = new LanguageData(grammar);
            parser = new Parser(language);
        }

        /// <summary>
        /// Evaluate a source file with a set of values
        /// </summary>
        /// <param name="inputs">a set of input values</param>
        /// <param name="ruleset">The ruleset to select from the source to evaluate. Empty if top level evaluation of map.</param>
        /// <param name="source">The DARL source</param>
        /// <returns>An augmented set of results</returns>
        public async Task<List<DarlResult>> Evaluate(List<DarlResult> inputs, string source, string ruleset = "")
        {
            ParseTree parseTree = CreateTree(source);
            return await Evaluate(parseTree, inputs, ruleset);
        }


        /// <summary>
        /// Evaluate an existing tree.
        /// </summary>
        /// <param name="parseTree">The tree to evaluate</param>
        /// <param name="inputs">a set of input values</param>
        /// <param name="ruleset">The ruleset to select from the source to evaluate. Empty if top level evaluation of map.</param>
        /// <returns>An augmented set of results</returns>
        public virtual async Task<List<DarlResult>> Evaluate(ParseTree parseTree, List<DarlResult> inputs, string ruleset = "")
        {
            grammar.results = inputs;
            await grammar.RunSample(new RunSampleArgs(language, ruleset, parseTree));
            return inputs;
        }

        /// <summary>
        /// Create a tree to be used for repeated evaluations.
        /// </summary>
        /// <param name="source">The DARL source</param>
        /// <param name="stores">If we have dynamic io in the ruleset we will need to have a set of stores to resolve categies etc.</param>
        /// <returns>The tree</returns>
        /// <exception cref="RuleException">Thrown for syntax errors in source.</exception>
        public ParseTree CreateTree(string source, Dictionary<string, ILocalStore>? stores = null)
        {
            ParseTree parseTree = parser.Parse(source, stores);
            if (parseTree.Status == ParseTreeStatus.Error)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var message in parseTree.ParserMessages)
                {
                    sb.AppendLine(string.Format("{0}: {1}, line: {2} column: {3}", message.Level.ToString(), message.Message, message.Location.Line, message.Location.Column));
                }
                throw new RuleException(string.Format("Error in source: {0}", sb.ToString()));
            }
            return parseTree;
        }

        /// <summary>
        /// Create tree for editing purposes
        /// </summary>
        /// <remarks>Allows errors</remarks>
        /// <param name="source">The source</param>
        /// <returns>the tree</returns>
        public ParseTree CreateTreeEdit(string source)
        {
            return parser.Parse(source, null);
        }

        public void SetStoreInterface(ParseTree parseTree, string name, ILocalStore localStore)
        {
            var root = parseTree.Root.AstNode as MapRootNode;
            if (!root.stores.ContainsKey(name))
            {
                throw new System.Exception($"name {name} not in list of stores defined in map.");
            }
            if (!grammar.stores.ContainsKey(name))
                grammar.stores.Add(name, localStore);
            else
                grammar.stores[name] = localStore;
        }




        /// <summary>
        /// Calculates the saliences.
        /// </summary>
        /// <param name="currentState">Current state of the  inputs and outputs</param>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>
        /// A dictionary of inputs not yet given values, for which values are still required, and their saliences if &gt; 0.
        /// </returns>
        public Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState, ParseTree parseTree)
        {
            MapRootNode root = parseTree.GetMapRoot();
            return root.CalculateSaliences(currentState);
        }

        /// <summary>
        /// Fill in a rule definition using Supervised learning
        /// </summary>
        /// <param name="rulesource">the source rules</param>
        /// <param name="datasource">the data to mine</param>
        /// <param name="sets">The number of fuzzy sets to use for each numeric input or output</param>
        /// <param name="percentTrain">the percentage (0-100) of the set to use for training. The rest will be used for test.</param>
        /// <param name="rep">The report</param>
        /// <returns>
        /// The updated rule source
        /// </returns>
        /// <exception cref="RuleException">
        /// No rulesets defined.
        /// or
        /// More than one ruleset is set for machine learning
        /// or
        /// No ruleset is set for machine learning.
        /// or
        /// No pattern defined.
        /// or
        /// More than one output path is set for machine learning
        /// or
        /// No output path is set for machine learning
        /// or
        /// Invalid rule root.
        /// or
        /// No inputs defined.
        /// or
        /// Exactly one output must be defined.
        /// or
        /// Problems parsing source.
        /// or
        /// Insufficient data to learn from; only  + patternCount.ToString() +  patterns found. Review your data and selection pattern
        /// or
        /// Internal error: new ruleset cannot be parsed.
        /// </exception>
        public string MineSupervised(string rulesource, string datasource, int sets = 3, int percentTrain = 100, DarlMineReport? rep = null)
        {
            var ps = PrepareRuleSetForLearning(rulesource, datasource, percentTrain, sets);

            string newRuleSource;
            //Perform tree induction
            DarlDecisionNode rootDecisioNode = new DarlDecisionNode();
            rootDecisioNode.GenerateNodes(null, ps.outp, ps.inps, ps.inSamplePatterns, 0, 0);
            rootDecisioNode.CheckForMerge();

            //We now need to recreate the ruleset. The inputs and output will have acquired sets and categories
            //and we have a set of rules to insert. We want to replace everything between the braces

            string ruleString = "";
            //write out inputs sorted alphabetically
            var inList = new List<string>(ps.rroot.inputs.Keys);
            inList.Sort();
            foreach (string inpName in inList)
            {
                var inp = ps.rroot.inputs[inpName];
                ruleString += inp.TermToDarl();
            }
            ruleString += "\n";
            //now the single output
            ruleString += ps.outp.TermToDarl() + "\n";
            rootDecisioNode.GenerateRules(ref ruleString, "", 0);
            //insert rules into ruleset outline at insertion point
            newRuleSource = rulesource.Remove(ps.ruleSetContents.Location.Position, ps.ruleSetContents.Length + 1);
            newRuleSource = newRuleSource.Insert(ps.ruleSetContents.Location.Position, ruleString);
            //now re-parse the new ruleset and test
            ParseTree newTree = parser.Parse(newRuleSource, null);
            if (newTree.Status == ParseTreeStatus.Error)
            {
                throw new RuleException("Internal error: new ruleset cannot be parsed.");
            }
            double inSampleMeasure = 0;
            double outSampleMeasure = 0;
            int unknowns = 0;
            if (ps.jsonData)
                TestJson(ps, newTree, out inSampleMeasure, out outSampleMeasure);
            else
                TestXml(ps, newTree, out inSampleMeasure, out outSampleMeasure);

            string commentString = string.Format("// Generated by DARL rule induction on  {0}.\n", DateTime.Now.ToString());
            double iS;
            double oS = 0.0;
            if (ps.outp.iType == OutputDefinitionNode.OutputTypes.categorical_output)
            {
                iS = inSampleMeasure * 100.0 / ps.inSamplePatterns.Count;
                commentString += string.Format("// Train correct:  {0}% on {1} patterns.\n", iS.ToString("0.00"), ps.inSamplePatterns.Count.ToString());
                if (percentTrain < 100)
                {
                    oS = outSampleMeasure * 100.0 / ps.outSamplePatterns.Count;
                    commentString += string.Format("// Test correct:  {0}% on {1} patterns.\n", oS.ToString("0.00"), ps.outSamplePatterns.Count.ToString());
                }
            }
            else
            {
                iS = Math.Sqrt(inSampleMeasure / ps.inSamplePatterns.Count);
                commentString += string.Format("// Train RMS error:  {0} on {1} patterns.\n", iS.ToString("0.00"), ps.inSamplePatterns.Count.ToString());
                if (percentTrain < 100)
                {
                    oS = Math.Sqrt(outSampleMeasure * 100.0 / ps.outSamplePatterns.Count);
                    commentString += string.Format("// Test RMS error:  {0} on {1} patterns.\n", oS.ToString("0.00"), ps.outSamplePatterns.Count.ToString());
                }
            }



            commentString += string.Format("// Percentage of unknown responses over all patterns: {0}\n", rep.unknownResponsePercent.ToString("0.00"));

            //insert performance as comments
            newRuleSource = newRuleSource.Insert(ps.ruleSetContents.Location.Position, commentString);

            if (rep != null)
            {
                rep.trainPercent = percentTrain;
                rep.trainPerformance = iS;
                rep.testPerformance = oS;
                rep.unknownResponsePercent = unknowns * 100.0 / ps.patternCount;
                rep.code = newRuleSource;
            }

            return newRuleSource;
        }

        /// <summary>
        /// Async supervised mining
        /// </summary>
        /// <param name="rulesource">ruleset skeleton</param>
        /// <param name="datasource">Training data</param>
        /// <param name="sets">Number of sets, choices 3,5,7,9</param>
        /// <param name="percentTrain">percentage to train on</param>
        /// <returns>The new mined darl ruleset</returns>
        public async Task<string> MineSupervisedAsync(string rulesource, string datasource, int sets = 3, int percentTrain = 100, DarlMineReport? rep = null)
        {
            return await Task.Run<string>(() => MineSupervised(rulesource, datasource, sets, percentTrain, rep));
        }

        public string MineAssociation(string rulesource, string datasource, int percentTrain = 100, int sets = 3)
        {
            var ps = PrepareRuleSetForLearning(rulesource, datasource, percentTrain, sets);
            return string.Empty;
        }

        public async Task<string> MineAssociationAsync(string rulesource, string datasource, int percentTrain = 100, int sets = 3)
        {
            return await Task.Run<string>(() => MineAssociation(rulesource, datasource, percentTrain, sets));
        }


        public PreparedLearningSet PrepareRuleSetForLearning(string rulesource, string datasource, int percentTrain, int sets)
        {
            var ps = new PreparedLearningSet
            {
                sets = sets
            };
            try
            {
                ParseTree parseTree = parser.Parse(rulesource, null);
                ps.root = parseTree.Root.AstNode as MapRootNode;
                if (ps.root.rulesets.Count == 0)
                    throw new RuleException("No rulesets defined.");
                foreach (var rs in ps.root.rulesets.Values)
                {
                    if (rs.pType == RuleSetNode.ProcessType.supervised || rs.pType == RuleSetNode.ProcessType.reinforcement || rs.pType == RuleSetNode.ProcessType.association || rs.pType == RuleSetNode.ProcessType.unsupervised)
                    {
                        if (ps.ActiveRuleset == null)
                        {
                            ps.ActiveRuleset = rs;
                            ps.ruleset = rs.rulesetname;
                        }
                        else
                            throw new RuleException("More than one ruleset is set for machine learning");
                    }
                }
                if (ps.ActiveRuleset == null)
                    throw new RuleException("No ruleset is set for machine learning.");
                //check for pattern definition
                if (string.IsNullOrEmpty(ps.root.pattern))
                    throw new RuleException("No pattern defined.");
                //now tie each input in the active rule set to its respective mapinput and find the path
                foreach (var mapin in ps.root.inputs.Values)
                {
                    //match up the wire
                    foreach (var wire in ps.root.wires)
                    {
                        if (wire.wiretype == WireDefinitionNode.WireType.wirein)
                        {
                            if (wire.destRuleset == ps.ruleset && wire.sourcename == mapin.Name)
                            {
                                ps.inputs.Add(wire.destname, mapin.Path);
                                break;
                            }
                        }
                    }
                }
                ps.rroot = ps.ActiveRuleset.ChildNodes.Last() as RuleRootNode;
                if (ps.ActiveRuleset.pType == RuleSetNode.ProcessType.supervised)
                {
                    foreach (var mapout in ps.root.outputs.Values)
                    {
                        if (!string.IsNullOrEmpty(mapout.Path))
                        {
                            if (!string.IsNullOrEmpty(ps.outputPath))
                                throw new RuleException("More than one output path is set for machine learning");
                            ps.outputPath = mapout.Path;
                        }
                    }
                    if (string.IsNullOrEmpty(ps.outputPath))
                    {
                        throw new RuleException("No output path is set for machine learning");
                    }
                    if (ps.rroot.outputs.Values.First() is StoreNode)
                    {
                        throw new RuleException("Destination for machine learning cannot be a Store");
                    }
                    ps.outp = ps.rroot.outputs.Values.First() as OutputDefinitionNode;

                }
                else if (ps.ActiveRuleset.pType == RuleSetNode.ProcessType.reinforcement || ps.ActiveRuleset.pType == RuleSetNode.ProcessType.unsupervised)
                {
                    ps.outps = new List<OutputDefinitionNode>((IEnumerable<OutputDefinitionNode>)ps.rroot.outputs.Values.Where(a => a is OutputDefinitionNode));
                }
                if (ps.rroot == null)
                    throw new RuleException("Invalid rule root.");
                if (ps.rroot.inputs.Count == 0)
                    throw new RuleException("No inputs defined.");
                if (ps.rroot.outputs.Count != 1 && ps.ActiveRuleset.pType == RuleSetNode.ProcessType.supervised)
                    throw new RuleException("Exactly one output must be defined.");
                ps.ruleSetContents = ps.rroot.Span;
            }
            catch (Exception ex)
            {
                throw new RuleException("Problems parsing source.", ex);
            }
            if (datasource.Trim().StartsWith("<"))
            {
                ps.patternCount = MineXml(datasource, ps);
            }
            else
            {
                ps.patternCount = MineJson(datasource, ps);
                ps.jsonData = true;
            }
            //create an ind set for training and testing
            //Create a random selection of indices to be used when data is loaded
            if (percentTrain < 100)
            {
                Random rand = new Random();
                for (int n = 0; n < ps.patternCount; n++)
                {
                    if (rand.Next(100) < percentTrain)
                    {
                        ps.inSamplePatterns.Add(n);
                    }
                    else
                    {
                        ps.outSamplePatterns.Add(n);
                    }
                }
            }
            else
            {
                for (int n = 0; n < ps.patternCount; n++)
                {
                    ps.inSamplePatterns.Add(n);
                }
            }
            return ps;
        }

        private int MineJson(string datasource, PreparedLearningSet ps)
        {
            JObject doc = JObject.Parse(datasource);
            ps.jPatterns = doc.SelectTokens(ps.root.pattern);
            int patternCount = ps.jPatterns.Count();
            if (patternCount < 5)
                throw new RuleException("Insufficient data to learn from; only " + patternCount.ToString() + " patterns found. Review your data and selection pattern");
            ps.inps = new List<InputDefinitionNode>();
            //load data to train from into Structures
            foreach (var inputName in ps.root.inputs.Keys)
            {
                if (ps.inputs.ContainsKey(inputName)) // a path is supplied
                {
                    var inp = ps.rroot.inputs[inputName];
                    ps.inps.Add(inp);
                    string path = ps.inputs[inputName];
                    switch (inp.iType)
                    {
                        case InputDefinitionNode.InputTypes.arity_input:
                            //now fetch the individual data item, remembering it need not be there! - load string.Empty in that case.
                            foreach (var pat in ps.jPatterns)
                            {
                                inp.learningSource.Add(pat.SelectTokens(path).Count());
                            }
                            break;
                        case InputDefinitionNode.InputTypes.presence_input:
                            foreach (var pat in ps.jPatterns)
                            {
                                inp.learningSource.Add(pat.SelectTokens(path) != null ? 0 : 1);
                            }
                            break;
                        case InputDefinitionNode.InputTypes.categorical_input:
                            HandleCategoriesJson(ps.jPatterns, path, inp);
                            break;
                        case InputDefinitionNode.InputTypes.numeric_input:
                            HandleSetsJson(ps.jPatterns, path, inp, ps.sets);
                            break;
                        case InputDefinitionNode.InputTypes.textual_input:

                            break;
                    }
                }
            }
            //do the same for the output
            if (ps.ActiveRuleset.pType == RuleSetNode.ProcessType.supervised)
            {
                switch (ps.outp.iType)
                {
                    case OutputDefinitionNode.OutputTypes.categorical_output:
                        HandleCategoriesJson(ps.jPatterns, ps.outputPath, ps.outp);
                        break;
                    case OutputDefinitionNode.OutputTypes.numeric_output:
                        HandleSetsJson(ps.jPatterns, ps.outputPath, ps.outp, ps.sets);
                        break;
                }
            }
            return patternCount;
        }
        private int MineXml(string datasource, PreparedLearningSet ps)
        {
            XDocument doc = XDocument.Parse(datasource);
            ps.xPatterns = doc.XPathSelectElements(ps.root.pattern);
            int patternCount = ps.xPatterns.Count();
            if (patternCount < 5)
                throw new RuleException("Insufficient data to learn from; only " + patternCount.ToString() + " patterns found. Review your data and selection pattern");
            ps.inps = new List<InputDefinitionNode>();
            //load data to train from into Structures
            foreach (var inputName in ps.root.inputs.Keys)
            {
                if (ps.inputs.ContainsKey(inputName)) // a path is supplied
                {
                    var inp = ps.rroot.inputs[inputName];
                    ps.inps.Add(inp);
                    string path = ps.inputs[inputName];
                    switch (inp.iType)
                    {
                        case InputDefinitionNode.InputTypes.arity_input:
                            //now fetch the individual data item, remembering it need not be there! - load string.Empty in that case.
                            foreach (var pat in ps.xPatterns)
                            {
                                inp.learningSource.Add(pat.XPathSelectElements(path).Count());
                            }
                            break;
                        case InputDefinitionNode.InputTypes.presence_input:
                            foreach (var pat in ps.xPatterns)
                            {
                                inp.learningSource.Add(pat.XPathSelectElement(path) != null ? 0 : 1);
                            }
                            break;
                        case InputDefinitionNode.InputTypes.categorical_input:
                            HandleCategories(ps.xPatterns, path, inp);
                            break;
                        case InputDefinitionNode.InputTypes.numeric_input:
                            HandleSets(ps.xPatterns, path, inp, ps.sets);
                            break;
                        case InputDefinitionNode.InputTypes.textual_input:

                            break;
                    }
                }
            }
            if (ps.ActiveRuleset.pType == RuleSetNode.ProcessType.supervised)
            {
                //do the same for the output
                switch (ps.outp.iType)
                {
                    case OutputDefinitionNode.OutputTypes.categorical_output:
                        HandleCategories(ps.xPatterns, ps.outputPath, ps.outp);
                        break;
                    case OutputDefinitionNode.OutputTypes.numeric_output:
                        HandleSets(ps.xPatterns, ps.outputPath, ps.outp, ps.sets);
                        break;
                }
            }
            return patternCount;
        }

        private int TestXml(PreparedLearningSet ps, ParseTree newTree, out double inSampleMeasure, out double outSampleMeasure)
        {
            int unknowns = 0;
            int index = 0;
            inSampleMeasure = 0.0;
            outSampleMeasure = 0.0;
            foreach (var pat in ps.xPatterns)
            {
                var stimulus = new List<DarlResult>();
                foreach (string input in ps.inputs.Keys)
                {
                    string path = ps.inputs[input];
                    if (pat.XPathSelectElement(path) != null)
                    {
                        var val = pat.XPathSelectElement(path).Value;
                        if (string.IsNullOrEmpty(val))
                            stimulus.Add(new DarlResult(input, 0.0, true));
                        else
                            stimulus.Add(new DarlResult(input, val));
                    }
                }
                var response = Evaluate(newTree, stimulus, ps.ruleset).Result;
                if (pat.XPathSelectElement(ps.outputPath) != null)
                {
                    double score = 0.0;
                    var expected = pat.XPathSelectElement(ps.outputPath).Value;
                    var resp = response.First(a => a.name == ps.outp.name);
                    if (!string.IsNullOrEmpty(expected) && !resp.IsUnknown())
                    {
                        if (ps.outp.iType == OutputDefinitionNode.OutputTypes.categorical_output)
                        {
                            score = (string)resp.Value == expected ? 1.0 : 0.0;
                        }
                        else
                        {
                            double expectedD;
                            try
                            {
                                expectedD = Convert.ToDouble(expected);
                                score = (double)resp.Value - expectedD;
                                score *= score;
                                Trace.WriteLine(expectedD.ToString("0.000") + "," + ((double)resp.Value).ToString("0.000"));
                            }
                            catch { }
                        }
                        if (ps.inSamplePatterns.Contains(index))
                        {
                            inSampleMeasure += score;
                        }
                        else
                        {
                            outSampleMeasure += score;
                        }
                    }
                    else if (resp.IsUnknown())
                    {
                        unknowns++;
                    }
                }
                index++;
            }
            return unknowns;
        }

        private int TestJson(PreparedLearningSet ps, ParseTree newTree, out double inSampleMeasure, out double outSampleMeasure)
        {
            int unknowns = 0;
            int index = 0;
            inSampleMeasure = 0.0;
            outSampleMeasure = 0.0;
            foreach (var pat in ps.jPatterns)
            {
                var stimulus = new List<DarlResult>();
                foreach (string input in ps.inputs.Keys)
                {
                    string path = ps.inputs[input];
                    if (pat.SelectToken(path) != null)
                    {
                        var val = pat.SelectToken(path).ToString();
                        if (string.IsNullOrEmpty(val))
                            stimulus.Add(new DarlResult(input, 0.0, true));
                        else
                            stimulus.Add(new DarlResult(input, val));
                    }
                }
                var response = Evaluate(newTree, stimulus, ps.ruleset).Result;
                var resp = response.First(a => a.name == ps.outp.name);
                if (pat.SelectToken(ps.outputPath) != null)
                {
                    double score = 0.0;
                    var expected = pat.SelectToken(ps.outputPath).ToString();
                    if (!string.IsNullOrEmpty(expected) && !resp.IsUnknown())
                    {
                        if (ps.outp.iType == OutputDefinitionNode.OutputTypes.categorical_output)
                        {
                            score = (string)resp.Value == expected ? 1.0 : 0.0;
                        }
                        else
                        {
                            double expectedD;
                            try
                            {
                                expectedD = Convert.ToDouble(expected);
                                score = (double)resp.Value - expectedD;
                                score *= score;
                                Trace.WriteLine(expectedD.ToString("0.000") + "," + ((double)resp.Value).ToString("0.000"));
                            }
                            catch { }
                        }
                        if (ps.inSamplePatterns.Contains(index))
                        {
                            inSampleMeasure += score;
                        }
                        else
                        {
                            outSampleMeasure += score;
                        }
                    }
                    else if (resp.IsUnknown())
                    {
                        unknowns++;
                    }
                }
                index++;
            }
            return unknowns;
        }

        private void HandleCategories(IEnumerable<XElement> patterns, string path, IODefinitionNode inp)
        {
            foreach (var pat in patterns)
            {
                if (pat.XPathSelectElement(path) != null)
                {
                    string val = pat.XPathSelectElement(path).Value;
                    if (!inp.categories.Contains(val))
                    {
                        inp.categories.Add(val);//collect all categories
                        inp.catsAsIdentifiers.Add(val, false);
                    }
                    inp.learningSource.Add(inp.categories.IndexOf(val));
                }
                else
                {
                    inp.learningSource.Add(-1);
                }
            }
        }

        private void HandleCategoriesJson(IEnumerable<JToken> patterns, string path, IODefinitionNode inp)
        {
            foreach (var pat in patterns)
            {
                if (pat.SelectToken(path) != null)
                {
                    string val = pat.SelectToken(path).ToString();
                    if (!inp.categories.Contains(val))
                    {
                        inp.categories.Add(val);//collect all categories
                        inp.catsAsIdentifiers.Add(val, false);
                    }
                    inp.learningSource.Add(inp.categories.IndexOf(val));
                }
                else
                {
                    inp.learningSource.Add(-1);
                }
            }
        }

        private void HandleSets(IEnumerable<XElement> patterns, string path, IODefinitionNode inp, int sets)
        {
            List<double> values = new List<double>();
            foreach (var pat in patterns)
            {
                string val = pat.XPathSelectElement(path).Value;
                if (double.TryParse(val, out double dVal))
                {
                    values.Add(dVal);
                }
                else
                {
                    values.Add(double.NaN);
                }
            }
            //now create set definitions.
            FindSetBoundaries(sets, inp, values);
        }

        private void HandleSetsJson(IEnumerable<JToken> patterns, string path, IODefinitionNode inp, int sets)
        {
            List<double> values = new List<double>();
            foreach (var pat in patterns)
            {
                string val = pat.SelectToken(path).ToString();
                if (double.TryParse(val, out double dVal))
                {
                    values.Add(dVal);
                }
                else
                {
                    values.Add(double.NaN);
                }
            }
            //now create set definitions.
            FindSetBoundaries(sets, inp, values);
        }
        /// <summary>
        /// Find set boundaries by sorting the input values and dividing the range of non-null values
        /// </summary>
        /// <param name="desiredSets">Choices are 3,5,7 and 9;</param>
        /// <param name="inp">input to create sets for.</param>
        /// <param name="values">set of data values</param>
        /// <exception cref="RuleException">Thrown if desiredSets is illegal.</exception>
        /// <remarks>Null values are represented by NaNs. Having generated sets, use crisp values to categorize each value as a set index and membership</remarks>
        public void FindSetBoundaries(int desiredSets, IODefinitionNode inp, List<double> values)
        {
            List<int> keylist = new List<int>(); //this will hold the sorted indexes of the data values
            bool output = inp is OutputDefinitionNode;
            for (int n = 0; n < values.Count; n++)
                keylist.Add(n);
            keylist.Sort((a, b) =>
            {
                if (double.IsNaN(values[a]))
                    return 1;
                if (double.IsNaN(values[b]))
                    return -1;
                return values[a].CompareTo(values[b]);
            }); //sort the indexes, not the values

            int nValues = values.Count;
            if (values.Count > 0 && double.IsNaN(values[keylist[0]]))
                return; //only null data in this input/output
            // Some of these values may be nulls. The sort algorithm stuffs these at the top end with the value NaN
            // decrement nValues until the top value is not NaN.
            while (double.IsNaN(values[keylist[nValues - 1]]))
            {
                nValues--;
            }
            //check for too few data values
            if (nValues < desiredSets * 2 + 1)
                return; // can't create sets - test for this - shouldn't cause problems.

            //so that we don't need to repeatedly look up sets during learning,
            //the learning source will be appended with a composite int value.
            //The result after dividing by 1000 is the positive slope side of the set that contains the value
            //and the remainder is the degree of truth * 1000.
            //So 3567 is the positive slope of set three value 0.567. This means that set 2 also fires on
            //the negative slope 0.433.
            //this is chosen to be easy to reconstruct in IODefinitionNode.CalculateSetMembership
            List<int> ranks = new List<int>(keylist);
            for (int n = 0; n < values.Count; n++)
            {
                ranks[keylist[n]] = n;
            }

            DarlResult res;
            switch (desiredSets)
            {
                case 3:
                    res = new DarlResult(output ? 2 * values[keylist[0]] - values[keylist[(nValues) / 2]] : double.NegativeInfinity, values[keylist[0]], values[keylist[nValues / 2]])
                    {
                        leftUnbounded = true,
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues - 1]])
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 5:
                    res = new DarlResult(output ? 2 * values[keylist[0]] - values[keylist[(nValues) / 4]] : double.NegativeInfinity, values[keylist[0]], values[keylist[nValues / 4]])
                    {
                        leftUnbounded = true,
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues / 2]])
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[3 * nValues / 4]])
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues - 1]])
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? (2 * (double)res.values[2]) - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 7:
                    res = new DarlResult(output ? 2 * values[keylist[0]] - values[keylist[(nValues) / 6]] : double.NegativeInfinity, values[keylist[0]], values[keylist[nValues / 6]])
                    {
                        leftUnbounded = true,
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues / 3]])
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues / 2]])
                    {
                        identifier = "quite_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[2 * nValues / 3]])
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[(5 * nValues) / 6]])
                    {
                        identifier = "quite_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues - 1]])
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 9:
                    res = new DarlResult(output ? (2 * values[keylist[0]]) - values[keylist[(nValues) / 8]] : double.NegativeInfinity, values[keylist[0]], values[keylist[(nValues) / 8]])
                    {
                        leftUnbounded = true,
                        identifier = "extremely_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues / 4]])
                    {
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[(3 * nValues) / 8]])
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues / 2]])
                    {
                        identifier = "quite_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[5 * nValues / 8]])
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[3 * nValues / 4]])
                    {
                        identifier = "quite_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[7 * nValues / 8]])
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], values[keylist[nValues - 1]])
                    {
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "extremely_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                default:
                    throw new RuleException(string.Format("illegal set choice: {0}", desiredSets));
            }

            for (int n = 0; n < values.Count; n++)
            {
                if (ranks[n] < nValues)
                {
                    int rampNumber = ranks[n] * (desiredSets - 1) / nValues;
                    var result = inp.sets[inp.categories[rampNumber]];
                    int truth = 0;
                    if ((double)result.values[2] != (double)result.values[1])
                        truth = Math.Min(999, Convert.ToInt32((values[n] - (double)result.values[1]) * 1000 / ((double)result.values[2] - (double)result.values[1])));
                    inp.learningSource.Add(rampNumber * 1000 + truth);
                }
                else
                    inp.learningSource.Add(-1);
            }
        }

        /// <summary>
        /// Gets the input names.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns>A list of map input names</returns>
        public List<string> GetInputNames(ParseTree tree)
        {
            var root = tree.GetMapRoot();
            return new List<string>(root.inputs.Keys);
        }

        /// <summary>
        /// Gets the output names.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns>A list of map output names</returns>
        public List<string> GetOutputNames(ParseTree tree)
        {
            var root = tree.GetMapRoot();
            return new List<string>(root.outputs.Keys);
        }

        /// <summary>
        /// Clears inputs of an existing tree so it can be re-used
        /// </summary>
        /// <param name="tree">The tree</param>
        public void ClearInputs(ParseTree tree)
        {
            var root = tree.GetMapRoot();
            foreach (var rs in root.rulesets.Values)
            {
                foreach (var inp in rs.ruleRoot.inputs.Values)
                {
                    inp.Value = new DarlResult(0.0, true);
                }
                foreach (var rulegroup in rs.ruleRoot.rules.Values)
                {
                    foreach (var rule in rulegroup)
                        rule.IsUnknown = true;
                }
            }
        }

    }

    /// <summary>
    /// Set of data and parameters derived from the source data and the rule set for machine learning
    /// </summary>
    public class PreparedLearningSet
    {
        public string pattern { get; set; } = string.Empty;
        public Dictionary<string, string> inputs { get; set; } = new Dictionary<string, string>();

        public string outputPath { get; set; } = string.Empty;
        public string ruleset { get; set; } = "";
        public RuleSetNode ActiveRuleset { get; set; } = null;

        //check for ruleset with matching name to load.
        public RuleRootNode rroot { get; set; }
        public MapRootNode root { get; set; }
        public SourceSpan ruleSetContents { get; set; }
        public bool jsonData { get; set; } = false;

        public int patternCount { get; set; }
        public List<InputDefinitionNode> inps { get; set; }
        public OutputDefinitionNode outp { get; set; }

        public List<OutputDefinitionNode> outps { get; set; }

        public int sets { get; set; }

        public List<int> inSamplePatterns { get; set; } = new List<int>();
        public List<int> outSamplePatterns { get; set; } = new List<int>();
        public IEnumerable<JToken> jPatterns { get; internal set; }
        public IEnumerable<XElement> xPatterns { get; internal set; }
    }
}
