using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    public class DictionaryStringStringPairService : IDictionaryStringStringPairService
    {
        public async Task<List<StringStringPair>> GetPairsFromDictionary(Dictionary<string, string> dict)
        {
            var list = new List<StringStringPair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringStringPair(k, dict[k]));
            }
            return list;
        }
    }
}
