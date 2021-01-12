using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphObjectInputType : InputObjectGraphType<GraphObjectInput>
    {
        public GraphObjectInputType()
        {
            Name = "graphObjectInput";
            Description = "an object representing a real world entity in the graph database";
            Field<ListGraphType<DateTimeGraphType>>("existence", "The period of existence of the object", resolve: c => c.Source.existence);
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.subLineage).Description("The specialization of the lineage");
            Field(c => c.name).Description("The name of this object");
            Field(c => c.externalId, true).Description("An external Id for this object");
            Field<ListGraphType<GraphAttributeInputType>>("properties", "Other properties of this object", resolve: c => c.Source.properties);
        }
    }
}
