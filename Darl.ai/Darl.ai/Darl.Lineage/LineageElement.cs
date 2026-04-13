/// </summary>

﻿using ProtoBuf;

namespace Darl.Lineage
{
    public enum LineageType { concept, reference, value, literal, Default, composite }
    [ProtoContract(AsReferenceDefault = true)]
    [ProtoInclude(10, typeof(LineageRecord))]
    public class LineageElement
    {
        public LineageElement()
        {

        }

        [ProtoMember(1)]
        public string lineage { get; set; }

        [ProtoMember(2)]
        public LineageType type { get; set; }

        [ProtoMember(3)]
        public string description { get; set; }
    }
}