using Darl.Thinkbase;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class CosmosKnowledgeState : KnowledgeState
    {
        public ObjectId _id { get; set; }
    }
}
