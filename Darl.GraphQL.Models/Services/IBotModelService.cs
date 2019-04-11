using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IBotModelService
    {
        Task<BotModel> GetBotModel(string name);
        Task<List<BotModel>> GetBotModelsAsync();

        Task<BotModel> DeleteModel(String name);

        Task<BotModel> CreateEmptyModel(string name);

        Task<BotModel> CreateDefaultModel(string name);
    }
}
