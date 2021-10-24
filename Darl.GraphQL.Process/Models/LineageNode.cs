using ProtoBuf;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class LineageNode
    {
        /// <summary>
        /// temp nodes are created during evaluation. No need to serialize
        /// </summary>
        public bool temp { get; set; } = false;
        [ProtoMember(1)]
        public string lineageElement { get; set; }
        [ProtoMember(2)]
        public List<string> indexes { get; set; }
        public Dictionary<string, LineageEdge> edges { get; set; } = new Dictionary<string, LineageEdge>();
    }
}
