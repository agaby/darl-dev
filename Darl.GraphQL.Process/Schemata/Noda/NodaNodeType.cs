using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaNodeType : ObjectGraphType<NodaNode>
    {
        public NodaNodeType()
        {
            Name = "NodaNodeType";
            Description = "Noda Node descriptor";
            Field(c => c.collapsed);
            Field<NodaFacingType>("facing", resolve: context => context.Source.facing);
            Field(c => c.folded);
            Field(c => c.kind);
            Field(c => c.title);
            Field<NodaPositionType>("position", resolve: context => context.Source.position);
            Field<ListGraphType<NodaPropertyType>>("properties", resolve: context => context.Source.properties);
            Field<NodaNodeShapeEnum>("shape", resolve: context => context.Source.shape);
            Field(c => c.shape);
            Field(c => c.size);
            Field<NodaToneType>("tone", resolve: context => context.Source.tone);
            Field(c => c.uuid);
        }
    }
}
