/// <summary>
/// LiteKnowledgeState.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using LiteDB;

namespace Darl.GraphQL.Models.Models
{
    public class LiteKnowledgeState : KnowledgeState
    {
        public ObjectId _id { get; set; }

    }
}
