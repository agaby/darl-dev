//using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Default = Darl.GraphQL.Models.Models.Default;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IConnectivity
    {
        string userId { get; set; }

        Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken);

        Task<QuestionSetProxy> BeginQuestionnaire(string ruleSetName, string language = "en", int questCount = 1);

        Task<QuestionSetProxy> ContinueQuestionnaire(QuestionSetInput responses);

        Task<Authorization> CreateAuthorization(string name, Authorization auth);

        Task<BotConnection> CreateBotConnection(string botModelName, string appId, string password);

        Task<Contact> CreateContactAsync(Contact contact);

        Task<Default> CreateDefault(string name, string value);

        Task<BotModel> CreateDefaultModel(string name);

        Task<Models.MLModel> CreateEmptyMLModel(string name);

        Task<BotModel> CreateEmptyModel(string name);

        Task<Models.RuleSet> CreateEmptyRuleSet(string name);

        Task<LineageNodeDefinition> CreateLineageNode(string botModelName, string parent, string newName);

        Task<Models.MLModel> CreateMLModel(string name, DarlCommon.MLModel model);

        Task<LineageNodeDefinition> CreatePhrase(string botModelName, string path, object LineageNodeDefinition);

        Task<RuleForm> CreateRuleFormFromDarl(string name, string darl);

        Task<RuleSet> CreateRuleSet(string name, RuleForm rf, ServiceConnectivity sc);

        Task<StringDoublePair> CreateUpdateConstant(string botModelName, string name, double value);

        Task<string> CreateUpdateStore(string botModelName, string name);

        Task<StringStringPair> CreateUpdateString(string botModelName, string name, string value);

        Task<DarlUser> CreateUserAsync(DarlUserInput user);

        Task<string> DeleteAuthorization(string name, string name1);

        Task<AzureCredentials> DeleteAzureCredentials(string botModelName);

        Task<BotConnection> DeleteBotConnection(string botModelName, string appId);

        Task<BotModel> DeleteBotModel(string name);

        Task<StringDoublePair> DeleteConstant(string botModelName, string name);

        Task<Contact> DeleteContactAsync(string id);

        Task<Default> DeleteDefault(string name);

        Task<LineageNodeDefinition> DeleteLineageNode(string botModelName, string id);

        Task<Models.MLModel> DeleteMLModel(string name);

        Task<LineageNodeDefinition> DeletePhrase(string botModelName, string phrase);

        Task<RuleSet> DeleteRuleSet(string name);

        Task<SellerCenterCredentials> DeleteSellereCenterCredentials(string botModelName);

        Task<SendGridCredentials> DeleteSendgridCredentials(string botModelName);

        Task<string> DeleteStore(string botModelName, string name);

        Task<StringStringPair> DeleteString(string botModelName, string name);

        Task<TwilioCredentials> DeleteTwilioCredentials(string botModelName);

        Task<DarlUser> DeleteUser(string id);

        Task<ZendeskCredentials> DeleteZendeskCredentials(string botModelName);

        Task<List<LineageNodeDefinition>> GetAttribute(string botModelName, string phrase);

        Task<List<LineageNodeDefinition>> GetAttributeFromPath(string botModelName, string path);

        Task<List<Authorization>> GetAuthorizations(string name);

        Task<List<BotConnection>> GetBotConnectivity(string name);

        Task<BotModel> GetBotModel(string name);
        Task<List<BotModel>> GetBotModelsAsync();
        Task<List<UserUsage>> GetBotUsage(string appId, string v);

        Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string botModelName, string path, bool isRoot);

        Task<Contact> GetContactByEmail(string email);

        Task<Contact> GetContactById(string Id);

        Task<List<Contact>> GetContacts();

        Task<List<Contact>> GetContactsByLastName(string lastName);

        Task<List<Default>> GetDefaults();

        Task<string> GetDefaultValue(string name);

        Task<List<DarlVar>> GetExampleInputs(string ruleSetName);

        Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en");

        Task<Models.MLModel> GetMlModel(string name);
        Task<List<Models.MLModel>> GetMlModelsAsync();
        Task<RuleSet> GetRuleSet(string name);
        Task<List<RuleSet>> GetRuleSetsAsync();
        Task<DarlUser> GetUserById(string id);

        Task<List<DarlUser>> GetUsersByEmail(string email);

        Task<List<DarlVar>> InferFromRuleSetDarlVar(string ruleSetName, List<DarlVar> inputs);

        Task<List<DarlLintView>> LintDarl(string darl, string skeleton, string insertion);

        Task<Models.MLModel> MachineLearnModel(string mlmodelname);

        Task<LineageNodeDefinition> PasteLineageNode(string botModelName, string parent, List<string> nodes, string mode);

        Task<LineageNodeDefinition> RenameLineageNode(string botModelName, string id, string newName);

        Task<LineageNodeAttributeUpdate> UpdateAttribute(string botModelName, LineageNodeAttributeUpdate attribute);

        Task<AzureCredentials> UpdateAzureCredentials(string botModelName, string apiKey);

        Task<BotInputFormat> UpdateBotModelInputFormat(string botModelName, string inputName, InputFormatUpdate inputUpdate);

        Task<BotOutputFormat> UpdateBotModelOutputFormat(string botModelName, string outputName, BotOutputFormatUpdate outputUpdate);

        Task<Contact> UpdateContactAsync(Contact contact);
        Task<Default> UpdateDefault(string name, string value);

        Task<Models.MLModel> UpdateMLSpec(string name, MLSpecUpdate mlspec);

        Task<InputFormat> UpdateRuleFormInputFormat(string name, string inputName, InputFormatUpdate inputUpdate);
        Task<LanguageText> UpdateRuleFormLanguageText(string ruleSetName, string languageName, string languageText);

        Task<OutputFormat> UpdateRuleFormOutputFormat(string ruleSetName, string outputName, OutputFormatUpdate outputUpdate);
        Task<VariantText> UpdateRuleFormVariantText(string ruleSetName, string languageName, string isoLanguageName, string variantText);
        Task<SellerCenterCredentials> UpdateSellerCenterCredentials(string botModelName, bool liveMode, string merchantId, string stripeApiKey);
        Task<SendGridCredentials> UpdateSendgridCredentials(string botModelName, string sendGridAPIKey);

        Task<TwilioCredentials> UpdateTwilioCredentials(string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword);
        Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate);

        Task<ZendeskCredentials> UpdateZendeskCredentials(string botModelName, string zendeskApiKey, string zendeskURL, string zendeskUser);
    }
}
