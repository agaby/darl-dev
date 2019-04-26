using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using GraphQL;
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

        public async Task<Authorization> CreateAuthorization(string botModelName, Authorization auth)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Push("Authorizations", auth);
            await collection.FindOneAndUpdateAsync(filter, update);
            return auth;
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
            try
            {
                contact.Id = Guid.NewGuid().ToString();
                var mc = db.GetCollection<Contact>("contact");
                await mc.InsertOneAsync(contact);
                return contact;
            }
            catch(Exception ex)
            {
                throw new ExecutionError("Duplicate or malformed data"); 
            }
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

        public async Task<Default> CreateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>("default");
            var model = new Default { Name = name, Value = value };
            await mc.InsertOneAsync(model);
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
            try
            {
                var mc = db.GetCollection<Contact>("contact");
                var query = mc.AsQueryable().Where(p => p.Id == id);
                var old = await query.FirstOrDefaultAsync();
                var res = await mc.DeleteOneAsync(Builders<Contact>.Filter.Eq(r => r.Id, id));
                return old;
            }
            catch(Exception ex)
            {
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<Default> DeleteDefault(string name)
        {
            var mc = db.GetCollection<Default>("default");
            var query = mc.AsQueryable().Where(p => p.Name == name);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Default>.Filter.Eq(r => r.Name, name));
            return old;
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

        public async Task<List<Authorization>> GetAuthorizations(string name)
        {
            
            var collection = db.GetCollection<BotModel>("botmodel");
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.Authorizations);
            return await query.SingleAsync();
         }

            public async Task<List<BotConnection>> GetBotConnectivity(string name)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.botconnections);
            return await query.SingleAsync();
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

        public async Task<List<UserUsage>> GetBotUsage(string botModelName, string appId)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == botModelName)
                .Select(a => a.botconnections).FirstOrDefault().Where(b => b.AppId == appId)
                .Select(c => c.UsageHistory);
            return query.FirstOrDefault().ToList();
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

        public async Task<Contact> GetContactByEmail(string email)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.Email.ToLower() == email.ToLower());
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.LastName.ToLower() == lastName.ToLower());
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

        public async Task<AzureCredentials> UpdateAzureCredentials(string botModelName, string apiKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var ac = new AzureCredentials { AzureAPIKey = apiKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.azureCred", ac);
            await collection.FindOneAndUpdateAsync(filter, update);
            return ac;
        }

        public Task<BotInputFormat> UpdateBotModelInputFormat(string botModelName, string inputName, InputFormatUpdate inputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<BotOutputFormat> UpdateBotModelOutputFormat(string botModelName, string outputName, BotOutputFormatUpdate outputUpdate)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            var collection = db.GetCollection<Contact>("contact");
            var filter = Builders<Contact>.Filter.Where(x => x.Id == contact.Id );
            await collection.ReplaceOneAsync(filter, contact);
            return contact;
        }

        public async Task<InputFormat> UpdateRuleFormInputFormat(string name, string inputName, InputFormatUpdate inputUpdate)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == name && x.userId == userId && x.Contents.format.InputFormatList.Any(i => i.Name == inputName));
            var update = Builders<RuleSet>.Update.Combine(
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).EnforceCrisp, inputUpdate.EnforceCrisp),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).Increment, inputUpdate.Increment),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).MaxLength, inputUpdate.MaxLength),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).NumericMax, inputUpdate.NumericMax),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).NumericMin, inputUpdate.NumericMin),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).path, inputUpdate.path),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).Regex, inputUpdate.Regex),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList.ElementAt(-1).ShowSets, inputUpdate.ShowSets)
                );
            await collection.FindOneAndUpdateAsync(filter, update);
            return new InputFormat { };
        }

        public async Task<LanguageText> UpdateRuleFormLanguageText(string ruleSetName, string languageName, string languageText)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.Any(i => i.Name == languageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList.ElementAt(-1).Text, languageText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new LanguageText { Name = languageName, Text = languageText };
        }

        public async Task<OutputFormat> UpdateRuleFormOutputFormat(string ruleSetName, string outputName, OutputFormatUpdate outputUpdate)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.format.OutputFormatList.Any(i => i.Name == outputName));
            var update = Builders<RuleSet>.Update.Combine(
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).displayType.ToString(), outputUpdate.displayType.ToString()),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).Hide, outputUpdate.Hide),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).path, outputUpdate.path),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).ScoreBarColor, outputUpdate.ScoreBarColor),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).ScoreBarMaxVal, outputUpdate.ScoreBarMaxVal),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).ScoreBarMinVal, outputUpdate.ScoreBarMinVal),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).Uncertainty, outputUpdate.Uncertainty),
                Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList.ElementAt(-1).ValueFormat, outputUpdate.ValueFormat)
                );
            await collection.FindOneAndUpdateAsync(filter, update);
            return new OutputFormat { };
        }

        public async Task<VariantText> UpdateRuleFormVariantText(string ruleSetName, string languageName, string isoLanguageName, string variantText)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.First(i => i.Name == languageName).VariantList.Any( a => a.Language == isoLanguageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList.ElementAt(-1).VariantList.ElementAt(-1).Text, variantText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new VariantText { Language = isoLanguageName, Text = variantText };
        }

        public async Task<SellerCenterCredentials> UpdateSellereCenterCredentials(string botModelName, bool liveMode, string merchantId, string stripeApiKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var scc = new SellerCenterCredentials {  LiveMode = liveMode, MerchantId = merchantId, StripeApiKey = stripeApiKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.sellerCred", scc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return scc;
        }

        public async Task<SendGridCredentials> UpdateSendgridCredentials(string botModelName, string sendGridAPIKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var sgc = new SendGridCredentials {  SendGridAPIKey = sendGridAPIKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.sendgridCred", sgc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return sgc;
        }

        public async Task<TwilioCredentials> UpdateTwilioCredentials(string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var tc = new TwilioCredentials { SMSAccountFrom = sMSAccountFrom, SMSAccountIdentification = sMSAccountIdentification, SMSAccountPassword = sMSAccountPassword };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.twilioCred", tc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return tc;
        }

        public async Task<ZendeskCredentials> UpdateZendeskCredentials(string botModelName, string zendeskApiKey, string zendeskURL, string zendeskUser)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var zc = new ZendeskCredentials { ZendeskApiKey = zendeskApiKey, ZendeskURL = zendeskURL, ZendeskUser = zendeskUser};
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.zendeskCred", zc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return zc;
        }

        public Task<object> CreateDefaultModel(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => string.Equals(p.InvoiceEmail, email, StringComparison.OrdinalIgnoreCase));
            return await query.ToListAsync();
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.userId == id);
            return await query.FirstOrDefaultAsync();
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

        public async Task<Default> UpdateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>("default");
            var model = new Default { Name = name, Value = value };
            var filter = Builders<Default>.Filter.Where(x => x.Name == name);
            var update = Builders<Default>.Update.Set("Value", value);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Default,Default> {  IsUpsert = false });
            return model;
        }
    }
}
