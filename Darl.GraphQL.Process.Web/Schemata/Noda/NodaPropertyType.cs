/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaPropertyType : ObjectGraphType<NodaProperty>
    {
        public NodaPropertyType()
        {
            Name = "NodaPropertyType";
            Description = "Noda property descriptor";
            Field(c => c.image);
            Field(c => c.name);
            Field(c => c.notes);
            Field(c => c.page);
            Field(c => c.size);
            Field(c => c.text);
            Field(c => c.uuid);
            Field(c => c.video);
            Field<ListGraphType<NodaToneType>>("tone", resolve: context => context.Source.tone);
        }
    }
}
