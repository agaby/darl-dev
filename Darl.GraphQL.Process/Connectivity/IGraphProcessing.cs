using Darl.GraphQL.Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IGraphProcessing
    {
        Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage);
        Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float similaity);
        Task<GraphObject> GetGraphObjectById(string userId, string id);
        Task<GraphObject> CreateGraphObject(string userId, GraphObjectInput graphObject, bool definitive = false);
        Task<GraphConnection> CreateGraphConnection(string userId, GraphConnectionInput graphConnection, bool definitive = false);
        Task<GraphObject> DeleteGraphObject(string userId, string id);
        Task<GraphConnection> DeleteGraphConnection(string userId, string id);
        Task<GraphObject> UpdateGraphObject(string userId, GraphObjectUpdate graphObject, bool definitive = false);
        Task<GraphConnection> UpdateGraphConnection(string userId, GraphConnectionUpdate graphConnection, bool definitive = false);
        Task<string> gremlinPassThrough(string userId, string query);
        Task<bool> CreateNewGraph(string userId);
    }
}
