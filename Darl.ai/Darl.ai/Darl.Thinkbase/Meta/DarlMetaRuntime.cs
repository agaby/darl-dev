using Darl.Licensing;
using DarlCompiler.Parsing;
using Microsoft.Extensions.Configuration;
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

        public  bool licensed { get; private set; } = false;

        public  void SetLicense(string license)
        {
            licensed = DarlLicense.ProcessLicense(license);
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
        public virtual async Task Evaluate(ParseTree parseTree, List<DarlResult> inputs, KnowledgeState ks)
        {
            LicenseCheck();
            grammar.results = inputs;
            grammar.state = ks;
            await grammar.RunSample(new RunSampleArgs(language, string.Empty, parseTree));
        }

        private void LicenseCheck()
        {
            if (!licensed)
                throw new MetaRuleException("This library is not licensed! Got to https://darl.ai to obtain a license.");
        }
    }
}
