using Darl.GraphQL.Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IGraphProcessing
    {

        public enum OntologyAction { build, check, ignore };

        Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage);
        Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float similaity);
        Task<GraphObject> GetGraphObjectById(string userId, string id);
        Task<GraphObject> CreateGraphObject(string userId, GraphObjectInput graphObject, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphConnection> CreateGraphConnection(string userId, GraphConnectionInput graphConnection, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphObject> DeleteGraphObject(string userId, string id);
        Task<GraphConnection> DeleteGraphConnection(string userId, string id);
        Task<GraphObject> UpdateGraphObject(string userId, GraphObjectUpdate graphObject, OntologyAction ontology = OntologyAction.ignore);
        Task<GraphConnection> UpdateGraphConnection(string userId, GraphConnectionUpdate graphConnection, OntologyAction ontology = OntologyAction.ignore);
        Task<string> gremlinPassThrough(string userId, string query);
        Task<bool> CreateNewGraph(string userId, string partitionKey);
        Task<InferenceRecord> InferPath(GraphObjectInput start, GraphObjectInput end, string userId, string targetOutput);
    }
}
