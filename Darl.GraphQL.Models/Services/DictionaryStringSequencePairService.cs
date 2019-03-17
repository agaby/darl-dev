using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    class DictionaryStringSequencePairService : IDictionaryStringSequencePairService
    {
        public async Task<List<StringSequencePair>> GetPairsFromDictionary(Dictionary<string, List<List<string>>> dict)
        {
            var list = new List<StringSequencePair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringSequencePair(k,dict[k]) );
            }
            return list;
        }
    }
}
