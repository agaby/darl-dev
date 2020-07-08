using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{ 
    public class GraphObjectType : ObjectGraphType<GraphObject>
    {
        public GraphObjectType()
        {
            Name = "graphObject";
            Description = "an object representing a real world entity in the graph database";
            Field<ListGraphType<DateTimeGraphType>>("existence", "The period of existence of the object", resolve: c => c.Source.existence);
            Field(c => c.id).Description("The unique id");
            Field(c => c.inferred, true).Description("If true, the existence of this object is inferred from other sources");
            Field(c => c.lineage).Description("The type of this object in the noun hypernymy hierarchy");
            Field(c => c.name).Description("The name of this object");
            Field<ListGraphType<StringStringPairType>>("properties", "Other properties of this object", resolve: c => c.Source.properties);
            Field<BooleanGraphType>("virtual", "if true the object is a representative of a type, rather than a real world object", resolve: c => c.Source._virtual);
        }
    }
}
