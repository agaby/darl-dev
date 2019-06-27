using Darl.Lineage.Bot.Stores;
using DarlCommon;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotState
    {
        public ObjectId id { get; set; }

        public string userId { get; set; }

        public string conversationId { get; set; }

        public Stack<QuestionSetProxy> ruleProcessing { get; set; } = new Stack<QuestionSetProxy>();

        public List<DarlVar> values { get; set; } = new List<DarlVar>();

        public LocalBotData userData { get; set; }

        public LocalBotData conversationData { get; set; }

        public LocalBotData privateConversationData { get; set; }
    }
}
