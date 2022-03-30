using Darl.Common;
using Darl.Licensing;
using DarlCompiler.Parsing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DarlMetaRunTime : IDarlMetaRunTime
    {

        /// <summary>
        /// The DARL grammar object
        /// </summary>
        protected DarlMetaGrammar grammar;
        /// <summary>
        /// The language data
        /// </summary>
        protected LanguageData language;
        /// <summary>
        /// The DARL parser
        /// </summary>
        protected Parser parser;

        public string licenseLocation { get; set; } = "licensing:darlMetaLicense";

        public bool licensed { get; private set; } = false;

        public void SetLicense(string license)
        {
            licensed = DarlLicense.ProcessLicense(license);
        }

        public void SetEvaluationTime(List<DarlTime> now)
        {
            grammar.now = now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlRunTime"/> class.
        /// </summary>
        public DarlMetaRunTime(IConfiguration config, IMetaStructureHandler structure)
        {
            grammar = new DarlMetaGrammar();
            grammar.structure = structure;
            language = new LanguageData(grammar);
            parser = new Parser(language);
            SetLicense(config[licenseLocation]);
        }

        /// <summary>
        /// Create a tree to be used for repeated evaluations.
        /// </summary>
        /// <param name="source">The DARL Meta source</param>
        /// <returns>The tree</returns>
        /// <exception cref="MetaRuleException">Thrown for syntax errors in source.</exception>
        public ParseTree CreateTree(string source, GraphObject node, IGraphModel model)
        {
            LicenseCheck();
            grammar.currentNode = node;
            grammar.currentModel = model;
            ParseTree parseTree = parser.Parse(source);
            if (parseTree.Status == ParseTreeStatus.Error)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var message in parseTree.ParserMessages)
                {
                    sb.AppendLine(string.Format("{0}: {1}, line: {2} column: {3}", message.Level.ToString(), message.Message, message.Location.Line, message.Location.Column));
                }
                throw new MetaRuleException(string.Format("Error in source: {0}", sb.ToString()));
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
            LicenseCheck();
            return parser.Parse(source);
        }

        /// <summary>
        /// First pass determines list of dependencies
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<GraphObject> ExploreGraph(ParseTree tree)
        {
            return (tree.Root.AstNode as MetaRootNode).dependentGraphObjects.ToList();
        }

        /// <summary>
        /// Calculates the saliences.
        /// </summary>
        /// <param name="currentState">Current state of the  inputs and outputs</param>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>
        /// A dictionary of inputs not yet given values, for which values are still required, and their saliences if &gt; 0.
        /// </returns>
        public Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState, ParseTree tree)
        {
            return (tree.Root.AstNode as MetaRootNode).CalculateSaliences(currentState);
        }

        public SalienceRecord CalculateKGSaliences(List<SalienceRecord> saliences, KnowledgeState ks, ParseTree tree)
        {
            return (tree.Root.AstNode as MetaRootNode).CalculateKGSaliences(saliences, ks);
        }


        /// <summary>
        /// Extension method converting a parse tree back to formatted source code
        /// </summary>
        /// <param name="parseTree">The tree to convert</param>
        /// <returns>The source code</returns>
        public string ToDarl(ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return string.Empty;
            return TermToDarl(parseTree.Root.AstNode as DarlMetaNode);
        }


        /// <summary>
        /// Recursively builds a string from the tree of nodes
        /// </summary>
        /// <param name="node">The current parent node</param>
        /// <returns>A source code string</returns>
        public string TermToDarl(DarlMetaNode node)
        {
            StringBuilder sb = new StringBuilder();
            if (node != null)
            {
                sb.Append(node.preamble);
                int childcount = 0;
                foreach (var child in node.ChildNodes)
                {
                    childcount++;
                    if (child is DarlMetaNode)
                    {
                        sb.Append(TermToDarl(child as DarlMetaNode));
                        if (childcount < node.ChildNodes.Count)
                        {
                            sb.Append(node.midamble);
                        }
                    }
                }
                sb.Append(node.postamble);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Evaluate an existing tree on a given node updating the knowledge state
        /// </summary>
        /// <param name="parseTree">The tree to evaluate</param>
        /// <param name="inputs">a set of input values</param>
        /// <param name="ks">The knowledge state to use and modify</param>
        public virtual async Task Evaluate(ParseTree parseTree, List<DarlResult> inputs, KnowledgeState ks, FuzzyTime? evalTime = null)
        {
            LicenseCheck();
            grammar.results = inputs;
            grammar.state = ks;
            grammar.now = evalTime == null ? null : evalTime.darlTimes; // not reentrant - fix. 
            await grammar.RunSample(new RunSampleArgs(language, string.Empty, parseTree));
        }

        private void LicenseCheck()
        {
            if (!licensed)
                throw new MetaRuleException("This library is not licensed! Got to https://darl.ai to obtain a license.");
        }

        public DarlMineReport MineSupervised(PreparedLearningSet ps)
        {
            var rep = new DarlMineReport();
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
            foreach (var inpName in inList)
            {
                var inp = ps.rroot.inputs[inpName];
                ruleString += inp.TermToDarl();
            }
            //write out inputs sorted alphabetically
            var outList = new List<string>(ps.rroot.outputs.Keys);
            outList.Sort();
            foreach (var outpName in outList)
            {
                if (outpName != ps.outp.name)
                {
                    var inp = ps.rroot.outputs[outpName];
                    ruleString += inp.TermToDarl();
                }
            }
            ruleString += "\n";
            //now the single output
            ruleString += ps.outp.TermToDarl() + "\n";
            //insert any constants
            foreach(var c in ps.rroot.constants.Values)
            {
                ruleString += c.TermToDarl();
            }
            foreach (var c in ps.rroot.durations.Values)
            {
                ruleString += c.TermToDarl();
            }
            foreach (var c in ps.rroot.lineages.Values)
            {
                if(!grammar.structure.CommonLineages.ContainsKey(c.name))
                    ruleString += c.TermToDarl();
            }
            foreach (var c in ps.rroot.strings.Values)
            {
                ruleString += c.TermToDarl();
            }
            //insert any existing rules
            foreach (var rules in ps.rroot.rules.Values)
            {
                foreach(var r in rules)
                    ruleString += r.TermToDarl();
            }
            rootDecisioNode.GenerateRules(ref ruleString, "", 0);
            //insert rules into ruleset outline at insertion point
            newRuleSource = ps.ruleset.Remove(ps.ruleSetContents.Location.Position, ps.ruleSetContents.Length + 1);
            newRuleSource = newRuleSource.Insert(ps.ruleSetContents.Location.Position, ruleString);
            //now re-parse the new ruleset and test
            ParseTree newTree = CreateTree(newRuleSource, ps.targetNode, ps.model);
            if (newTree.Status == ParseTreeStatus.Error)
            {
                throw new MetaRuleException("Internal error: new ruleset cannot be parsed.");
            }
            double inSampleMeasure = 0;
            double outSampleMeasure = 0;
            int unknowns = 0;
            Test(ps, newTree, out inSampleMeasure, out outSampleMeasure);
            string commentString = string.Format("// Generated by DARL rule induction on  {0}.\n", DateTime.Now.ToString());
            double iS;
            double oS = 0.0;
            if (ps.outp.oType == OutputDefinitionNode.OutputTypes.categorical_output)
            {
                iS = inSampleMeasure * 100.0 / ps.inSamplePatterns.Count;
                commentString += string.Format("// Train correct:  {0}% on {1} patterns.\n", iS.ToString("0.00"), ps.inSamplePatterns.Count.ToString());
                if (ps.percentTrain < 100)
                {
                    oS = outSampleMeasure * 100.0 / ps.outSamplePatterns.Count;
                    commentString += string.Format("// Test correct:  {0}% on {1} patterns.\n", oS.ToString("0.00"), ps.outSamplePatterns.Count.ToString());
                }
            }
            else
            {
                iS = Math.Sqrt(inSampleMeasure / ps.inSamplePatterns.Count);
                commentString += string.Format("// Train RMS error:  {0} on {1} patterns.\n", iS.ToString("0.00"), ps.inSamplePatterns.Count.ToString());
                if (ps.percentTrain < 100)
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
                rep.trainPercent = ps.percentTrain;
                rep.trainPerformance = iS;
                rep.testPerformance = oS;
                rep.unknownResponsePercent = unknowns * 100.0 / ps.patternCount;
                rep.code = newRuleSource;
            }

            return rep;
        }

        private int Test(PreparedLearningSet ps, ParseTree newTree, out double inSampleMeasure, out double outSampleMeasure)
        {
            int unknowns = 0;
            int index = 0;
            inSampleMeasure = 0.0;
            outSampleMeasure = 0.0;
            foreach (var ks in ps.knowledgeStates)
            {
                double score = 0.0;
                var existing = ks.GetAttribute(ps.targetNodeId, ps.valueLineage)?.value;
                var results = new List<DarlResult>();
                Evaluate(newTree, results, ks).Wait();
                var predicted = ks.GetAttribute(ps.targetNodeId, ps.valueLineage);
                if(predicted != null && predicted.type == GraphAttribute.DataType.categorical)
                {
                    if(predicted.confidence > 0.1)
                    {
                        score = predicted.value == existing ? 1.0 : 0.0;
                    }
                    else
                    {
                        unknowns++;
                    }
                }
                else if(predicted != null && predicted.type == GraphAttribute.DataType.numeric)
                {
                    if (predicted.confidence > 0.1)
                    {
                        try
                        {
                            var expectedD = Convert.ToDouble(existing);
                            score = Convert.ToDouble(predicted.value) - expectedD;
                            score *= score;
                        }
                        catch { }
                    }
                    else
                    {
                        unknowns++;
                    }
                }
                if (ps.inSamplePatterns.Contains(index))
                {
                    inSampleMeasure += score;
                }
                else
                {
                    outSampleMeasure += score;
                }
                index++;
            }
            return unknowns;
        }


    }
}
