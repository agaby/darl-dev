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
        Task<BotModel> GetBotModelAsync(string name);
        Task<List<BotModel>> GetBotModelsAsync();
        Task<Models.MLModel> GetMlModelAsync(string name);
        Task<List<Models.MLModel>> GetMlModelsAsync();
        Task<DarlCommon.MLModel> GetMlInternalModelAsync(string name);
        Task<List<BotUsage>> GetBotUsage(string appId);
        Task<List<TableAuthorizations>> GetAuthorizations(string name);
        Task<RuleForm> GetRuleFormAsync(string name);
        Task<List<ConnectivityView>> GetBotConnectivity(string name);
        Task<RuleSet> GetRuleSetAsync(string name);
        Task<List<RuleSet>> GetRuleSetsAsync();
        Task<LineageModel> GetLineageModelAsync(string name);
        string userId { get; set; }
        Task<ServiceConnectivity> GetServiceConnectivity();
        Task<List<Contact>> GetContacts();
        Task<List<Contact>> GetContactsByLastName(string lastName);
        Task<List<Contact>> GetContactsByEmail(string email);
        Task<List<Default>> GetDefaults();
        Task<string> GetDefaultValue(string name);
    }
}
