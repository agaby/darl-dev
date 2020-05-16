using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphObjectUpdateType : InputObjectGraphType<GraphObjectUpdate>
    {
        public GraphObjectUpdateType()
        {
            Name = "graphObjectUpdate";
            Description = "updating an object representing a real world entity in the graph database";
            Field<ListGraphType<DateTimeGraphType>>("existence", "The period of existence of the object", resolve: c => c.Source.existence);
            Field(c => c.id).Description("The id of the object");
            Field(c => c.firstname, true).Description("The first name if human");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field(c => c.lineage,true).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name,true).Description("The name of this object");
            Field(c => c.secondname, true).Description("The second name if human");
            Field(c => c.externalId, true).Description("An external Id for this object");
            Field<ListGraphType<StringStringPairInputType>>("properties", "Other properties of this object", resolve: c => c.Source.properties);
            Field<BooleanGraphType>("virtual", "if true the object is a representative of a type, rather than a real world object", resolve: c => c.Source._virtual);
        }
    }
}
