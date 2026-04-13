/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayObjectOuterType : ObjectGraphType<DisplayObject>
    {
        public DisplayObjectOuterType()
        {
            Name = "displayObject";
            Description = "A display representation of a knowledge graph object";
            Field<DisplayObjectInnerType>("data", resolve: context => context.Source);
            Field<BooleanGraphType>("selectable", resolve: context => true);
            Field<BooleanGraphType>("grabbable", resolve: context => true);
        }
    }
}