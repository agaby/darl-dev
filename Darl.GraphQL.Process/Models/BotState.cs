using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    [BsonIgnoreExtraElements]
    public class BotState
    {
//        public ObjectId id { get; set; }

        public string userId { get; set; }

        public string conversationId { get; set; }

        public Stack<RuleSetHandler> ruleProcessing { get; set; } = new Stack<RuleSetHandler>();

        public List<DarlVar> values { get; set; } = new List<DarlVar>();

        public LocalBotData userData { get; set; }

        public LocalBotData conversationData { get; set; }

        public LocalBotData privateConversationData { get; set; }
    }
}
