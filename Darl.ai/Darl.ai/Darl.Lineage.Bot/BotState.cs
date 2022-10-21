using Darl.Common;
using Darl.Thinkbase;
using DarlCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Lineage.Bot
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

        [ProtoMember(7)]
        public DateTime? updated { get; set; }

        public List<List<string>>? kGraphData { get; set; }
        [ProtoMember(9)]
        public DarlVar? pending { get; set; }

        [ProtoMember(8)] //only here because protobuf can't handle nested arrays
        private List<ProtobufArray>? _nestedArrayForProtoBuf // Never used elsewhere
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

        [ProtoMember(10)]
        public Dictionary<string, KnowledgeState> states { get; set; } = new Dictionary<string, KnowledgeState>();

        public void ClearBotState(string graphname)
        {
            values.Clear();
            if(kGraphData != null)
                kGraphData.Clear();
            pending = null;
            if(states.ContainsKey(graphname))
                states.Remove(graphname);
        }

    }
}
