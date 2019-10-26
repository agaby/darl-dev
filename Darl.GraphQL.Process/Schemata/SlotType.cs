using Darl.GraphQL.Process.Models.Alexa;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SlotType : ObjectGraphType<Slot>
    {
        public SlotType()
        {
            Name = "slot";
            Description = "An Alexa Slot used in Skill definitions";
            Field(c => c.name);
            Field<ListGraphType<StringGraphType>>("samples", resolve: c => c.Source.samples);
            Field(c => c.type);
        }
    }
}
