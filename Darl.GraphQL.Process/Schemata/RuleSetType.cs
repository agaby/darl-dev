using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class RuleSetType : ObjectGraphType<RuleSet>
    {
        public RuleSetType()
        {
            Name = "Ruleset";
            Description = "Top level ruleset description";
            Field(C => C.Name);
            Field<RuleFormType>("ruleform", resolve: context => context.Source.Contents);
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity);
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory);
        }
    }
}

