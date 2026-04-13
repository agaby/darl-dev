/// <summary>
/// DarlMetaActivityType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase.Meta;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMetaActivityType : ObjectGraphType<DarlMetaActivity>
    {
        public DarlMetaActivityType()
        {
            Name = "darlMetaActivity";
            Description = "Holds the active ruleset information for the last evaluation.";
            Field<ListGraphType<DarlMetaActiveNodeType>>("activeNodes", resolve: c => c.Source.activeNodes);
        }
    }
}
