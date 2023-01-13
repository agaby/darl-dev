using Darl.Thinkbase;
using MongoDB.Bson;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class CosmosKnowledgeState : KnowledgeState
    {
        public ObjectId _id { get; set; }
    }
}
