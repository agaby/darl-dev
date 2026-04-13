/// <summary>
/// </summary>

﻿//using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IConnectivity
    {

        Task<List<KGraph>> GetKGraphsAsync(string userId);
        Task<KnowledgeState> GetKnowledgeState(string userId, string ksId, string graphName);
        Task<List<KnowledgeState>> GetKnowledgeStates(string userId, string graphName);
        Task<KnowledgeState?> DeleteKnowledgeState(string userId, string ksId, string graphName);
        Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state);
        Task<KnowledgeState> CreateKnowledgeState(KnowledgeState state);
        Task<long> DeleteAllKnowledgeStates(string userId, string graphName);
        Task<KGraph> GetKGModel(string userId, string model);
        Task<KGraph> CreateKGraph(string userId, string name);
        Task<KGraph> DeleteKGraph(string userId, string name);
        Task<KGraph> UpdateKGraph(string userId, string name, KGraphUpdate kgupdate);
        Task<int> GetKGraphCountAsync(string userId);
        Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage);
        Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden);
        Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName);
        Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName, List<string> notFound);
    }
}
