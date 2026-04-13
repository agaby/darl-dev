/// </summary>

﻿using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase.Meta
{
    public static class DarlMetaParseTreeExtensions
    {

        /// Extension method converting a parse tree back to formatted source code
        /// </summary>
        /// <param name="parseTree">The tree to convert</param>
        /// <returns>The source code</returns>
        public static string ToDarl(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return string.Empty;
            return TermToDarl(parseTree.Root.AstNode as DarlMetaNode);
        }


        /// Recursively builds a string from the tree of nodes
        /// </summary>
        /// <param name="node">The current parent node</param>
        /// <returns>A source code string</returns>
        public static string TermToDarl(this DarlMetaNode node)
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

        public static List<InputDefinitionNode> GetInputs(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var root = parseTree.Root.AstNode as MetaRootNode;
            return new List<InputDefinitionNode>(root.inputs.Values);
        }

        /// Gets the input range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The numeric range of the input - based on any sets defined</returns>
        public static DarlResult GetInputRange(this ParseTree parseTree, string name)
        {
            var input = GetInputs(parseTree).Where(a => a.name == name).FirstOrDefault();
            if (input == null)
                input = GetInputs(parseTree).Where(a => a.name == "response").FirstOrDefault();
            DarlResult res = new DarlResult(0.0, true);
            if (!(res is null))
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
        /// Gets the practical input range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The numeric range of the input - based on any sets defined</returns>
        public static DarlResult GetPracticalInputRange(this ParseTree parseTree, string name)
        {
            var input = GetInputs(parseTree).Where(a => a.name == name).FirstOrDefault();
            DarlResult res = new DarlResult(0.0, true);
            if (!(res is null))
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

        /// Clears inputs of an existing tree so it can be re-used
        /// </summary>
        /// <param name="tree">The tree</param>
        public static void ClearInputs(this ParseTree tree)
        {
            var root = tree.Root.AstNode as MetaRootNode;
            foreach (var inp in root.inputs.Values)
            {
                inp.Value = new DarlResult(0.0, true);
            }
            foreach (var rulegroup in root.rules.Values)
            {
                foreach (var rule in rulegroup)
                    rule.IsUnknown = true;
            }
        }

    }
}
