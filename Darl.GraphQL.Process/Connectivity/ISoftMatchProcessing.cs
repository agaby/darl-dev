using Darl.GraphQL.Models.Models;
using Darl.SoftMatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface ISoftMatchProcessing
    {
        Task<string> CreateSoftMatchModel(string userId, string treeName, List<StringStringPair> data, bool rebuild = false);
        Task<List<MatchResult>> InferFromSoftMatchModel(string userId, string treeName, List<string> texts);
        Task<List<string>> ListSoftMatchModels(string userId);
        Task<string> DeleteSoftMatchModel(string userId, string name);
    }
}
