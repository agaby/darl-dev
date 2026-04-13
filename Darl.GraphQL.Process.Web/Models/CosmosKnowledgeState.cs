/// </summary>

﻿using Darl.Thinkbase;
using MongoDB.Bson;

namespace Darl.GraphQL.Models.Models
{
    public class CosmosKnowledgeState : KnowledgeState
    {
        public ObjectId _id { get; set; }
    }
}
