using Darl.GraphQL.Models.Models;
using Darl.SoftMatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IConceptMapProcessing
    {
        Task<string> CreateConceptMatchTree(string userId, string treeName, List<StringStringPair> data, bool rebuild = false);
        Task<List<MatchResult>> InferFromConceptMatchTree(string userId, string treeName, List<string> texts);
    }
}
