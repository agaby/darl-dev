using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class Default
    {
        public ObjectId id { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        
    }
}
