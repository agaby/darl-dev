using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    public class BotModelService : IBotModelService
    {

        public BotModelService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }

        IConnectivity Connectivity;
        public Task<BotModel> GetBotModel(string name)
        {
            return Task.FromResult(Connectivity.GetBotModel(name));
        }

        public async Task<List<BotModel>> GetBotModelsAsync()
        {
            return await Connectivity.GetBotModelsAsync();
        }
    }
}
