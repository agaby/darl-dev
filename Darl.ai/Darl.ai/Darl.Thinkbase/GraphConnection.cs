using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Darl.Thinkbase
{
    [ProtoContract(AsReferenceDefault = true)]
    public class GraphConnection : GraphElement
    {
        [ProtoMember(1)]
        public double weight { get; set; } = 1.0;
        [ProtoMember(2)]
        public string startId { get; set; }
        [ProtoMember(3)]
        public string endId { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Name = {name},");
            sb.AppendLine($"inferred = {inferred},");
            sb.AppendLine($"lineage = {lineage},");
            sb.AppendLine($"startId = {startId},");
            sb.AppendLine($"endId = {endId},");
            sb.AppendLine($"weight = {weight}");
            return sb.ToString();
        }
    }
}
