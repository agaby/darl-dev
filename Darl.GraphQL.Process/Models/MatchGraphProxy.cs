using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// Used to represent the elements of a MatchGraph during serialization
    /// </summary>
    [ProtoContract]
    public class MatchGraphProxy
    {
        [ProtoMember(1)]
        public List<LineageNode> nodes { get; set; } = new List<LineageNode>();
        [ProtoMember(2)]
        public List<LineageEdge> edges { get; set; } = new List<LineageEdge>();
        [ProtoMember(3)]
        public Dictionary<string, List<string>> properNouns { get; set; } 
    }
}
