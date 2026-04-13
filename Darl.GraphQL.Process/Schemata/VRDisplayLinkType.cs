/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class VRDisplayLinkType : ObjectGraphType<VRDisplayLink>
    {
        public VRDisplayLinkType()
        {
            Name = "vrDisplayLink";
            Description = "A simplified version of a knowledge graph edge for VR display purposes";
            Field<StringGraphType>("id", resolve: c => c.Source.id);
            Field<StringGraphType>("label", resolve: c => c.Source.name);
            Field<StringGraphType>("source", resolve: c => c.Source.source);
            Field<StringGraphType>("target", resolve: c => c.Source.target);
        }
    }
}