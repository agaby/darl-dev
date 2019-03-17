using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    class DictionaryStringDoublePairService : IDictionaryStringDoublePairService
    {
        public async Task<List<StringDoublePair>> GetPairsFromDictionary(Dictionary<string, double> dict)
        {
            var list = new List<StringDoublePair>();
            foreach(var k in dict.Keys)
            {
                list.Add(new StringDoublePair { name = k, value = dict[k] });
            }
            return list;
        }
    }
}
