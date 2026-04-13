/// <summary>
/// </summary>

﻿using ProtoBuf;
using System.Collections.Generic;

namespace Darl.Common
{
    [ProtoContract]
    public class ProtobufArray
    {
        [ProtoMember(1)]
        public List<string>? InnerArray;

        public ProtobufArray()
        { }
        public ProtobufArray(List<string> array)
        {
            InnerArray = array;
        }
    }
}
