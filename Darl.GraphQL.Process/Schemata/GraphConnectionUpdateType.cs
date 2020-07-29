using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphConnectionUpdateType : InputObjectGraphType<GraphConnectionUpdate>
    {
        public GraphConnectionUpdateType()
        {
            Name = "graphConnectionUpdate";
            Description = "Updating a connection representing a real world relationship in the graph database";
            Field(c => c.id).Description("The id of the connection to update");
            Field<ListGraphType<DateTimeGraphType>>("existence", "The period of existence of the connection", resolve: c => c.Source.existence);
            Field(c => c.lineage,true).Description("The type of this connection in the verb hypernymy hierarchy");
            Field(c => c.name,true).Description("The name of this connection");
            Field(c => c.weight,true).Description("The degree of plausibility of this connection");
            Field<ListGraphType<GraphAttributeInputType>>("properties", "Other properties of this connection", resolve: c => c.Source.properties);
        }
    }
}
