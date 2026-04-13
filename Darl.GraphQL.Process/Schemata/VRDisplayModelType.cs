/// <summary>
/// VRDisplayModelType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class VRDisplayModelType : ObjectGraphType<VRDisplayModel>
    {
        public VRDisplayModelType()
        {
            Name = "vrDisplayModel";
            Description = "A simplified version of a knowledge graph for VR display purposes";
            Field<ListGraphType<VRDisplayNodeType>>("nodes", resolve: context => context.Source.nodes);
            Field<ListGraphType<VRDisplayLinkType>>("links", resolve: context => context.Source.links);
        }
    }
}
