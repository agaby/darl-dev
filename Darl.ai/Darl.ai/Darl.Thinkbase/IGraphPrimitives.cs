using Darl.Lineage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{

    /// <summary>
    /// Back end processes that communicate with a graph database or in memory representation
    /// </summary>
    public interface IGraphPrimitives
    {
        Task<GraphConnection> CreateConnection(string compositeName, GraphConnectionInput conn);
        Task CreateVirtualObject(IGraphModel model, string lineage, string typeword, string description);
        Task CreateVirtualConnection(IGraphModel model, string child, string lineage, string connectionLabel);
        Task<bool> LineageExists(IGraphModel model, string lineage);
        Task<bool> VirtualAssociationExists(IGraphModel model, string lineage1, string lineage2);
        Task<GraphObject?> CreateObject(string compositeName, GraphObjectInput graphObject);
        Task<GraphConnection?> DeleteConnection(string compositeName, string id);
        Task<GraphObject?> DeleteObject(string compositeName, string id);
        Task<GraphObject?> UpdateObject(string compositeName, GraphObjectUpdate graphObject);
        Task<GraphConnection?> UpdateConnection(string compositeName, GraphConnectionUpdate graphConnection);
        Task<List<GraphObject>?> GetGraphObjects(string compositeName, string name, string lineage);
        Task<GraphObject?> GetGraphObjectById(string compositeName, string id);
        Task<GraphObject?> GetGraphObjectByExternalId(string compositeName, string externalId);
        Task<GraphConnection?> GetConnectionByIds(string compositeName, string startId, string endId, string lineage);
        Task<string> GetGraphObjectProperty(string compositeName, string id, string property);
        Task<bool> Exists(string compositeName);
        Task<bool> CreateModel(string compositeName);
        Task<bool> DeleteModel(string compositeName);
        Task<List<string>> ListModels(string userId);
        Task<IGraphModel> Load(string compositeName);
        Task Store(string compositeName);
        Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID);
        Task<List<StringStringPair>> GetLinkedCategories(string compositeName, string rootName, string childLineage, string childValueAttribute);
        Task<List<StringStringPair>> GetCategoriesByLineage(string compositeName, string childLineage, string childValueAttribute);
        Task<string> GetAttribute(string compositeName, string externalID, string propertyName);
        List<GraphElement>? ShortestPath(IGraphModel model, GraphObject start, GraphObject target);
        Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage);
        Task<List<GraphObject>> GetAllRealObjects(string compositeName);
        Task<IEnumerable<GraphObject>> GetAllVirtualObjects(string compositeName);
        Task<IEnumerable<GraphConnection>> GetAllRealConnections(string compositeName);
        Task<IEnumerable<GraphConnection>> GetAllVirtualConnections(string compositeName);
        Task CreateRawObject(IGraphModel model, GraphObjectInput graphObject);
        Task CreateRawConnection(IGraphModel model, GraphConnection graphConnection);
        Task CreateVirtualAttribute(string compositeName, string lineage, GraphAttributeInput att);
        Task<GraphObject> GetRecognitionRoot(IGraphModel model, string subjectId);
        Task<GraphObject> CreateRecognitionRoot(string compositeName, string rootLineage);
        Task<GraphConnection> CreateRecognitionConnection(string compositeName, GraphConnectionInput graphConnection);
        Task<GraphObject> CreateRecognitionObject(string compositeName, GraphObjectInput graphObject);
        Task<GraphObject> DeleteRecognitionObject(string compositeName, string id);
        Task<GraphObject> DeleteRecognitionRoot(string compositeName, string rootLineage);
        Task<GraphObject> UpdateRecognitionObject(string compositeName, GraphObjectUpdate graphObject);
        Task<GraphObject> UpdateVirtualObject(string compositeName, GraphObjectUpdate graphObject, bool merge = false);
        Task<List<GraphObject>> NavigateRecognition(string compositeName, string root, string path);
        Task<GraphObject> FindRecognition(string compositeName, string root, string path);
        Task<DisplayModel> GetRealDisplayGraph(string compositeName, string lineageFilter);
        Task<DisplayModel> GetVirtualDisplayGraph(string compositeName);
        Task<DisplayModel> GetRecognitionDisplayGraph(string compositeName);
        Task<GraphObject> GetVirtualObjectByLineage(string compositeName, string lineage);
        Task<GraphObject> GetRecognitionObjectById(string compositeName, string id);
        Task CorrectBrokenLinks(string compositeName);
        Task SaveKSChanges(string userId, string subjectId, KnowledgeState ks);
        Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId, string graphName, bool external);
        Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, string graphName, bool externalIds);
        Task ClearGraphContent(string compositeName);
        Task<string> CopyRenameKG(string userId, string name, string newName);
        Task<GraphAttribute> UpdateRecognitionObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt);
        Task<GraphAttribute> UpdateVirtualObjectAttribute(string compositeName, string objectLineage, GraphAttributeInput graphAtt);
        Task<GraphAttribute> DeleteRecognitionObjectAttribute(string compositeName, string objectId, string graphLineage);
        Task<GraphAttribute> DeleteVirtualObjectAttribute(string compositeName, string objectLineage, string graphLineage);
        Task<GraphAttribute> UpdateGraphObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt);
        Task<GraphAttribute> DeleteGraphObjectAttribute(string compositeName, string objectId, string graphLineage);
        Task<List<LineageRecord>> GetLineagesInKG(string compositeName, GraphElementType gtype);
        Task<GraphConnection> GetConnectionById(string compositeName, string id);
        Task<VRDisplayModel> GetRealVRDisplayGraph(string compositeName, string lineageFilter);
        Task<int> GetKGraphCountAsync(string userId);
        Task<string> GetGraphObjectToString(string compositeName, string id);
        Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName);
        Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue);
        Task<KnowledgeState> DeleteKnowledgeState(string userId, string subjectId, string graphName);
        Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage);
        Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden);
        Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName);
        string CreateTimedAccessUrl(string userId, string name);
        Task<ModelMetaData> UpdateKGraph(string userId, string name, ModelMetaData kgupdate);
        Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName);
    }
}
