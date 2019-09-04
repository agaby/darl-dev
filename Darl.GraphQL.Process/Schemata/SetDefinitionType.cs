using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SetDefinitionType : ObjectGraphType<SetDefinition>
    {
        public SetDefinitionType()
        {
            Name = "SetDefinition";
            Description = "Definition of a DARL fuzzy set..";
            Field(c => c.name).Description("The name of the set.");
            Field < ListGraphType <FloatGraphType>>("values", resolve: context => context.Source.values);
        }
    }
}
