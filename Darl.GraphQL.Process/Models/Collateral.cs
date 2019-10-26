using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class Collateral
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string userId { get; set; }
        public byte[] content { get; set; }

    }
}
