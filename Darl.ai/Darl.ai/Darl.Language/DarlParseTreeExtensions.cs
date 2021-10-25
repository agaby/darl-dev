using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarlLanguage
{
    /// <summary>
    /// Darl Parse tree extensions
    /// </summary>
    public static class DarlParseTreeExtensions
    {
        /// <summary>
        /// Extension method converting a parse tree back to formatted source code
        /// </summary>
        /// <param name="parseTree">The tree to convert</param>
        /// <returns>The source code</returns>
        public static string ToDarl(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return string.Empty;
            return TermToDarl(parseTree.Root.AstNode as DarlNode);
        }


        /// <summary>
        /// Recursively builds a string from the tree of nodes
        /// </summary>
        /// <param name="node">The current parent node</param>
        /// <returns>A source code string</returns>
        public static string TermToDarl(this DarlNode node)
        {
            StringBuilder sb = new StringBuilder();
            if (node != null)
            {
                sb.Append(node.preamble);
                int childcount = 0;
                foreach (var child in node.ChildNodes)
                {
                    childcount++;
                    if (child is DarlNode)
                    {
                        sb.Append(TermToDarl(child as DarlNode));
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
        /// Gets the map root node.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>The root of the parse tree</returns>
        public static MapRootNode GetMapRoot(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            return parseTree.Root.AstNode as MapRootNode;
        }

        /// <summary>
        /// Gets the map inputs.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>A list of map input definitions</returns>
        public static List<MapInputDefinitionNode> GetMapInputs(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var root = parseTree.Root.AstNode as MapRootNode;
            return new List<MapInputDefinitionNode>(root.inputs.Values);
        }

        /// <summary>
        /// Gets the map outputs.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>A list of map output definitions</returns>
        public static List<MapOutputDefinitionNode> GetMapOutputs(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var root = parseTree.Root.AstNode as MapRootNode;
            return new List<MapOutputDefinitionNode>(root.outputs.Values);
        }

        /// <summary>
        /// Get the map stores
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>A list of map store definitions</returns>
        public static List<MapStoreDefinitionNode> GetMapStores(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var root = parseTree.Root.AstNode as MapRootNode;
            return new List<MapStoreDefinitionNode>(root.stores.Values);
        }

        /// <summary>
        /// Gets the type of the map input.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The type of the chosen input</returns>
        public static string GetMapInputType(this ParseTree parseTree, string name)
        {
            var inputs = GetInputs(parseTree, name);
            if (inputs.Count >= 1)
                return inputs[0].iType.ToString();
            return string.Empty;

        }

        /// <summary>
        /// Gets the type of the map output.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The type of the chosen output</returns>
        public static string GetMapOutputType(this ParseTree parseTree, string name)
        {
            var outputs = GetOutputs(parseTree, name);
            if (outputs.Count >= 1 && outputs[0] is OutputDefinitionNode)
                return ((OutputDefinitionNode)outputs[0]).iType.ToString();
            return string.Empty;
        }

        /// <summary>
        /// Gets the map output range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The numeric range of the output - based on any sets defined</returns>
        public static DarlResult GetMapOutputRange(this ParseTree parseTree, string name)
        {
            var outputs = GetOutputs(parseTree, name);
            DarlResult res = new DarlResult(0.0, true);
            foreach (var output in outputs)
            {
                if (output is OutputDefinitionNode)
                {
                    if (((OutputDefinitionNode)output).iType == OutputDefinitionNode.OutputTypes.numeric_output)
                    {
                        foreach (var set in output.sets.Values)
                        {
                            res = res.IsUnknown() ? set : DarlResult.Support(res, set);
                        }
                    }
                }

            }
            return res;

        }

        /// <summary>
        /// Gets the map input range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>THe numeric range of the input - based on any sets defined</returns>
        public static DarlResult GetMapInputRange(this ParseTree parseTree, string name)
        {
            var inputs = GetInputs(parseTree, name);
            DarlResult res = new DarlResult(0.0, true);
            foreach (var input in inputs)
            {
                if (input.iType == InputDefinitionNode.InputTypes.numeric_input)
                {
                    foreach (var set in input.sets.Values)
                    {
                        res = res.IsUnknown() ? set : DarlResult.Support(res, set);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Gets the map practical input range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>THe numeric range of the input - based on any sets defined</returns>
        public static DarlResult GetMapPracticalInputRange(this ParseTree parseTree, string name)
        {
            var inputs = GetInputs(parseTree, name);
            DarlResult res = new DarlResult(0.0, true);
            foreach (var input in inputs)
            {
                if (input.iType == InputDefinitionNode.InputTypes.numeric_input)
                {
                    foreach (var set in input.sets.Values)
                    {
                        res = res.IsUnknown() ? set : DarlResult.PracticalSupport(res, set);
                    }
                }
            }
            return res;
        }

        public static List<ConstantDefinitionNode> GetSingleRuleSetConstants(this ParseTree tree)
        {
            return GetSingleRuleRoot(tree).constants.Values.ToList();
        }

        public static List<DurationDefinitionNode> GetSingleRuleSetPeriods(this ParseTree tree)
        {
            return GetSingleRuleRoot(tree).durations.Values.ToList();
        }


        public static List<StringDefinitionNode> GetSingleRuleSetStrings(this ParseTree tree)
        {
            return GetSingleRuleRoot(tree).strings.Values.ToList();
        }

        public static List<SequenceDefinitionNode> GetSingleRuleSetSequences(this ParseTree tree)
        {
            return GetSingleRuleRoot(tree).sequences.Values.ToList();
        }

        public static List<ConstantDefinitionNode> GetRuleSetConstants(this ParseTree tree, string ruleset)
        {
            return GetRuleRoot(tree, ruleset).constants.Values.ToList();
        }

        public static List<DurationDefinitionNode> GetRuleSetPeriods(this ParseTree tree, string ruleset)
        {
            return GetRuleRoot(tree, ruleset).durations.Values.ToList();
        }

        public static List<StringDefinitionNode> GetRuleSetStrings(this ParseTree tree, string ruleset)
        {
            return GetRuleRoot(tree, ruleset).strings.Values.ToList();
        }

        public static List<SequenceDefinitionNode> GetRuleSetSequences(this ParseTree tree, string ruleset)
        {
            return GetRuleRoot(tree, ruleset).sequences.Values.ToList();
        }

        public static double GetRuleSetConstant(this ParseTree tree, string ruleset, string name)
        {
            return GetRuleRoot(tree, ruleset).constants[name].Value;
        }

        public static TimeSpan GetRuleSetPeriod(this ParseTree tree, string ruleset, string name)
        {
            return GetRuleRoot(tree, ruleset).durations[name].Value;
        }

        public static string GetRuleSetString(this ParseTree tree, string ruleset, string name)
        {
            return GetRuleRoot(tree, ruleset).strings[name].Value;
        }

        public static List<List<string>> GetRuleSetSequence(this ParseTree tree, string ruleset, string name)
        {
            return GetRuleRoot(tree, ruleset).sequences[name].Value;
        }


        private static RuleRootNode GetSingleRuleRoot(ParseTree tree)
        {
            var root = tree.Root.AstNode as MapRootNode;
            if (root.rulesets.Count != 1)
                throw new System.Exception("This method only works with single ruleset maps.");
            return root.rulesets.First().Value.ruleRoot;
        }

        private static RuleRootNode GetRuleRoot(ParseTree tree, string ruleset)
        {
            var root = tree.Root.AstNode as MapRootNode;
            if (!root.rulesets.ContainsKey(ruleset))
            {
                throw new ArgumentException($"ruleset {ruleset} not found");
            }
            return root.rulesets[ruleset].ruleRoot;
        }


        public static List<string> GetSingleRuleSetTextualRHS(this ParseTree tree, string output)
        {
            var list = new List<string>();
            var root = GetSingleRuleRoot(tree);
            if (root.rules.ContainsKey(output))
            {
                if (root.rules[output].Count > 0)
                {
                    foreach (var rule in root.rules[output])
                    {
                        if (rule.rhs.IsConstant())
                        {
                            list.Add(((DarlNumberLiteralNode)rule.rhs).FixedResult.Value.ToString());
                        }
                        else if (rule.rhs is RandomTextNode)
                        {
                            foreach (var r in rule.rhs.ChildNodes)
                            {
                                if (r is DarlNumberLiteralNode)
                                {
                                    list.Add(((DarlNumberLiteralNode)r).FixedResult.Value.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static void ChangeSingleRuleSetTextualRHS(this ParseTree tree, string output, List<string> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count < 1)
                throw new ArgumentOutOfRangeException(nameof(values), "values must contain at least one string");
            var root = GetSingleRuleRoot(tree);
            if (root.rules.ContainsKey(output))
            {
                if (root.rules[output].Count > 0)
                {
                    foreach (var rule in root.rules[output])
                    {
                        if (rule.rhs.IsConstant() && values.Count == 1)
                        {
                            ((DarlNumberLiteralNode)rule.rhs).FixedResult = new DarlResult("", values[0], DarlResult.DataType.textual);
                        }
                        else if (rule.rhs is RandomTextNode && values.Count > 1)
                        {
                            rule.rhs.ChildNodes.Clear();
                            foreach (var s in values)
                            {
                                rule.rhs.ChildNodes.Add(new DarlNumberLiteralNode { Parent = rule.rhs, FixedResult = new DarlResult("", s, DarlResult.DataType.textual) });
                            }
                        }
                        else if (rule.rhs.IsConstant() && values.Count > 1) //convert text rhs to randomtext
                        {
                            var index = rule.ChildNodes.FindIndex(a => a.ToString() == rule.rhs.ToString());
                            rule.rhs = new RandomTextNode { Parent = rule };
                            rule.rhs.ChildNodes.Clear();
                            foreach (var s in values)
                            {
                                rule.rhs.ChildNodes.Add(new DarlNumberLiteralNode { Parent = rule.rhs, FixedResult = new DarlResult("", s, DarlResult.DataType.textual) });
                            }
                            rule.ChildNodes[index] = rule.rhs;
                        }
                        else if (rule.rhs is RandomTextNode && values.Count == 1) //convert randomtext to text rhs
                        {
                            var index = rule.ChildNodes.FindIndex(a => a.ToString() == rule.rhs.ToString());
                            rule.rhs = new DarlNumberLiteralNode { FixedResult = new DarlResult("", values[0], DarlResult.DataType.textual), Parent = rule };
                            rule.ChildNodes[index] = rule.rhs;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// delete all the rules associated with an output
        /// </summary>
        /// <param name="tree"></param> 
        /// <param name="output"></param>
        public static void DeleteSingleRulesetRules(this ParseTree tree, string output)
        {
            var root = GetSingleRuleRoot(tree);
            if (root.rules.ContainsKey(output))
            {
                foreach (var x in root.rules[output])
                {
                    var index = root.ChildNodes.FindIndex(a => a == x);
                    root.ChildNodes.RemoveAt(index);
                }
                root.rules[output] = new List<RuleNode>();
            }
        }


        /// <summary>
        /// Gets the map output categories.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>List of categories for that output - set names if numeric</returns>
        public static List<string> GetMapOutputCategories(this ParseTree parseTree, string name)
        {
            HashSet<string> cats = new HashSet<string>();
            var outputs = GetOutputs(parseTree, name);
            foreach (var output in outputs)
            {
                if (output is OutputDefinitionNode)
                {

                    if (((OutputDefinitionNode)output).iType == OutputDefinitionNode.OutputTypes.numeric_output)
                    {
                        foreach (var cat in output.sets.Keys)
                            cats.Add(cat);
                    }
                    else if (((OutputDefinitionNode)output).iType == OutputDefinitionNode.OutputTypes.categorical_output)
                    {
                        foreach (var cat in output.categories)
                            cats.Add(cat);
                    }
                }
            }
            return cats.ToList();
        }

        /// <summary>
        /// Gets the map input categories.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>List of categories for that input - set names if numeric</returns>
        public static List<string> GetMapInputCategories(this ParseTree parseTree, string name)
        {
            HashSet<string> cats = new HashSet<string>();
            var inputs = GetInputs(parseTree, name);
            foreach (var input in inputs)
            {
                if (input.iType == InputDefinitionNode.InputTypes.numeric_input)
                {
                    foreach (var cat in input.sets.Keys)
                        cats.Add(cat);
                }
                else if (input.iType == InputDefinitionNode.InputTypes.categorical_input)
                {
                    foreach (var cat in input.categories)
                        cats.Add(cat);
                }
                else if (input.iType == InputDefinitionNode.InputTypes.dynamic_categorical_input)
                {
                    //in this case find the associated store and call it.
                    foreach (var cat in input.categories)
                        cats.Add(cat);
                }
            }
            return cats.ToList();
        }

        /// <summary>
        /// Get the ruleset inputs connected to this map input in this tree
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name of the associated map input.</param>
        /// <returns>
        /// A list of input definitions
        /// </returns>
        public static List<InputDefinitionNode> GetInputs(this ParseTree parseTree, string name)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            List<InputDefinitionNode> inputs = new List<InputDefinitionNode>();
            var root = parseTree.Root.AstNode as MapRootNode;
            if (root.inputs.ContainsKey(name))
            {
                foreach (
                    WireDefinitionNode wire in
                        root.wires.Where(a => a.sourcename == name && string.IsNullOrEmpty(a.sourceRuleset) && !string.IsNullOrEmpty(a.destRuleset)))
                {
                    var rs = root.rulesets[wire.destRuleset];
                    inputs.Add(rs.ruleRoot.inputs[wire.destname]);
                }
            }
            return inputs;
        }

        /// <summary>
        /// Gets the ruleset outputs connected to this map output
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name of the associated map output</param>
        /// <returns>A list of output definitions</returns>
        public static List<IOSequenceDefinitionNode> GetOutputs(this ParseTree parseTree, string name)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var outputs = new List<IOSequenceDefinitionNode>();
            var root = parseTree.Root.AstNode as MapRootNode;
            if (root.outputs.ContainsKey(name))
            {
                foreach (
                    WireDefinitionNode wire in
                        root.wires.Where(a => a.destname == name && string.IsNullOrEmpty(a.destRuleset) && !string.IsNullOrEmpty(a.sourceRuleset)))
                {
                    var rs = root.rulesets[wire.sourceRuleset];
                    outputs.Add(rs.ruleRoot.outputs[wire.sourcename]);
                }
            }
            return outputs;
        }

        /// <summary>
        /// Create an HTML Version of a parse tree
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>The HTML</returns>
        public static string ToHtml(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return string.Empty;
            int currentTokenindex = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("<pre>");
            bool skip = false;
            //copy from source to output any characters while looking for tokens. Convert cr/lf
            int currentCharIndex = 0;
            var token = parseTree.Tokens[currentTokenindex];
            string closing = string.Empty;
            foreach (var c in parseTree.SourceText)
            {
                if (currentCharIndex == token.Location.Position)
                {
                    //emit appropriate markup
                    switch (token.Category)
                    {
                        case TokenCategory.Comment:
                            sb.Append("<span class=\"text-success\">");
                            closing = "</span>";
                            break;
                        case TokenCategory.Content:
                            switch (token.EditorInfo.Type)
                            {
                                case TokenType.Keyword:
                                case TokenType.Operator:
                                    sb.Append("<span class=\"text-info\" title=\"" + token.EditorInfo.ToolTip + "\">");
                                    closing = "</span>";
                                    break;
                                case TokenType.String:
                                case TokenType.Text:
                                    sb.Append("<span class=\"text-danger\">");
                                    closing = "</span>";
                                    break;
                            }
                            break;
                    }
                }
                else if (currentCharIndex == token.Location.Position + token.Length)
                {
                    //emit closing markup and move to next token
                    sb.Append(closing);
                    currentTokenindex++;
                    if (currentTokenindex < parseTree.Tokens.Count)
                    {
                        token = parseTree.Tokens[currentTokenindex];
                    }
                }
                if (c == '\r' || c == '\n')//not twice if cr/lf
                {
                    if (!skip)
                        sb.Append("<br/>");
                    if (currentCharIndex + 1 < parseTree.SourceText.Count())
                    {
                        if (parseTree.SourceText[currentCharIndex + 1] == '\n' && c == '\r')//if this is a cr/lf pnly one line break.
                            skip = true; //skip next character
                        else
                            skip = false;
                    }
                    else
                        skip = false;
                }
                else
                {
                    sb.Append(c);
                }

                currentCharIndex++;
            }

            sb.Append("</pre>");

            return sb.ToString();

        }

    }
}
