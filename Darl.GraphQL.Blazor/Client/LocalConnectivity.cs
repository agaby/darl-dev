using ThinkBase.ComponentLibrary.Interfaces;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Blazor.Client
{
    public class LocalConnectivity : IClientConnectivity
    {
        public Task<string> AddRealNode(string graphName, string name, string externalId, string lin, string sublin)
        {
            throw new NotImplementedException();
        }

        public Task<Subscription?> AddSubscription(string sub)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateKGraph(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string?> CreateRealConnection(string graphName, string name, string lin, string startId, string endId, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateRecognitionConnection(string graphName, string startId, string endId)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateRecognitionNode(string graphName, string lineage, string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteKGraph(string name)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRealAttribute(string graphName, string id, string aLin)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRealConnection(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRealNode(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRecognitionNode(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVirtualAttribute(string graphName, string id, string aLin)
        {
            throw new NotImplementedException();
        }

        public Task<KGraphDescription?> GetKGraphMetaData(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetKGraphs(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLineagesForWord(string word, string wordType = "")
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLineagesinKG(string graphName, string lType)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNodeCode(string name, string id, IClientConnectivity.GraphSource source)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetRealConnectionLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetRealConnectionName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeAttributes(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeExternalId(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRealNodeTypeWords(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRecognitionLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRecognitionMarkDown(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTypeWord(string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<(string?, bool, bool)> GetUserSettings(string userId, string defaultKG)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeAttributes(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeLineage(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetVirtualNodeName(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsDemo(string graphName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsValidLineage(string lineage)
        {
            throw new NotImplementedException();
        }

        public Task<string> LintAsync(string darl)
        {
            throw new NotImplementedException();
        }

        public Task<string> RealKGraphData(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecognitionKGraphData(string name)
        {
            throw new NotImplementedException();
        }

        public Task SaveKGraph(string graphName)
        {
            throw new NotImplementedException();
        }

        public Task SetActiveKGForBot(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultTarget(string graphName, string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateKGraphMetaData(string name, KGraphDescription desc)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateNodeCode(string graphName, string currentNodeId, string editorText, IClientConnectivity.GraphSource src)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNodeMarkDown(string graphName, string currentNodeId, string markDown, IClientConnectivity.GraphSource src)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealConnectionName(string graphName, string id, string text)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeAttribute(string graphName, string id, string newAtt)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeExternalId(string graphName, string id, string newExternalId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRealNodeName(string graphName, string id, string newName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRecognitionNode(string graphName, string id, string lineage, string word)
        {
            throw new NotImplementedException();
        }

        public Task UpdateVirtualAttribute(string graphName, string id, string newAtt)
        {
            throw new NotImplementedException();
        }

        public Task<string> VirtualKGraphData(string name)
        {
            throw new NotImplementedException();
        }
    }
}
