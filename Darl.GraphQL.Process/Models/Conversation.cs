using MongoDB.Bson;
using System;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// General details about a conversation
    /// </summary>
    public class Conversation
    {
        public ObjectId Id { get; set; }
        public string appId { get; set; }
        public string conversationId { get; set; }
        public string city { get; set; }
        public string countryOrRegion { get; set; }
        public string stateOrProvince { get; set; }
        public DateTime timestamp { get; set; }
        public int count { get; set; } = 1;
    }
}
