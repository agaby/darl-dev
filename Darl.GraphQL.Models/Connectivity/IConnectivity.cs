using Darl.Connectivity.Models;
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
        BotModel GetBotModel(string name);
        Task<List<BotModel>> GetBotModelsAsync();
        Models.MLModel GetMlModel(string name);
        Task<List<Models.MLModel>> GetMlModelsAsync();
        Task<DarlCommon.MLModel> GetMlInternalModelAsync(string name);
        Task<Models.MLModel> CreateEmptyMLModel(string name);
        Task<List<BotUsage>> GetBotUsage(string appId);
        Task<List<TableAuthorizations>> GetAuthorizations(string name);
        Task DeleteMLModel(string name);
        Task<RuleForm> GetRuleFormAsync(string name);
        Task<List<ConnectivityView>> GetBotConnectivity(string name);
        RuleSet GetRuleSet(string name);
        Task<List<RuleSet>> GetRuleSetsAsync();
        Task<LineageModel> GetLineageModelAsync(string name);
        Task DeleteBotModel(string name);

        string userId { get; set; }
        Task<ServiceConnectivity> GetServiceConnectivity();
        Task<BotModel> CreateEmptyModel(string name);
        Task<List<Contact>> GetContacts();
        Task<List<Contact>> GetContactsByLastName(string lastName);
        Task<List<Contact>> GetContactsByEmail(string email);
        Task<List<Default>> GetDefaults();
        Task<string> GetDefaultValue(string name);
        Task SaveModel(string userId, string modelName, LineageModel model);
    }
}
