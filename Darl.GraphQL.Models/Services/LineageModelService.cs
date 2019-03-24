using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage;

namespace Darl.GraphQL.Models.Services
{
    public class LineageModelService : ILineageModelService
    {
        IConnectivity Connectivity;

        public LineageModelService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }

        public async Task<List<TableAuthorizations>> GetAuthorizations(string name)
        {
            return await Connectivity.GetAuthorizations($"{name}.model");
        }

        public async Task<List<ConnectivityView>> GetBotConnectivity(string name)
        {
            return await Connectivity.GetBotConnectivity($"{name}.model");
        }

        public async Task<LineageModel> GetLineageModelAsync(string name)
        {
            return await Connectivity.GetLineageModelAsync(name);
        }

    }
}
