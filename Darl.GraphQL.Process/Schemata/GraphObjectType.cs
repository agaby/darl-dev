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
            Field<StringGraphType>("lineage", "The type of this object in the noun hypernymy hierarchy", resolve: c => ExtractLineage(c.Source.lineage));
            Field<StringGraphType>("subLineage", "The sub-type of this object.", resolve: c => ExtractSubLineage(c.Source.lineage));
            Field(c => c.name, true).Description("The name of this object");
            Field(c => c.externalId, true).Description("A reference to this object in an external system");
            Field<ListGraphType<GraphAttributeType>>("properties", "Other properties of this object", resolve: c => c.Source.properties);
            Field<BooleanGraphType>("virtual", "if true the object is a representative of a type, rather than a real world object", resolve: c => c.Source._virtual);
        }

        private string ExtractLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if(pos == -1)
            {
                return lineage;
            }
            return lineage.Substring(0, pos);
        }
        private string ExtractSubLineage(string lineage)
        {
            var pos = lineage.IndexOf('+');
            if (pos == -1)
            {
                return null;
            }
            return lineage.Substring(pos + 1);
        }
    }
}
