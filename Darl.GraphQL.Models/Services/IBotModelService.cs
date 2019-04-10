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

        Task DeleteModel(String name);

        Task CreateEmptyModel(string name);

        Task CreateDefaultModel(string name);
    }
}
