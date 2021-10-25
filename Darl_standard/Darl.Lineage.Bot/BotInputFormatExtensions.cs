using DarlCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarlLanguage.Processing;

namespace Darl.Lineage.Bot
{
    public static class BotInputFormatExtensions
    {
        public static string ToDarl(this BotInputFormat bif)
        {
            var darl = $"\tinput {bif.InType.ToString()} {bif.Name}";
            switch(bif.InType)
            {
                case InputFormat.InputType.categorical:
                    if(bif.Categories != null && bif.Categories.Any())
                    {
                        var sb = new StringBuilder();
                        sb.Append("{");
                        for(int n = 0; n < bif.Categories.Count; n++)
                        {
                            if(n < bif.Categories.Count -1)
                            {
                                sb.Append($"{WrapInvalidIdentifier(bif.Categories[n])}, ");
                            }
                            else
                            {
                                sb.Append(WrapInvalidIdentifier(bif.Categories[n]));
                            }
                        }
                        sb.Append("}");
                        darl = $"{darl} {sb.ToString()};";
                    }
                    else
                        darl = $"{darl};";
                    break;
                case InputFormat.InputType.numeric:
                    if (bif.Sets != null && bif.Sets.Any())
                    {
                        var sb = new StringBuilder();
                        sb.Append("{");
                        for (int n = 0; n < bif.Sets.Count; n++)
                        {
                            if (n < bif.Sets.Count - 1)
                            {
                                sb.Append($"{bif.Sets[n].ToDarl()}, ");
                            }
                            else
                            {
                                sb.Append(bif.Sets[n].ToDarl());
                            }
                        }
                        sb.Append("}");
                        darl = $"{darl} {sb.ToString()};";
                    }
                    else
                        darl = $"{darl};";
                    break;
                case InputFormat.InputType.textual:
                    darl = darl + ";";
                    break;

            }
            return darl;
        }

        internal static BotInputFormat ToBotFormat(InputDefinitionNode s)
        {
            return new BotInputFormat() { Name = s.name, Categories = s.categories, Sets = BotOutputFormatExtensions.ConvertSets(s.sets), InType = ConvertInputType(s.iType), EnforceCrisp = false, ShowSets = false };
        }

        private static InputFormat.InputType ConvertInputType(InputDefinitionNode.InputTypes iType)
        {
            switch(iType)
            {
                case InputDefinitionNode.InputTypes.numeric_input:
                    return InputFormat.InputType.numeric;
                case InputDefinitionNode.InputTypes.categorical_input:
                    return InputFormat.InputType.categorical;
                case InputDefinitionNode.InputTypes.textual_input:
                    return InputFormat.InputType.textual;
                default:
                    return InputFormat.InputType.numeric;
            }
        }

        private static string WrapInvalidIdentifier(string s)
        {
            if(s.IndexOfAny(new char[] { ' ','\t','\n','\r'}) > -1 || char.IsDigit(s[0]))
            {
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
