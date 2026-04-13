/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DisplayObjectInnerType : ObjectGraphType<DisplayObject>
    {
        public DisplayObjectInnerType()
        {
            Field<StringGraphType>("id", resolve: c => c.Source.id);
            Field<StringGraphType>("label", resolve: c => c.Source.name);
            Field<StringGraphType>("lineage", resolve: c => c.Source.lineage);
            Field<StringGraphType>("sublineage", resolve: c => c.Source.subLineage);
            Field<StringGraphType>("externalId", resolve: c => c.Source.externalId);
            Field<StringGraphType>("parent", resolve: c => c.Source.parent);
        }
    }
}