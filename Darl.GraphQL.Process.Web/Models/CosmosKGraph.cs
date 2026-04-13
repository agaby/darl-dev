/// <summary>
/// CosmosKGraph.cs - Core module for the Darl.dev project.
/// </summary>

﻿using MongoDB.Bson;

namespace Darl.GraphQL.Models.Models
{
    public class CosmosKGraph : KGraph
    {
        public ObjectId Id { get; set; }
    }
}
