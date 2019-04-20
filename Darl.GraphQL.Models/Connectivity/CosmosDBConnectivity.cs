using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Default = Darl.GraphQL.Models.Models.Default;
using MLModel = Darl.GraphQL.Models.Models.MLModel;

namespace Darl.GraphQL.Models.Connectivity
{
    public class CosmosDBConnectivity : IConnectivity
    {
        public string userId { get; set; }

        MongoClient mongoClient;
        IMongoDatabase db;

        IOptions<AppSettings> _opt;

        public CosmosDBConnectivity(IOptions<AppSettings> optionsAccessor)
        {
            _opt = optionsAccessor;

            string connectionString =  _opt.Value.MongoConnectionString;
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            mongoClient = new MongoClient(settings);
            userId = _opt.Value.boaiuserid;
            db = mongoClient.GetDatabase(_opt.Value.MongoDatabase);
            BsonClassMap.RegisterClassMap<LineageRecord>();
        }

        public Task<string> CreateAuthorization(string botModelName, string authorizationName)
        {
            throw new NotImplementedException();
        }

        public async Task<BotConnection> CreateBotConnection(string botModelName, string appId, string password)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var bm = new BotConnection { AppId = appId, Password = password };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Push("botconnections", bm);
            await collection.FindOneAndUpdateAsync(filter, update);
            return bm;
        }

        public async Task<BotModel> CreateBotModel(string name, LineageModel lm, ServiceConnectivity sc, List<Authorization> authorizations, List<BotConnection> botConnections)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var model = new BotModel { Name = name, userId = userId,  Authorizations = authorizations, serviceConnectivity = sc, Model = lm, botconnections = botConnections };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            var mc = db.GetCollection<Contact>("contact");
            await mc.InsertOneAsync(contact);
            return contact;
        }

        public async Task<Models.MLModel> CreateEmptyMLModel(string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var model = new MLModel { Name = name, model = new DarlCommon.MLModel { name = name, percentTest = 0, sets = 3, darl = "ruleset newRuleSet supervised\n{\n}\n"}, results = new List<MLResult>(), userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<BotModel> CreateEmptyModel(string name)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var model = new BotModel { Name = name, userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<RuleSet> CreateEmptyRuleSet(string name)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var model = new RuleSet {  Name = name, userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<RuleSet> CreateRuleSet(string name, RuleForm rf, ServiceConnectivity sc)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var model = new RuleSet { Name = name, userId = userId, Contents = rf, serviceConnectivity = sc };
            await mc.InsertOneAsync(model);
            return model;
        }

        public Task<LineageNodeDefinition> CreateLineageNode(string botModelName, string parent, string newName)
        {
            throw new NotImplementedException();
        }

        public async Task<MLModel> CreateMLModel(string name, DarlCommon.MLModel model)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var mm = new MLModel { Name = name, userId = userId, model =  model};
            await mc.InsertOneAsync(mm);
            return mm;
        }

        public Task<LineageNodeDefinition> CreatePhrase(string botModelName, string path, object LineageNodeDefinition)
        {
            throw new NotImplementedException();
        }

        public Task<RuleForm> CreateRuleFormFromDarl(string name, string darl)
        {
            throw new NotImplementedException();
        }

        public Task<StringDoublePair> CreateUpdateConstant(string botModelName, string name, double value)
        {
            throw new NotImplementedException();
        }

        public async Task<Default> CreateUpdateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>("default");
            var model = new Default { Name = name, Value = value };
            await mc.ReplaceOneAsync(Builders<Default>.Filter.Eq(r => r.Name, "name"), model, new UpdateOptions() { IsUpsert = true });
            return model;
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

        public async Task<BotModel> DeleteBotModel(string name)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var query = mc.AsQueryable().Where(p => p.userId == userId && p.Name == name);
            var old =  await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<BotModel>.Filter.Eq(r => r.userId, userId) & Builders<BotModel>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public Task<StringDoublePair> DeleteConstant(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> DeleteContactAsync(string id)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable().Where(p => p.Id == id);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Contact>.Filter.Eq(r => r.Id, id));
            return old;
        }

        public Task<Default> DeleteDefault(string name)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinition> DeleteLineageNode(string botModelName, string id)
        {
            throw new NotImplementedException();
        }

        public Task<MLModel> DeleteMLModel(string name)
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

        public Task<List<string>> GetAuthorizations(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<BotConnection>> GetBotConnectivity(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<BotModel> GetBotModel(string name)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == name);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<BotModel>> GetBotModelsAsync()
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public Task<List<BotUsage>> GetBotUsage(string appId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string botModelName, string path, bool isRoot)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> GetContactById(string Id)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.Id == Id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Contact>> GetContacts()
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<Contact> GetContactsByEmail(string email)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.Email == email);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.LastName == lastName);
            return await query.ToListAsync();
        }

        public async Task<List<Default>> GetDefaults()
        {
            var mc = db.GetCollection<Default>("default");
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<string> GetDefaultValue(string name)
        {
            var mc = db.GetCollection<Default>("default");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? string.Empty : def.Value;
        }

        public Task<List<LineageRecord>> GetLineagesForWord(string isoLanguage, string word)
        {
            throw new NotImplementedException();
        }


        public async Task<Models.MLModel> GetMlModel(string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync(); 
        }

        public async Task<List<Models.MLModel>> GetMlModelsAsync()
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<RuleSet> GetRuleSet(string name)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<RuleSet>> GetRuleSetsAsync()
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public Task<ServiceConnectivity> GetServiceConnectivity()
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

        public Task<SellerCenterCredentials> UpdateSellereCenterCredentials(string botModelName, string liveMode, string merchantId, string stripeApiKey)
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

        public Task<object> CreateDefaultModel(string name)
        {
            throw new NotImplementedException();
        }
    }
}
