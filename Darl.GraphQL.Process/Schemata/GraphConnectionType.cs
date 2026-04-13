/// <summary>
/// GraphConnectionType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphConnectionType : ObjectGraphType<GraphConnection>
    {
        public GraphConnectionType()
        {
            Name = "graphConnection";
            Description = "A connection representing a real world relationship in the graph database";
            Field<ListGraphType<DarlTimeType>>("existence", "The period of existence of the connection", resolve: c => c.Source.existence);
            Field(c => c.id).Description("The unique id");
            Field(c => c.inferred, true).Description("If true, the existence of this connection is inferred from other sources");
            Field(c => c.lineage).Description("The type of this connection in the verb hypernymy hierarchy");
            Field(c => c.name).Description("The name of this connection");
            Field(c => c.startId).Description("The object at the start of this connection");
            Field(c => c.endId).Description("The object at the end of this connection");
            Field(c => c.weight).Description("The degree of plausibility of this connection").DefaultValue(1.0);
            Field<ListGraphType<GraphAttributeType>>("properties", "Other properties of this connection", resolve: c => c.Source.properties);
            Field<BooleanGraphType>("virtual", "if true the connection is a representative of a fundamental relationship, rather than a real world connection", resolve: c => c.Source._virtual);
        }
    }
}
