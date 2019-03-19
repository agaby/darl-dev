using Darl.GraphQL.Models.Models;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    interface IConnectivity
    {
        Task<BotModel> GetBotModelAsync(string name);
        Task<List<BotModel>> GetMlBotModelsAsync(string name);
        Task<DarlCommon.MLModel> GetMlModelAsync(string name);
        Task<List<DarlCommon.MLModel>> GetMlModelsAsync(string name);
        Task<RuleForm> GetRuleFormAsync(string name);
        Task<List<Models.MLModel>> GetRuleFormsAsync(string name);
    }
}
