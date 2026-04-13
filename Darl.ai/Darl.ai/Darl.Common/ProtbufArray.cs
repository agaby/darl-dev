/// <summary>
/// ProtbufArray.cs - Core module for the Darl.dev project.
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
