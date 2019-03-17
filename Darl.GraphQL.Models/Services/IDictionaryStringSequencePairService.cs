using Darl.GraphQL.Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IDictionaryStringSequencePairService
    {
        Task<List<StringSequencePair>> GetPairsFromDictionary(Dictionary<string, List<List<string>>> dict);
    }
}