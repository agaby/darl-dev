using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{ 

    public class BotDataStore : ILocalStore
    {
        public BotDataStore(IBotDataInterface store)
        {
            this.store = store;
        }

        public IBotDataInterface store;

        public Task<DarlResult> ReadAsync(List<string> address)
        {
            if (address.Any() && store != null && store.ContainsKey(address[0]))
            {
                if (store.TryGetValue(address[0], out double dval))
                {
                    return Task.FromResult<DarlResult>(new DarlResult(dval));
                }

                if (store.TryGetValue(address[0], out string text))
                    return Task.FromResult<DarlResult>(new DarlResult("", text, DarlResult.DataType.textual));
            }
            return Task.FromResult<DarlResult>(new DarlResult(0.0, true));
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            if (address.Any() && store != null)
            {
                switch (value.dataType)
                {
                    case DarlResult.DataType.categorical:
                    case DarlResult.DataType.textual:
                        store.SetValue<string>(address[0], value.Value as string);
                        break;
                    case DarlResult.DataType.numeric:
                        store.SetValue<double>(address[0], (double)value.Value);
                        break;
                }
            }
            return Task.CompletedTask;
        }
    }
}
