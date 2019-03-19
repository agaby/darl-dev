using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    public class BotModelService : IBotModelService
    {
        public Task<BotModel> GetBotModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<BotModel>> GetMlBotModelsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
