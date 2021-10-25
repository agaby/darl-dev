using ProtoBuf;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageAssociation
    {
        [ProtoMember(1)]
        public LineageRecord start { get; set; }

        [ProtoMember(2)]
        public LineageRecord end { get; set; }

        [ProtoMember(3)]
        public double weight { get; set; }
    }
}
