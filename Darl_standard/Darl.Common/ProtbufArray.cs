using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Common
{
    [ProtoContract]
    public class ProtobufArray
    {
        [ProtoMember(1)]
        public List<string> InnerArray;

        public ProtobufArray()
        { }
        public ProtobufArray(List<string> array)
        {
            InnerArray = array;
        }
    }
}
