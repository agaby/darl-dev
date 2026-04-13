/// <summary>
/// NodaNodeIdType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    class NodaNodeIdType : ObjectGraphType<NodaNodeId>
    {
        public NodaNodeIdType()
        {
            Name = "NodaNodeIdType";
            Description = "Noda Node Id descriptor";
            Field(c => c.id);
            Field(c => c.Uuid);
        }
    }
}
