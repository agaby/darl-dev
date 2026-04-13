/// <summary>
/// DQTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public enum DQType { rule_edit, bot_edit };
    public class DQTypeEnum : EnumerationGraphType
    {
        public DQTypeEnum()
        {
            Name = "DynamicQType";
            Description = "The type of the dynamic questionnaire to run";
            Add("rule_edit", 0, "edit the supporting values of a rule set");
            Add("bot_edit", 1, "edit the contents of a bot model");
        }
    }
}
