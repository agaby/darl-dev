using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class LiteKGraph : KGraph
    {
        public ObjectId Id { get; set; }
    }
}
