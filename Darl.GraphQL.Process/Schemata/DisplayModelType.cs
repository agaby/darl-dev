/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayModelType : ObjectGraphType<DisplayModel>
    {
        public DisplayModelType()
        {
            Name = "displayModel";
            Description = "A simplified version of a knowledge graph for display purposes";
            Field<ListGraphType<DisplayObjectOuterType>>("nodes", resolve: context => context.Source.nodes);
            Field<ListGraphType<DisplayConnectionOuterType>>("edges", resolve: context => context.Source.edges);
        }
    }
}
