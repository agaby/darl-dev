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
        Task<Models.RuleSet> CreateEmptyRuleSet(string name);
        Task<Models.MLModel> CreateEmptyMLModel(string name);
        Task<List<BotUsage>> GetBotUsage(string appId);
        Task DeleteRuleSet(string name);
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
        Task<Contact> CreateContactAsync(Contact contact);
        Task<Contact> UpdateContactAsync(Contact contact);
        Task<Contact> GetContactById(string Id);
        Task DeleteContactAsync(string id);
        Task<RuleForm> CreateRuleFormFromDarl(string name, string darl);
        Task<InputFormat> UpdateRuleFormInputFormat(string name, string inputName, InputFormatUpdate inputUpdate);
        Task<OutputFormat> UpdateRuleFormOutputFormat(string ruleSetName, string outputName, OutputFormatUpdate outputUpdate);
        Task<LanguageText> UpdateRuleFormLanguageText(string ruleSetName, string languageName, string languageText);
        Task<VariantText> UpdateRuleFormVariantText(string ruleSetName, string languageName, string isoLanguageName, string variantText);
        Task<TableAuthorizations> CreateAuthorization(string name, string name1);
        Task<Default> CreateUpdateDefault(string name, string value);
        Task<Default> DeleteDefault(string name);
        Task<TableAuthorizations> DeleteAuthorization(string name, string name1);
        Task<ConnectivityView> CreateBotConnection(string botModelName, string appId, string password);
        Task<ConnectivityView> DeleteBotConnection(string botModelName, string appId);
        Task<BotInputFormat> UpdateBotModelInputFormat(string botModelName, string inputName, InputFormatUpdate inputUpdate);
        Task<BotOutputFormat> UpdateBotModelOutputFormat(string botModelName, string outputName, BotOutputFormatUpdate outputUpdate);
        Task<StringDoublePair> CreateUpdateConstant(string botModelName, string name, double value);
        Task<StringDoublePair> DeleteConstant(string botModelName, string name);
        Task<string> CreateUpdateStore(string botModelName, string name);
        Task<string> DeleteStore(string botModelName, string name);
        Task<StringStringPair> CreateUpdateString(string botModelName, string name, string value);
        Task<StringStringPair> DeleteString(string botModelName, string name);
        Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string botModelName, string path, bool isRoot);
        Task<List<LineageRecord>> GetLineagesForWord(string isoLanguage, string word);
        Task<List<LineageNodeDefinition>> GetAttribute(string botModelName, string phrase);
        Task<List<LineageNodeDefinition>> GetAttributeFromPath(string botModelName, string path);
        Task<LineageNodeDefinitionUpdate> UpdateAttribute(string botModelName, LineageNodeDefinitionUpdate attribute);
        Task<LineageNodeDefinition> CreateLineageNode(string botModelName, string parent, string newName);
    }
}
