/// <summary>
/// DarlMetaActiveNodeType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase.Meta;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMetaActiveNodeType : ObjectGraphType<DarlMetaActiveNode>
    {
        public DarlMetaActiveNodeType()
        {
            Name = "darlMetaActivityNode";
            Description = "The location and weight associated with a ruleset node";
            Field(c => c.weight);
            Field(c => c.name);
            Field<SourceSpanType>("span", resolve: c => c.Source.location);
        }
    }
}
