//using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage;
using Darl.Thinkbase;
using DarlCommon;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default = Darl.GraphQL.Models.Models.Default;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IConnectivity
    {
        IMongoDatabase db { get; set; }

        Task<Authorization> CreateAuthorization(string userId, string name, Authorization auth);

        Task<BotConnection> CreateBotConnection(string userId, string botModelName, string appId, string password);
        Task<Contact> CreateContactAsync(Contact contact);

        Task<Default> CreateDefault(string name, string value);
        Task<BotState> GetBotState(string userId, string conversationId);
        Task<BotModel> CreateDefaultModel(string userId, string name);

        Task<Models.MLModel> CreateEmptyMLModel(string userId, string name);

        Task<BotModel> CreateEmptyModel(string userId, string name);

        Task<Models.RuleSet> CreateEmptyRuleSet(string userId, string name);

        Task<LineageNodeDefinition> CreateLineageNode(string userId, string botModelName, string parent, string newName);

        Task<Models.MLModel> CreateMLModel(string userId, string name, DarlCommon.MLModel model);

        Task<LineageNodeDefinition> CreatePhrase(string userId, string botModelName, string path, LineageNodeAttributes attribute);

        Task<RuleForm> CreateRuleFormFromDarl(string userId, string name, string darl);

        Task<RuleSet> CreateRuleSet(string userId, string name, RuleForm rf, ServiceConnectivity sc);

        Task<StringDoublePair> CreateUpdateConstant(string userId, string botModelName, string name, double value);

        Task<string> CreateUpdateStore(string userId, string botModelName, string name);
        Task SaveBotState(BotState bs);
        Task<StringStringPair> CreateUpdateString(string userId, string botModelName, string name, string value);

        Task<DarlUser> CreateUserAsync(DarlUserInput user);

        Task<DarlUser> CreateAndProvisionNewUser(DarlUserInput user);

        Task<string> DeleteAuthorization(string userId, string name, string name1);

        Task<AzureCredentials> DeleteAzureCredentials(string userId, string botModelName, ModelType modelType);
        Task<List<KGraph>> GetKGraphsAsync(string userId);
        Task<BotConnection> DeleteBotConnection(string userId, string botModelName, string appId);

        Task<BotModel> DeleteBotModel(string userId, string name);

        Task<StringDoublePair> DeleteConstant(string userId, string botModelName, string name);

        Task<Contact> DeleteContactAsync(string id);

        Task<Default> DeleteDefault(string name);

        Task<LineageNodeDefinition> DeleteLineageNode(string userId, string botModelName, string id);

        Task<Models.MLModel> DeleteMLModel(string userId, string name);

        Task<LineageNodeDefinition> DeletePhrase(string userId, string botModelName, string phrase);

        Task<RuleSet> DeleteRuleSet(string userId, string name);

        Task<SellerCenterCredentials> DeleteSellereCenterCredentials(string userId, string botModelName, ModelType modelType);

        Task<SendGridCredentials> DeleteSendgridCredentials(string userId, string botModelName, ModelType modelType);

        Task<string> DeleteStore(string userId, string botModelName, string name);

        Task<StringStringPair> DeleteString(string userId, string botModelName, string name);

        Task<TwilioCredentials> DeleteTwilioCredentials(string userId, string botModelName, ModelType modelType);

        Task<DarlUser> DeleteUser(string id);


        Task<bool> FactoryReset(string userId);

        Task<LineageNodeAttributes> GetAttribute(string userId, string botModelName, string phrase);

        Task<LineageNodeAttributes> GetAttributeFromPath(string userId, string botModelName, string path);

        Task<List<Authorization>> GetAuthorizations(string userId, string name);

        Task<List<BotConnection>> GetBotConnectivity(string userId, string name);

        Task<BotModel> GetBotModel(string userId, string name);
        Task<List<BotModel>> GetBotModelsAsync(string userId);
        Task<List<UserUsage>> GetBotUsage(string appId);

        Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string userId, string botModelName, string path, bool isRoot);

        Task<Contact> GetContactByEmail(string email);

        Task<Contact> GetContactById(string Id);

        Task<List<Contact>> GetContacts();

        Task<List<Contact>> GetContactsByLastName(string lastName);

        Task<string> GetDarlFromRuleset(string userId, string rulesetName);

        Task<List<Default>> GetDefaults();

        Task<string> GetDefaultValue(string name);

        Task<List<DarlVar>> GetExampleInputs(string userId, string ruleSetName);

        Task<LineageModel> GetLineageModel(string userId, string botModelName);

        Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en");

        Task<Models.MLModel> GetMlModel(string userId, string name);
        Task<List<Models.MLModel>> GetMlModelsAsync(string userId);
        Task<RuleSet> GetRuleSet(string userId, string name);
        Task<List<RuleSet>> GetRuleSetsAsync(string userId);
        Task<DarlUser> GetUserByApiKey(string apiKey);

        Task<DarlUser> GetUserById(string id);

        Task<List<DarlUser>> GetUsers();


        Task<List<DarlUser>> GetUsersByEmail(string email);

        string GetCurrentUserId(object userContext);

        Task<List<DarlVar>> InferFromRuleSetDarlVar(string userId, string ruleSetName, List<DarlVarInput> inputs);

        Task<List<DarlLintView>> LintDarl(string darl, string skeleton, string insertion);

        Task<Models.MLModel> MachineLearnModel(string userId, string mlmodelname);

        Task<LineageNodeDefinition> PasteLineageNode(string userId, string botModelName, string parent, List<string> nodes, string mode);

        Task<LineageNodeDefinition> RenameLineageNode(string userId, string botModelName, string id, string newName);

        Task<LineageNodeAttributes> UpdateAttribute(string userId, string botModelName, string path, LineageNodeAttributeUpdate attribute);

        Task<AzureCredentials> UpdateAzureCredentials(string userId, string botModelName, string apiKey, ModelType modelType);

        Task<BotInputFormat> UpdateBotModelInputFormat(string userId, string botModelName, string inputName, InputFormatUpdate inputUpdate);

        Task<BotOutputFormat> UpdateBotModelOutputFormat(string userId, string botModelName, string outputName, BotOutputFormatUpdate outputUpdate);

        Task<Contact> UpdateContactAsync(Contact contact);
        Task<Default> UpdateDefault(string name, string value);

        Task<Models.MLModel> UpdateMLSpec(string userId, string name, MLSpecUpdate mlspec);

        Task<InputFormat> UpdateRuleFormInputFormat(string userId, string name, string inputName, InputFormatUpdate inputUpdate);
        Task<LanguageText> UpdateRuleFormLanguageText(string userId, string ruleSetName, string languageName, string languageText);

        Task<OutputFormat> UpdateRuleFormOutputFormat(string userId, string ruleSetName, string outputName, OutputFormatUpdate outputUpdate);
        Task<VariantText> UpdateRuleFormVariantText(string userId, string ruleSetName, string languageName, string isoLanguageName, string variantText);
        Task<SellerCenterCredentials> UpdateSellerCenterCredentials(string userId, string botModelName, bool liveMode, string merchantId, string stripeApiKey, ModelType modelType);
        Task<SendGridCredentials> UpdateSendgridCredentials(string userId, string botModelName, string sendGridAPIKey, ModelType modelType);
        Task<TwilioCredentials> UpdateTwilioCredentials(string userId, string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword, ModelType modelType);
        Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate darlUserUpdate);
        Task<string> UpdateUserAPIKey(string userId);
        Task<string> UpdateDarlInRuleset(string userId, string ruleSetName, string darl);
        Task<LineageNodeAttributeResources> getLineageNodeAttributeResources(string userId, string botModelName);
        Task<DarlUser> GetUserByStripeId(string stripeId);
        Task<string> GetCollateral(string userId, string name);
        Task<Collateral> UpdateCollateral(string userId, string name, string value);
        Task<Collateral> DeleteCollateral(string userId, string name);
        Task<List<Collateral>> GetCollaterals(string userId);
        Task<DateTime> GetLastUpdate(string from, string to);
        Task<DateTime> SetLastUpdate(string from, string to);
        Task<bool> CreateSupportRequest(string customerName, string customerEmail, string text, string project);
        Task<List<Conversation>> GetConversations();
        Task<Conversation> CreateConversation(Conversation conversationInput);
        Task<UserUsage> CreateUserUsage(DateTime date, int count, string userId);
        Task<UserUsage> CreateBotUsage(DateTime date, int count, string userId, string botId);
        Task<BotRuntimeModel> GetBotModelFromAppId(string appId);
        Task<List<BotConnection>> GetBotConnectionsAsync();
        Task<string> GetUserIdFromAppId(string appId);
        Task CreateDefaultResponse(DefaultResponse response);
        Task<Document> GetDocument(string userId, string name);
        Task<List<Document>> GetDocuments(string userId);
        Task<Document> UpdateDocument(Document document);
        Task<Document> DeleteDocument(string userId, string name);
        Task<DarlVar> CreateRulesetPreload(string userId, string rulesetName, DarlVar preloadData);
        Task<TriggerView> UpdateRuleFormTrigger(string userId, string ruleSetName, TriggerViewInput trigger);
        Task<string> CopyToReserveAccount(string userId, ResourceType resourceType, string name, string newName);
        Task<List<Update>> GetUpdates();
        Task<Purchase> ReportPurchase(string email, string name, string sessionId, DateTime date);
        Task<bool> CheckEmail(string email, string ipaddress = "");
        Task<List<DarlVar>> InferFromDarlDarlVar(string userId, string code, List<DarlVarInput> inputs);
        Task<ModelDetails> CreateRulesetDetails(string userId, string rulesetName, ModelDetails details);
        Task<GraphQLCredentials> UpdateGraphQLCredentials(string userId, string modelName, string url, string header, ModelType modelType);
        Task<GraphQLCredentials> DeleteGraphQLCredentials(string userId, string botModelName, ModelType modelType);
        Task<long> GetContactsCount(string userId);
        Task<long> GetUserCount(string userId);
        Task<long> GetConversationCount(string userId);
        Task<long> GetContactsDayCount(string userId);
        Task<long> GetContactsMonthCount(string userId);
        Task<UserUsage> CreateSimulationUsage(DateTime date, int count, string userId, string model);
        Task<UserUsage> CreateMLModelUsage(DateTime date, int count, string userId, string model);
        Task<UserUsage> CreateRuleSetUsage(DateTime date, int count, string userId, string model);
        Task<UserUsage> CreateBotModelUsage(DateTime date, int count, string userId, string model);
        Task<string> GetTypeWordForLineage(string lineage, string isoLanguage = "en");
        Task<DarlUser.SubscriptionType> GetSubscriptionType(string userId);
        Task<DarlUser.SubscriptionType> UpdateSubscriptionType(string userId, DarlUser.SubscriptionType type);
        Task<bool> CloseAccount(string userId);
        Task<string> CreateKey(string userId, string company, string email, DateTime endDate);
        Task<bool> CheckKey(string userId, string key);
        Task<ModelDetails> UpdateRuleFormDetails(string userId, string ruleSetName, ModelDetails details);
        Task<List<Contact>> GetRecentContacts();
        IQueryable<Contact> GetContactsQueryable();
        Task<KnowledgeState> GetKnowledgeState(string userId, string ksId);
        Task<List<KnowledgeState>> GetKnowledgeStates(string userId);
        Task<KnowledgeState> DeleteKnowledgeState(string userId, string ksId);
        Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state);
        Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state);
        Task<UserUsage> CreateKGModelUsage(DateTime date, int count, string userId, string model);
        Task<KGraph> GetKGModel(string userId, string model);
        Task<KGraph> CreateKGraph(string userId, string name);
        Task<List<DarlLintView>> LintDarlMeta(string darl);
    }
}
