using ProtoBuf;
using QuickGraph;

namespace Darl.GraphQL.Models.Models
{
    [ProtoContract]
    public class LineageEdge : IEdge<LineageNode>
    {
        [ProtoMember(1)]
        public LineageNode Source { get; set; }

        [ProtoMember(2)]
        public LineageNode Target { get; set; }

    }
}
