using Darl.Common;
using Darl.GraphQL.Process.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    [ProtoContract]
    public class BotState
    {
        [ProtoMember(1)] 
        public string userId { get; set; }

        [ProtoMember(2)]
        public string conversationId { get; set; }

        [ProtoMember(3)]
        public List<DarlVar> values { get; set; } = new List<DarlVar>();

        [ProtoMember(4)]
        public StoredBotData userData { get; set; }
        [ProtoMember(5)]
        public StoredBotData conversationData { get; set; }
        [ProtoMember(6)]
        public StoredBotData privateConversationData { get; set; }
        [ProtoMember(7)]
        public DateTime? updated { get; set; }

        public List<List<string>> kGraphData { get; set; }
        [ProtoMember(9)]
        public DarlVar? pending { get; set; }

        [ProtoMember(8)] //only here because protobuf can't handle nested arrays
        private List<ProtobufArray> _nestedArrayForProtoBuf // Never used elsewhere
        {
            get
            {
                if (kGraphData == null)  
                    return null;
                return kGraphData.Select(p => new ProtobufArray(p)).ToList();
            }
            set
            {
                kGraphData = value.Select(p => p.InnerArray).ToList();
            }
        }

    }
}
