using ProtoBuf;
using System.Collections.Generic;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageRecord : LineageElement
    {

        public LineageRecord() : base()
        {

        }

        [ProtoMember(1)]
        public List<LineageAssociation> follows { get; set; }
        [ProtoMember(2)]
        public List<LineageAssociation> precedes { get; set; }
        [ProtoMember(3)]
        public string typeWord { get; set; }
    }
}
