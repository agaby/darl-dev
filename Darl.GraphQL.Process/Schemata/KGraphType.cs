/// <summary>
/// KGraphType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class KGraphType : ObjectGraphType<KGraph>
    {
        public KGraphType(IGraphProcessing graph)
        {
            Name = "kGraph";
            Description = "A Knowledge Graph and its status.";
            Field(c => c.Name).Description("The unique name of the knowledge graph");
            FieldAsync<GraphModelType>("model", resolve: async context => await graph.GetModel(context.Source.userId, context.Source.Name));
        }
    }
}
