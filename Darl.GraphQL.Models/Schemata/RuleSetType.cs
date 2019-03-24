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
            Field(C => C.Name);
            Field(c => c.LastModified);
            Field(c => c.Size);
            Field<RuleFormType>("ruleform", resolve: context => ruleforms.GetRuleFormAsync(context.Source.Name));
        }
    }
}

