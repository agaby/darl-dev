using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.Lineage.Bot.Stores
{
    /// <summary>
    /// Implements a local ephemeral store of botdata for testing
    /// </summary>
    public class LocalBotData : IBotDataInterface
    {
        Dictionary<string, string> store;

        public LocalBotData(Dictionary<string,string> store)
        {
            this.store = store;
        }

        public bool ContainsKey(string v)
        {
            return store.ContainsKey(v);
        }

        public Dictionary<string, string> ConvertStore()
        {
            return store;
        }

        public void SetValue<T>(string v1, T v2)
        {
            if (ContainsKey(v1))
                store[v1] = v2.ToString();
            else
                store.Add(v1, v2.ToString());
        }

        public bool TryGetValue<T>(string v, out T dval)
        {
            if (ContainsKey(v))
            {
                dval = (T)Convert.ChangeType(store[v],typeof(T));
                return true;
            }
            dval = (T)Convert.ChangeType(0, typeof(T));
            return false;
        }

    }
}
