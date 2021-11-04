using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{

    public enum OntologyAction { build, check, ignore };
    public enum GraphElementType { node, connection, attribute };

    public interface IGraphProcessing
    {
        Dictionary<string, LineageDefinitionNode> PreloadLineages { get; }

        Task<List<GraphObject>> GetGraphObjects(string compositeName, string name, string lineage);
        Task<GraphObject> GetGraphObjectById(string compositeName, string id);
        Task<GraphObject> GetVirtualObjectByLineage(string compositeName, string lineage);
        Task<GraphObject> GetGraphObjectByExternalId(string compositeName, string externalId);
        Task<GraphObject> CreateGraphObject(string compositeName, GraphObjectInput graphObject, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphConnection> CreateGraphConnection(string compositeName, GraphConnectionInput graphConnection, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphObject> DeleteGraphObject(string compositeName, string id);
        Task<GraphConnection> DeleteGraphConnection(string compositeName, string id);
        Task<GraphObject> UpdateGraphObject(string compositeName, GraphObjectUpdate graphObject, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphConnection?> UpdateGraphConnection(string compositeName, GraphConnectionUpdate graphConnection, OntologyAction ontology = OntologyAction.ignore);
        Task<bool> CreateNewGraph(string userId, string name);
        Task<string> GetGraphObjectProperty(string compositeName, string id, string property);
        Task<GraphConnection> GetConnectionByIds(string compositeName, string startId, string endId, string lineage);
        Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID);
        Task<string> ProcessAttribute(string compositeName, string externalID, string propertyName);
        Task<List<StringStringPair>> ProcessCategories(string compositeName, string rootName, string childLineage, string childValueAttribute);
        Task<List<StringStringPair>> ProcessCategories(string compositeName, string childLineage, string childValueAttribute);
        Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage);
        Task Store(string compositeName);
        Task<bool> DeleteGraph(string userId, string name);
        Task<List<string>> GetGraphs(string userId);
        Task LoadGraphML(string compositeName, Stream graphML, List<StringStringPair> attributes);
        Task<Stream> StoreGraphML(string compositeName);
        Task CreateVirtualAttribute(string compositeName, string lineage, GraphAttributeInput att);
        Task<IGraphModel> GetModel(string userId, string name);
        Task<List<MatchedElement>> Match(string v, string subjectId, List<string> tokens);
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
        Task<GraphObject> GetRecognitionObjectById(string compositeName, string id);
        Task SaveKSChanges(string userId, string subjectId, KnowledgeState ks);
        Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, string graphName, bool externalIds);
        Task ClearGraphContent(string compositeName);
        Task<string> CopyRenameKG(string userId, string name, string newName);
        Task<GraphAttribute> UpdateRecognitionObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt);
        Task<GraphAttribute> UpdateVirtualObjectAttribute(string compositeName, string objlineage, GraphAttributeInput graphAtt);
        Task<GraphAttribute> DeleteRecognitionObjectAttribute(string compositeName, string objectId, string graphLineage);
        Task<GraphAttribute> DeleteVirtualObjectAttribute(string compositeName, string objLineage, string graphLineage);
        Task<GraphAttribute> UpdateGraphObjectAttribute(string compositeName, string objectId, GraphAttributeInput graphAtt);
        Task<GraphAttribute> DeleteGraphObjectAttribute(string compositeName, string objectId, string graphLineage);
        Task<List<LineageRecord>> GetLineagesInKG(string compositeName, GraphElementType gtype);
        Task<GraphConnection> GetConnectionById(string compositeName, string id);
        Task<KnowledgeState> GetKnowledgeState(string userId, string Id, string graphName, bool external = false);
        bool FindMetaDisplayStructure(IGraphModel model, GraphObject res, ref DarlVar? pending, List<InteractTestResponse> responses);
        string FindDisplayAttribute(IGraphModel model, string id);
        Task<VRDisplayModel> GetRealVRDisplayGraph(string compositeName, string lineageFilter);
        void HandleCodelessValue(IGraphModel model, GraphObject res, DarlVar? pending, List<DarlVar> values, KnowledgeState ks);
        void HandleCodelessCompletion(IGraphModel model, GraphObject res, KnowledgeState ks);
        Task<GraphAttribute> GetGraphAttribute(string userId, string graphName, string externalId, string lineage, string? ksUserId = null);
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
        public Task<string> CreateTimedAccessUrl(string userId, string name);
        Task<ModelMetaData> UpdateKGraph(string userId, string name, ModelMetaData kgupdate);
        Task<bool> Exists(string userId, string name);
        Task<List<KnowledgeState>> CreateKnowledgeStateList(string userId, List<KnowledgeStateInput> states);
    }
}
