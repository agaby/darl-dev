/// </summary>

﻿using Darl.Common;
using ProtoBuf;
using System.Collections.Generic;


namespace Darl.Thinkbase
{
    [ProtoContract(AsReferenceDefault = true)]
    [ProtoInclude(10, typeof(GraphObject))]
    [ProtoInclude(20, typeof(GraphConnection))]
    [ProtoInclude(30, typeof(GraphAttribute))]
    public abstract class GraphElement : GraphAbstraction
    {

        public GraphElement()
        {

        }

        [ProtoMember(1)]
        public string? id { get; set; }
        [ProtoMember(2)]
        public string? name { get; set; }
        [ProtoMember(3)]
        public string? lineage { get; set; }
        [ProtoMember(4)]
        public List<DarlTime>? existence { get; set; }
        [ProtoMember(5)]
        public bool inferred { get; set; } = false;
        [ProtoMember(6)]
        public bool? _virtual { get; set; }
        [ProtoMember(7)]
        public List<GraphAttribute>? properties { get; set; }
        [ProtoMember(8)]
        public string? dynamicSource { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
