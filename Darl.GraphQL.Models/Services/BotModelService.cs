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
        public async Task<BotModel> GetBotModelAsync(string name)
        {
            return await Connectivity.GetBotModelAsync(name);
        }

        public async Task<List<BotModel>> GetBotModelsAsync()
        {
            return await Connectivity.GetBotModelsAsync();
        }
    }
}
