using Darl.SoftMatch;
using Darl.Thinkbase;

namespace Darl.GraphQL.Process.Blazor.Connectivity
{
    public interface ISoftMatchProcessing
    {
        Task<string> CreateSoftMatchModel(string userId, string treeName, List<StringStringPair> data, bool rebuild = false);
        Task<List<MatchResult>> InferFromSoftMatchModel(string userId, string treeName, List<string> texts);
        Task<List<string>> ListSoftMatchModels(string userId);
        Task<string> DeleteSoftMatchModel(string userId, string name);
    }
}
