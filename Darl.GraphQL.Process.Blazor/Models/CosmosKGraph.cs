using MongoDB.Bson;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public class CosmosKGraph : KGraph
    {
        public ObjectId Id { get; set; }
    }
}
