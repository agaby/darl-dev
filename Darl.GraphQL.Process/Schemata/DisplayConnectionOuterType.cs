/// <summary>
/// DisplayConnectionOuterType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayConnectionOuterType : ObjectGraphType<DisplayConnection>
    {
        public DisplayConnectionOuterType()
        {
            Name = "displayConnection";
            Description = "A display representation of a knowledge graph connection";
            Field<DisplayConnectionInnerType>("data", resolve: context => context.Source);
            Field<BooleanGraphType>("selectable", resolve: context => true);
            Field<BooleanGraphType>("grabbable", resolve: context => true);
        }
    }
}