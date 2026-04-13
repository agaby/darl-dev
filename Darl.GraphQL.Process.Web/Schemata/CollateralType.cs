/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class CollateralType : ObjectGraphType<Collateral>
    {
        public CollateralType()
        {
            Name = "Collateral";
            Description = "Formatted text that can be used in bot responses";
            Field(c => c.Name);
            Field(c => c.Value);
        }
    }
}
