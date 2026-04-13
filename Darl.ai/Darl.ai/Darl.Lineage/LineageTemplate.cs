/// <summary>
/// LineageTemplate.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageTemplate
    {
        public LineageTemplate()
        {
        }

        public List<List<LineageElement>> sequence { get; set; }

        [ProtoMember(1)]
        public string text { get; set; }

        [ProtoMember(2)]
        public string encSequence
        {
            get
            {
                return JsonConvert.SerializeObject(sequence);
            }
            set
            {
                sequence = JsonConvert.DeserializeObject<List<List<LineageElement>>>(value);
            }
        }
    }
}