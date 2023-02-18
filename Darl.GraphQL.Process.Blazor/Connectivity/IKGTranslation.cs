using Darl.GraphQL.Process.Blazor.Models;
using Darl.Lineage;
using Darl.Thinkbase;
using DarlCommon;
using Microsoft.AspNetCore.Http;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Process.Blazor.Connectivity
{
    public interface IKGTranslation
    {
        string GetCurrentUserId(GraphQLUserContext? userContext);
        Task<bool> CreateNewGraph(string userId, string modelName);
        Task<string> GetSuggestedRuleSet(string userId, string modelName, string objectId, string lineage);
        Task<string> GetTypeWordForLineage(string lineage, string isoLanguage = "en");
        Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en");
        Task<List<DarlLintView>> LintDarlMeta(string darl);
        Task<List<GraphAttribute>> GetConceptCloudData(string userId, string graphName, string address);
        Task<string> CreateTempKG(string userId, string graphName, IFormFile file);
        Task<bool> TempKGExists(string userId, string graphName);
        Task<byte[]> KGContents(string userId, string graphName);
        Task<List<KGraphListElement>>GetKGraphs(string userId, string tenantId);
        string GetCurrentTenantId(GraphQLUserContext? userContext);
        Task Promote(string userId, string tenantId, string name);
    }
}
