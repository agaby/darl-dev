using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IDictionaryStringDoublePairService
    {
        Task<List<StringDoublePair>> GetPairsFromDictionary(Dictionary<string, double> pair);
    }
}
