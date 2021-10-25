using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase.Meta
{
    public static class DarlMetaParseTreeExtensions
    {
        public static List<InputDefinitionNode> GetInputs(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null)
                return null;
            var root = parseTree.Root.AstNode as MetaRootNode;
            return new List<InputDefinitionNode>(root.inputs.Values);
        }

        /// <summary>
        /// Gets the input range.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <param name="name">The name.</param>
        /// <returns>The numeric range of the input - based on any sets defined</returns>
        public static DarlResult GetInputRange(this ParseTree parseTree, string name)
        {
            var input = GetInputs(parseTree).Where(a => a.name == name).FirstOrDefault();
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
        /// <summary>
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

        /// <summary>
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
