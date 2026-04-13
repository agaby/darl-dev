/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageNodeDefinitionType : ObjectGraphType<LineageNodeDefinition>
    {
        public LineageNodeDefinitionType()
        {
            Name = "lineageNodeDefinition";
            Description = "data required to build an editable tree of the text engine contents";
            Field(c => c.children);
            Field(c => c.id, true);
            Field(c => c.text, true);
            Field(c => c.icon, true);
            Field(c => c.type, true);
            Field<LineageNodeAttributeType>("attributes", resolve: c => c.Source.attributes);
        }
    }
}
