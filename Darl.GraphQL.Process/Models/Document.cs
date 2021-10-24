using MongoDB.Bson;

namespace Darl.GraphQL.Models.Models
{
    public class Document
    {
        public ObjectId id { get; set; }

        public string name { get; set; }

        public string userId { get; set; }

        public byte[] content { get; set; }
    }
}
