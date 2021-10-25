using DarlCommon;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    public class ValuesStore : ILocalStore

    {
        public List<DarlVar> values { get; set; }

        public ValuesStore(List<DarlVar> _values)
        {
            values = _values;
        }

        public Task<DarlResult> ReadAsync(List<string> address)
        {
            if (address.Any() && !string.IsNullOrEmpty(address[0]))
            {
                int index = 0;
                if (address.Count == 2 && !string.IsNullOrEmpty(address[1]))
                {//this has an index
                    int.TryParse(address[1], out index);
                }
                if (!LineageLibrary.lineages.ContainsKey(address[0].ToLower()))
                    throw new ArgumentOutOfRangeException(nameof(address), $"Address {address[0]} is not a valid lineage value.");
                var list = values.Where(a => a.name.StartsWith(address[0].ToLower())).ToList();
                if (list.Count > index)
                    return Task.FromResult<DarlResult>(DarlVarExtensions.Convert(list[index]));
            }
            return Task.FromResult<DarlResult>(new DarlResult(0.0, true));
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            return Task.CompletedTask;
        }

        public Dictionary<string, string> ConvertSettings()
        {
            var dict = new Dictionary<string, string>();
            foreach (var v in values)
            {
                if (!dict.ContainsKey(v.name))
                    dict.Add(v.name, v.Value);
                else
                    dict[v.name] = v.Value;
            }
            return dict;
        }
    }
}
