/// <summary>
/// </summary>

﻿using ProtoBuf;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageRecord : LineageElement
    {

        public LineageRecord() : base()
        {

        }

        [ProtoMember(1)]
        [JsonIgnore]
        public List<LineageAssociation> follows { get; set; }
        [ProtoMember(2)]
        [JsonIgnore]
        public List<LineageAssociation> precedes { get; set; }
        [ProtoMember(3)]
        public string typeWord { get; set; }
    }
}
