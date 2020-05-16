using Darl.GraphQL.Models.Models;
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
            Field(c => c.firstname, true).Description("The first name if human");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name).Description("The name of this object");
            Field(c => c.secondname, true).Description("The second name if human");
            Field(c => c.externalId, true).Description("An external Id for this object");
            Field<ListGraphType<StringStringPairInputType>>("properties", "Other properties of this object", resolve: c => c.Source.properties);
            Field<BooleanGraphType>("virtual", "if true the object is a representative of a type, rather than a real world object", resolve: c => c.Source._virtual);
        }
    }
}
