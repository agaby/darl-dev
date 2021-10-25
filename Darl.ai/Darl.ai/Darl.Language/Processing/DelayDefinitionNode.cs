using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Defines a delay
    /// </summary>
    public class DelayDefinitionNode : DarlNode
    {
        /// <summary>
        /// Gets the sourcename.
        /// </summary>
        /// <value>
        /// The sourcename.
        /// </value>
        public string sourcename { get; private set; }
        /// <summary>
        /// Gets the destname.
        /// </summary>
        /// <value>
        /// The destname.
        /// </value>
        public string destname { get; private set; }

        private int writeIndex = 0;


        /// <summary>
        /// Gets the dest ruleset.
        /// </summary>
        /// <value>
        /// The dest ruleset.
        /// </value>
        public string destRuleset { get; private set; }

        /// <summary>
        /// Gets the composed name
        /// </summary>
        /// <value>
        /// The comp dest.
        /// </value>
        public string compDest
        {
            get
            {
                if (string.IsNullOrEmpty(destRuleset))
                    return destname;
                else
                    return destRuleset + "." + destname;
            }
        }

        /// <summary>
        /// Gets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public DarlResult schedule { get; private set; }


        /// <summary>
        /// Gets or sets the history.
        /// </summary>
        /// <value>
        /// The history.
        /// </value>
        public List<List<DarlResult>> History { get; set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                var childAst = AddChild("-", child);
                switch (childAst.AsString)
                {
                    case "map_output":
                        sourcename = ((DarlIdentifierNode)childAst).name;
                        break;
                    case "compcategoryinput":
                    case "compnumericinput":
                        destRuleset = ((DarlIdentifierNode)childAst.ChildNodes[0]).name;
                        destname = ((DarlIdentifierNode)childAst.ChildNodes[1]).name;
                        break;
                    case "Expression list":
                        {
                            List<double> vals = new List<double>();
                            foreach (var ch in childAst.ChildNodes)
                            {
                                vals.Add((double)((DarlNumberLiteralNode)ch).FixedResult.values[0]);
                            }
                            schedule = new DarlResult(vals);
                        }
                        break;
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
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlGrammar grammar = thread.Runtime.Language.Grammar as DarlGrammar;
            if (History != null) // in a simulation rather than a simple evaluation
            {
                var dest = grammar.LastResultByName(compDest);
                int currentState = History.Count - 1; //most recent index
                switch (schedule.HowFuzzy())
                {
                    case DarlResult.Fuzzyness.singleton:
                        if ((double)schedule.values[0] < History.Count)
                        {
                            var previousState = History[currentState - Convert.ToInt32(schedule.values[0])];
                            if (previousState.Any(a => a.name == sourcename))
                            {
                                grammar.results.RemoveAll(a => a.name == compDest);
                                grammar.results.Add(new DarlResult(compDest, previousState.First(a => a.name == sourcename)));
                            }
                        }
                        break;
                    case DarlResult.Fuzzyness.interval:
                        for (int n = Convert.ToInt32(schedule.values[0]); n <= Convert.ToInt32(schedule.values[1]); n++)
                        {
                            Transfer(n, currentState, dest);
                        }
                        dest.Simplify(new OutputDefinitionNode { confidence = 0.0 });
                        break;
                    case DarlResult.Fuzzyness.triangle:
                        for (int n = Convert.ToInt32(schedule.values[0]); n <= Convert.ToInt32(schedule.values[2]); n++)
                        {
                            Transfer(n, currentState, dest);
                        }
                        dest.Simplify(new OutputDefinitionNode { confidence = 0.0 });
                        break;
                    case DarlResult.Fuzzyness.trapezoid:
                        for (int n = Convert.ToInt32(schedule.values[0]); n <= Convert.ToInt32(schedule.values[3]); n++)
                        {
                            Transfer(n, currentState, dest);
                        }
                        dest.Simplify(new OutputDefinitionNode { confidence = 0.0 });
                        break;

                    case DarlResult.Fuzzyness.unknown:
                        break;
                }
            }
            
            thread.CurrentNode = Parent; //standard epilogue
            return Task.FromResult<object>(null);
        }

        private void Transfer(int n, int currentState, DarlResult dest)
        {
            if (n < History.Count)
            {
                var previousState = History[currentState - n];
                if (previousState.Any(a => a.name == sourcename))
                    dest.FuzzyAggregate(previousState.First(a => a.name == sourcename));//Todo: apply weight slopes
            }

        }
        public override string preamble
        {
            get
            {
                return "delay ";
            }
        }


        public override string postamble
        {
            get
            {
                writeIndex = 0;
                var values = new List<string>();
                foreach (double v in schedule.values)
                    values.Add(v.ToString());
                return $"{{{string.Join(",",values)}}};";
            }
        }
    }
}
