using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public enum DQType { rule_edit, bot_edit };
    public class DQTypeEnum : EnumerationGraphType<DQType>
    {
        public DQTypeEnum()
        {
            Name = "DynamicQType";
            Description = "The type of the dynamic questionnaire to run";
            AddValue("rule_edit", "edit the supporting values of a rule set", 0);
            AddValue("bot_edit", "edit the contents of a bot model", 1);
        }
    }
}
