using DarlCommon;
using DarlLanguage;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Lineage.Bot
{
    public static class BotFormatExtensions
    {
        public static string ToDarl(this BotFormat bf)
        {
            var sb = new StringBuilder();
            foreach (var i in bf.InputFormatList)
            {
                sb.AppendLine(i.ToDarl());
            }
            foreach (var s in bf.Stores)
                sb.AppendLine($"\tstore {s};");

            foreach (var o in bf.OutputFormatList)
            {
                sb.AppendLine(o.ToDarl());
            }
            //emit strings, constants and sequences
            foreach (var c in bf.Constants.Keys)
            {
                sb.AppendLine($"\tconstant {c} {bf.Constants[c].ToString()};");
            }
            foreach (var c in bf.Strings.Keys)
            {
                sb.AppendLine($"\tstring {c} {bf.Constants[c]};");
            }
            foreach (var c in bf.Sequences.Keys)
            {
                sb.Append($"sequence {c} ");
                sb.Append("{");
                var p = bf.Sequences[c];
                int outercount = 0;
                foreach (var s in p)
                {
                    sb.Append("{");
                    int innercount = 0;
                    foreach (var ss in s)
                    {
                        innercount++;
                        sb.Append(innercount == s.Count ? $"{{\"{ss}\"}}" : $"{{\"{ss}\"}},");
                    }
                    outercount++;
                    sb.Append(outercount == p.Count ? "}" : "},");
                }
                sb.Append("};");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Create a BotFormat class from a given DARL file
        /// </summary>
        /// <param name="darl">the source</param>
        /// <returns>The new class</returns>
        /// <remarks>Currently only works with a single ruleset Darl map.</remarks>
        /// <exception cref="Exception">If not exactly one rule set.</exception>
        public static BotFormat ToBotFormat(string darl)
        {
            var bf = new BotFormat();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(darl);
            if (!tree.HasErrors())
            {
                bf.InputFormatList = new List<BotInputFormat>();
                bf.OutputFormatList = new List<BotOutputFormat>();
                foreach (var i in tree.GetMapInputs())
                {
                    var inputs = new HashSet<InputDefinitionNode>();
                    foreach (var s in tree.GetInputs(i.Name))
                    {
                        if (!inputs.Contains(s))
                        {
                            bf.InputFormatList.Add(BotInputFormatExtensions.ToBotFormat(s));
                            inputs.Add(s);
                        }
                    }
                }
                foreach (var i in tree.GetMapOutputs())
                {
                    var outputs = new HashSet<IOSequenceDefinitionNode>();
                    foreach (var s in tree.GetOutputs(i.Name))
                    {
                        if (!outputs.Contains(s))
                        {
                            bf.OutputFormatList.Add(BotOutputFormatExtensions.ToBotFormat(s));
                            outputs.Add(s);
                        }
                    }
                }
                //do stores, constants strings and sequences.
                foreach (var i in tree.GetMapStores())
                {
                    bf.Stores.Add(i.Name);
                }
                foreach (var c in tree.GetSingleRuleSetConstants())
                {
                    bf.Constants.Add(c.name, c.Value);
                }
                foreach (var c in tree.GetSingleRuleSetStrings())
                {
                    bf.Strings.Add(c.name, c.Value);
                }
                foreach (var c in tree.GetSingleRuleSetSequences())
                {
                    bf.Sequences.Add(c.name, c.Value);
                }
            }
            return bf;
        }

    }
}
