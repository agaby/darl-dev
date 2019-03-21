using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IConnectivity
    {
        Task<BotModel> GetBotModelAsync(string name);
        Task<List<BotModel>> GetBotModelsAsync();
        Task<DarlCommon.MLModel> GetMlModelAsync(string name);
        Task<List<DarlCommon.MLModel>> GetMlModelsAsync();
        Task<RuleForm> GetRuleFormAsync(string name);
        Task<List<RuleForm>> GetRuleFormsAsync();
        Task<LineageModel> GetLineageModelAsync(string name);
    }
}
