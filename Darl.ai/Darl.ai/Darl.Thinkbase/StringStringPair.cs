/// <summary>
/// StringStringPair.cs - Core module for the Darl.dev project.
/// </summary>

﻿using ProtoBuf;

namespace Darl.Thinkbase
{
    [ProtoContract]
    public class StringStringPair
    {
        public StringStringPair()
        {

        }

        public StringStringPair(string name, string value)
        {
            Name = name;
            Value = value;
        }
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Value { get; set; }
    }
}
