using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using DarlLanguage;
using DarlLanguage.Processing;
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

        public async Task<Contact> DeleteContactAsync(string email)
        {
            try
            {
                var mc = db.GetCollection<Contact>("contact");
                var query = mc.AsQueryable().Where(p => p.Email == email);
                var old = await query.FirstOrDefaultAsync();
                DeleteResult res =  await mc.DeleteOneAsync<Contact>(r => r.Email == email);
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

        public async Task<MLModel> DeleteMLModel(string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<MLModel>.Filter.Eq(r => r.userId, userId) & Builders<MLModel>.Filter.Eq(r => r.Name, name));
            return old;
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
            var filter = Builders<Contact>.Filter.Where(x => x.Email == contact.Email );
            var updList = new List<UpdateDefinition<Contact>>();
            if (contact.Company != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Company, contact.Company));
            if (contact.Country != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Country, contact.Country));
            if (contact.FirstName != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.FirstName, contact.FirstName));
            if (contact.IntroSent)
                updList.Add(Builders<Contact>.Update.Set(x => x.IntroSent, contact.IntroSent));
            if (contact.LastName != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.LastName, contact.LastName));
            if (contact.Notes != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Notes, contact.Notes));
            if (contact.Phone != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Phone, contact.Phone));
            if (contact.Sector != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Sector, contact.Sector));
            if (contact.Source != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Source, contact.Source));
            if (contact.Title != null)
                updList.Add(Builders<Contact>.Update.Set(x => x.Title, contact.Title));
            var update = Builders<Contact>.Update.Combine(updList);
            await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Contact, Contact> { IsUpsert = false });
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

        public async Task<SellerCenterCredentials> UpdateSellerCenterCredentials(string botModelName, bool liveMode, string merchantId, string stripeApiKey)
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
            .Where(p => p.InvoiceEmail.ToLower() == email.ToLower());
            return await query.ToListAsync();
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.userId == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<DarlUser> CreateUserAsync(DarlUserInput user)
        {
            try
            {
                var mc = db.GetCollection<DarlUser>("user");
                var duser = new DarlUser { Created = user.Created, current_period_end = user.current_period_end, InvoiceEmail = user.InvoiceEmail, InvoiceName = user.InvoiceName, InvoiceOrganization = user.InvoiceOrganization, Issuer = user.Issuer, PaidUsageStarted = user.PaidUsageStarted, StripeCustomerId = user.StripeCustomerId, UsageStripeSubscriptionItem = user.UsageStripeSubscriptionItem, userId = user.userId  };
                await mc.InsertOneAsync(duser);
                return duser;
            }
            catch (Exception ex)
            {
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate user)
        {
            var collection = db.GetCollection<DarlUser>("user");
            var filter = Builders<DarlUser>.Filter.Where(x => x.userId == userId);
            var updList = new List<UpdateDefinition<DarlUser>>();
            if (user.accountState != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.accountState, user.accountState));
            if (user.current_period_end != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.current_period_end, user.current_period_end));
            if (user.InvoiceEmail != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.InvoiceEmail, user.InvoiceEmail));
            if (user.InvoiceName != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.InvoiceName, user.InvoiceName));
            if (user.InvoiceEmail != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.InvoiceEmail, user.InvoiceEmail));
            if (user.InvoiceName != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.InvoiceName, user.InvoiceName));
            if (user.InvoiceOrganization != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.InvoiceOrganization, user.InvoiceOrganization));
            if (user.PaidUsageStarted != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.PaidUsageStarted, user.PaidUsageStarted));
            if (user.StripeCustomerId != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.StripeCustomerId, user.StripeCustomerId));
            if (user.UsageStripeSubscriptionItem != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.UsageStripeSubscriptionItem, user.UsageStripeSubscriptionItem));
            var update = Builders<DarlUser>.Update.Combine(updList);
            var newUser = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<DarlUser, DarlUser> { IsUpsert = false });
            return newUser;
        }

        public async Task<DarlUser> DeleteUser(string userId)
        {
            try
            {
                var mc = db.GetCollection<DarlUser>("user");
                var query = mc.AsQueryable().Where(p => p.userId == userId);
                var old = await query.FirstOrDefaultAsync();
                DeleteResult res = await mc.DeleteOneAsync<DarlUser>(r => r.userId == userId);
                return old;
            }
            catch (Exception ex)
            {
                throw new ExecutionError("Duplicate or malformed data");
            }
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

        /// <summary>
        /// run machine learning model. 
        /// </summary>
        /// <param name="mlmodelname"></param>
        /// <returns></returns>
        public async Task<MLModel> MachineLearnModel(string mlmodelname)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable()
            .Where(p => p.Name == mlmodelname && p.userId == userId);
            var model = await query.FirstOrDefaultAsync();
            var runtime = new DarlRunTime();
            var rep = new DarlMineReport();
            var start = DateTime.Now;
            try
            {
                var newcode = runtime.MineSupervised(model.model.darl, model.model.trainData, model.model.sets, 100 - model.model.percentTest, rep);
            }
            catch (Exception ex)
            {
                rep.errorText = $"Error in running machine learning task: {ex.ToString()}";
            }
            var end = DateTime.Now;
            //insert result into MLModel result array
            var mlr = new MLResult { code = rep.code, errorText = rep.errorText, executionDate = start, executionTime = (end - start),  trainPercent = rep.trainPercent, trainPerformance = rep.trainPerformance, testPerformance = rep.testPerformance, unknownResponsePercent = rep.unknownResponsePercent};
            var filter = Builders<MLModel>.Filter.Where(x => x.Name == mlmodelname && x.userId == userId);
            var update = Builders<MLModel>.Update.AddToSet("results", mlr);
            var options = new FindOneAndUpdateOptions<MLModel, MLModel> { IsUpsert = false, ReturnDocument = ReturnDocument.After };
            return await mc.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<MLModel> UpdateMLSpec(string name, MLSpecUpdate mlspec)
        {
            var collection = db.GetCollection<MLModel>("mlmodel");
            var filter = Builders<MLModel>.Filter.Where(x => x.Name == name && x.userId == userId);
            var updList = new List<UpdateDefinition<MLModel>>();
            if (mlspec.author != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.author, mlspec.author));
            if (mlspec.copyright != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.copyright, mlspec.copyright));
            if (mlspec.darl != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.darl, mlspec.darl));
            if (mlspec.dataSchema != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.dataSchema, mlspec.dataSchema));
            if (mlspec.description != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.description, mlspec.description));
            if (mlspec.destinationRulesetName != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.destinationRulesetName, mlspec.destinationRulesetName));
            if (mlspec.license != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.license, mlspec.license));
            if (mlspec.percentTest != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.percentTest, mlspec.percentTest));
            if (mlspec.sets != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.sets, mlspec.sets));
            if (mlspec.trainData != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.trainData, mlspec.trainData));
            if (mlspec.version != null)
                updList.Add(Builders<MLModel>.Update.Set(x => x.model.version, mlspec.version));
            var update = Builders<MLModel>.Update.Combine(updList);
            var newUser = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<MLModel, MLModel> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return newUser;
        }

        public Task<QuestionSetProxy> BeginQuestionnaire(string ruleSetName)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionSetProxy> ContinueQuestionnaire(QuestionSetInput responses)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken)
        {
            throw new NotImplementedException();
        }
    }
}
