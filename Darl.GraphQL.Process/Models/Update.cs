using MongoDB.Bson;
using System;

namespace Darl.GraphQL.Models.Models
{
    public class Update
    {
        public ObjectId Id { get; set; }
        public string from { get; set; }

        public string to { get; set; }

        public DateTime updated { get; set; }
    }
}
