/// </summary>

﻿using Darl.Lineage.Bot.Stores;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Darl.Lineage.Bot
{
    [ProtoContract]
    public class StoredBotData : IBotDataInterface
    {
        [ProtoMember(1)]
        public Dictionary<string, string> data { get; set; } = new Dictionary<string, string>();

        public bool ContainsKey(string v)
        {
            return data.ContainsKey(v);
        }

        public Dictionary<string, string> ConvertStore()
        {
            return data;
        }

        public void SetValue<T>(string v1, T v2)
        {
            if (ContainsKey(v1))
                data[v1] = v2.ToString();
            else
                data.Add(v1, v2.ToString());
        }

        public bool TryGetValue<T>(string v, out T dval)
        {
            if (ContainsKey(v))
            {
                if (typeof(T) == typeof(double))
                {
                    if (double.TryParse(data[v], out double dvalo))
                    {
                        dval = (T)Convert.ChangeType(data[v], typeof(T));
                        return false;
                    }
                    else
                    {
                        dval = (T)Convert.ChangeType(0, typeof(T));
                        return false;
                    }
                }
                dval = (T)Convert.ChangeType(data[v], typeof(T));
                return true;
            }
            dval = (T)Convert.ChangeType(0, typeof(T));
            return false;
        }
    }
}
