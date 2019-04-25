using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using Gremlin.Net;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Options;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GremlinConnectivity : IConnectivity
    {
        public string userId { get; set; }


        IOptions<AppSettings> _opt;

        private GremlinServer gremlinServer;
        private GremlinClient gremlinClient;

        public GremlinConnectivity(IOptions<AppSettings> optionsAccessor)
        {
            _opt = optionsAccessor;
            gremlinServer = new GremlinServer(_opt.Value.GremlinHostName, 443, enableSsl: true, username: "/dbs/darlaigraph/colls/darlai", password: _opt.Value.GremlinAuthKey);
            gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);
        }

        public Task<string> CreateAuthorization(string name, string name1)
        {
            throw new NotImplementedException();
        }

        public Task<BotConnection> CreateBotConnection(string botModelName, string appId, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Task<object> CreateDefaultModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Models.MLModel> CreateEmptyMLModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<BotModel> CreateEmptyModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<RuleSet> CreateEmptyRuleSet(string name)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> CreateLineageNode(string botModelName, string parent, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<Models.MLModel> CreateMLModel(string name, DarlCommon.MLModel model)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> CreatePhrase(string botModelName, string path, object LineageNodeDefinition)
        {
            throw new NotImplementedException();
        }

        public Task<RuleForm> CreateRuleFormFromDarl(string name, string darl)
        {
            throw new NotImplementedException();
        }

        public Task<RuleSet> CreateRuleSet(string name, RuleForm rf, ServiceConnectivity sc)
        {
            throw new NotImplementedException();
        }

        public Task<StringDoublePair> CreateUpdateConstant(string botModelName, string name, double value)
        {
            throw new NotImplementedException();
        }

        public Task<Default> CreateUpdateDefault(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateUpdateStore(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<StringStringPair> CreateUpdateString(string botModelName, string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteAuthorization(string name, string name1)
        {
            throw new NotImplementedException();
        }

        public Task<AzureCredentials> DeleteAzureCredentials(string botModelName)
        {
            throw new NotImplementedException();
        }

        public Task<BotConnection> DeleteBotConnection(string botModelName, string appId)
        {
            throw new NotImplementedException();
        }

        public Task<BotModel> DeleteBotModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<StringDoublePair> DeleteConstant(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> DeleteContactAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Default> DeleteDefault(string name)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> DeleteLineageNode(string botModelName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<Models.MLModel> DeleteMLModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> DeletePhrase(string botModelName, string phrase)
        {
            throw new NotImplementedException();
        }

        public Task<RuleSet> DeleteRuleSet(string name)
        {
            throw new NotImplementedException();
        }

        public Task<SellerCenterCredentials> DeleteSellereCenterCredentials(string botModelName)
        {
            throw new NotImplementedException();
        }

        public Task<SendGridCredentials> DeleteSendgridCredentials(string botModelName)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteStore(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<StringStringPair> DeleteString(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<TwilioCredentials> DeleteTwilioCredentials(string botModelName)
        {
            throw new NotImplementedException();
        }

        public Task<ZendeskCredentials> DeleteZendeskCredentials(string botModelName)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetAttribute(string botModelName, string phrase)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetAttributeFromPath(string botModelName, string path)
        {
            throw new NotImplementedException();
        }

        public Task<List<Authorization>> GetAuthorizations(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<BotConnection>> GetBotConnectivity(string name)
        {
            throw new NotImplementedException();
        }

        public Task<BotModel> GetBotModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<BotModel>> GetBotModelsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserUsage>> GetBotUsage(string appId, string v)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string botModelName, string path, bool isRoot)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetContactById(string Id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Contact>> GetContacts()
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetContactsByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Default>> GetDefaults()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultValue(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageRecord>> GetLineagesForWord(string isoLanguage, string word)
        {
            throw new NotImplementedException();
        }

        public Task<Models.MLModel> GetMlModel(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.MLModel>> GetMlModelsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RuleSet> GetRuleSet(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<RuleSet>> GetRuleSetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlVar>> InferFromRuleSetDarlVar(string ruleSetName, List<DarlVar> inputs)
        {
            throw new NotImplementedException();
        }

        public Task<List<StringStringPair>> InferFromRulesetSimple(string ruleSetName, List<StringStringPair> inputs)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> PasteLineageNode(string botModelName, string parent, List<string> nodes, string mode)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> RenameLineageNode(string botModelName, string id, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeAttributeUpdate> UpdateAttribute(string botModelName, LineageNodeAttributeUpdate attribute)
        {
            throw new NotImplementedException();
        }

        public Task<AzureCredentials> UpdateAzureCredentials(string botModelName, string apiKey)
        {
            throw new NotImplementedException();
        }

        public Task<BotInputFormat> UpdateBotModelInputFormat(string botModelName, string inputName, InputFormatUpdate inputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<BotOutputFormat> UpdateBotModelOutputFormat(string botModelName, string outputName, BotOutputFormatUpdate outputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> UpdateContactAsync(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Task<InputFormat> UpdateRuleFormInputFormat(string name, string inputName, InputFormatUpdate inputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<LanguageText> UpdateRuleFormLanguageText(string ruleSetName, string languageName, string languageText)
        {
            throw new NotImplementedException();
        }

        public Task<OutputFormat> UpdateRuleFormOutputFormat(string ruleSetName, string outputName, OutputFormatUpdate outputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<VariantText> UpdateRuleFormVariantText(string ruleSetName, string languageName, string isoLanguageName, string variantText)
        {
            throw new NotImplementedException();
        }

        public Task<SellerCenterCredentials> UpdateSellereCenterCredentials(string botModelName, bool liveMode, string merchantId, string stripeApiKey)
        {
            throw new NotImplementedException();
        }

        public Task<SendGridCredentials> UpdateSendgridCredentials(string botModelName, string sendGridAPIKey)
        {
            throw new NotImplementedException();
        }

        public Task<TwilioCredentials> UpdateTwilioCredentials(string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword)
        {
            throw new NotImplementedException();
        }

        public Task<ZendeskCredentials> UpdateZendeskCredentials(string botModelName, string zendeskApiKey, string zendeskURL, string zendeskUser)
        {
            throw new NotImplementedException();
        }

        public Task<Authorization> CreateAuthorization(string name, Authorization auth)
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> GetUserById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> CreateUserAsync(DarlUserInput contact)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> UpdateUserAsync(DarlUserUpdate darlUserUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<DarlUser> DeleteUser(string id)
        {
            throw new NotImplementedException();
        }
    }
}
