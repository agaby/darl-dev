using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IGraphProcessing
    {
        Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage);
        Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float distance);
        Task<GraphObject> GetGraphObjectById(string userId, string id);
    }
}
