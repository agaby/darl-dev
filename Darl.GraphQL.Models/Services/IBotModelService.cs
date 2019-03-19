using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IBotModelService
    {
        Task<BotModel> GetBotModelAsync(string name);
        Task<List<BotModel>> GetMlBotModelsAsync(string name);
    }
}
