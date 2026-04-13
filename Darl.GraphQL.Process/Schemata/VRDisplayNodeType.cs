/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class VRDisplayNodeType : ObjectGraphType<VRDisplayNode>
    {
        public VRDisplayNodeType()
        {
            Name = "vrDisplayNode";
            Description = "A simplified version of a knowledge graph node for VR display purposes";
            Field<StringGraphType>("id", resolve: c => c.Source.id);
            Field<StringGraphType>("label", resolve: c => c.Source.name);
            Field<StringGraphType>("lineage", resolve: c => c.Source.lineage);
            Field<StringGraphType>("sublineage", resolve: c => c.Source.subLineage);
            Field<StringGraphType>("externalId", resolve: c => c.Source.externalId);
            Field<StringGraphType>("parent", resolve: c => c.Source.parent);
            Field<StringGraphType>("compositelineage", resolve: c => c.Source.compositeLineage);
            Field<ListGraphType<VRDisplayAttType>>("attributes", resolve: c => c.Source.attributes);
        }
    }
}