/// </summary>

﻿using DarlCommon;
using DarlLanguage.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Lineage.Bot
{
    public static class BotOutputFormatExtensions
    {
        public static string ToDarl(this BotOutputFormat bof)
        {
            var darl = $"\toutput {bof.OutputType.ToString()} {bof.Name}";
            switch (bof.OutputType)
            {
                case OutputFormat.OutType.categorical:
                    if (bof.Categories != null && bof.Categories.Any())
                    {
                        var sb = new StringBuilder();
                        sb.Append("{");
                        for (int n = 0; n < bof.Categories.Count; n++)
                        {
                            if (n < bof.Categories.Count - 1)
                            {
                                sb.Append($"{WrapInvalidIdentifier(bof.Categories[n])}, ");
                            }
                            else
                            {
                                sb.Append(WrapInvalidIdentifier(bof.Categories[n]));
                            }
                        }
                        sb.Append("}");
                        darl = $"{darl} {sb.ToString()};";
                    }
                    else
                        darl = $"{darl};";
                    break;
                case OutputFormat.OutType.numeric:
                    if (bof.Sets != null && bof.Sets.Any())
                    {
                        var sb = new StringBuilder();
                        sb.Append("{");
                        for (int n = 0; n < bof.Sets.Count; n++)
                        {
                            if (n < bof.Sets.Count - 1)
                            {
                                sb.Append($"{bof.Sets[n].ToDarl()}, ");
                            }
                            else
                            {
                                sb.Append(bof.Sets[n].ToDarl());
                            }
                        }
                        sb.Append("}");
                        darl = $"{darl} {sb.ToString()};";
                    }
                    else
                        darl = $"{darl};";
                    break;
                case OutputFormat.OutType.textual:
                    darl = darl + ";";
                    break;

            }
            return darl;
        }

        internal static BotOutputFormat ToBotFormat(IOSequenceDefinitionNode s)
        {
            BotOutputFormat o = null;
            if (s is OutputDefinitionNode)
            {
                var p = s as OutputDefinitionNode;
                o = new BotOutputFormat() { Name = p.name, OutputType = ConvertOutputType(p.iType), displayType = BotOutputFormat.DisplayType.Text, ValueFormat = string.Empty, Categories = p.categories, Sets = ConvertSets(p.sets) };
            }
            return o;
        }

        internal static OutputFormat.OutType ConvertOutputType(OutputDefinitionNode.OutputTypes oType)
        {
            switch (oType)
            {
                case OutputDefinitionNode.OutputTypes.categorical_output:
                    return OutputFormat.OutType.categorical;
                case OutputDefinitionNode.OutputTypes.numeric_output:
                    return OutputFormat.OutType.numeric;
                default:
                    return OutputFormat.OutType.textual;
            }
        }

        internal static List<SetDefinition> ConvertSets(Dictionary<string, DarlResult> sets)
        {
            var list = new List<SetDefinition>();
            foreach (var n in sets.Keys)
            {
                var vals = new List<double>();
                foreach (double v in sets[n].values)
                    vals.Add(v);
                list.Add(new SetDefinition { name = n, values = vals });
            }

            return list;
        }

        private static string WrapInvalidIdentifier(string s)
        {
            if (s.IndexOfAny(new char[] { ' ', '\t', '\n', '\r' }) > -1 || char.IsDigit(s[0]))
            {
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
