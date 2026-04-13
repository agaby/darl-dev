/// </summary>

﻿using DarlLanguage.Processing;
using System.Linq;

namespace Darl.Language
{
    public static class IOExtensions
    {
        public static bool IsNumeric(this InputDefinitionNode node)
        {
            return node.iType == InputDefinitionNode.InputTypes.numeric_input;
        }

        public static bool IsTextual(this InputDefinitionNode node)
        {
            return node.iType == InputDefinitionNode.InputTypes.textual_input;
        }

        public static bool IsCategorical(this InputDefinitionNode node)
        {
            return node.iType == InputDefinitionNode.InputTypes.categorical_input;
        }

        public static int GetPartitions(this InputDefinitionNode node)
        {
            switch (node.iType)
            {
                case InputDefinitionNode.InputTypes.categorical_input:
                    return node.categories.Count;
                case InputDefinitionNode.InputTypes.numeric_input:
                    return node.sets.Count;
                case InputDefinitionNode.InputTypes.presence_input:
                    return 2;
                default:
                    return 1;
            }
        }

        public static string GetNameFromPartitionIndex(this InputDefinitionNode node, int partition)
        {
            switch (node.iType)
            {
                case InputDefinitionNode.InputTypes.categorical_input:
                    return node.categories[partition];
                case InputDefinitionNode.InputTypes.numeric_input:
                    return node.sets[node.sets.Keys.ToList()[partition]].name;
                case InputDefinitionNode.InputTypes.presence_input:
                    return partition == 0 ? "absent" : "present";
                default:
                    return string.Empty;
            }
        }
    }
}
