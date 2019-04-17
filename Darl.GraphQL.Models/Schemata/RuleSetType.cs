using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class RuleSetType : ObjectGraphType<RuleSet>
    {
        public RuleSetType(IRuleFormService ruleforms)
        {
            Name = "Ruleset";
            Description = "Top level ruleset description";
            Field(C => C.Name);
            Field<RuleFormType>("ruleform", resolve: context => ruleforms.GetRuleFormAsync(context.Source.Name));
        }
    }
}

