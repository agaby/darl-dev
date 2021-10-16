using Darl.Thinkbase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class LiteKnowledgeState : KnowledgeState
    {
        public ObjectId _id { get; set; }

    }
}
