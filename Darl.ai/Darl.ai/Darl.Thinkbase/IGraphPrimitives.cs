/// <summary>
/// </summary>

﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{

    /// <summary>
    /// Back end processes that communicate with a graph database or in memory representation
    /// </summary>
    public interface IGraphPrimitives
    {
        Task<bool> IsDemo(string compositeName);

        Task<bool> Exists(string compositeName);
        Task<bool> CreateModel(string compositeName);
        Task<bool> DeleteModel(string compositeName);
        Task<List<string>> ListModels(string userId);
        Task<IGraphModel?> Load(string compositeName);
        Task Store(string compositeName);
        Task<bool> SaveKSChanges(string userId, string subjectId, KnowledgeState ks);
        Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId, string graphName, bool external);
        Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, string graphName, bool externalIds);
        Task<string> CopyRenameKG(string userId, string name, string newName);
        Task<int> GetKGraphCountAsync(string userId);
        Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName);
        Task<KnowledgeState> CreateKnowledgeState(KnowledgeState state);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<KnowledgeState> DeleteKnowledgeState(string userId, string subjectId, string graphName);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage);
        Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden);
        Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName);
        string CreateTimedAccessUrl(string userId, string name);
        Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName);
        Task<KnowledgeState> ConvertKSIDs(KnowledgeState ks);
        Task<bool> ExistsInCache(string userId, string graphName);
        Task<byte[]> KGContents(string userId, string graphName);
        Task<string> CreateTempKG(string userId, string graphName, byte[] bytes);
        Task Store(string blobName, IGraphModel model);
        string CreateCompositeName(string userId, string name);
    }
}
