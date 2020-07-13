using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Forms;
using DarlCommon;
using DarlCompiler;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using VSTS.Net;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Types;
using Default = Darl.GraphQL.Models.Models.Default;
using MLModel = Darl.GraphQL.Models.Models.MLModel;
using Newtonsoft.Json.Linq;
using QuickGraph.Serialization;
using QuickGraph.Algorithms.Search;
using Darl.GraphQL.Process.Middleware;
using QuickGraph.Algorithms;
using Darl.Thinkbase;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.Options;

namespace Darl.GraphQL.Models.Connectivity
{
    public class CosmosDBConnectivity : IConnectivity
    {
        private IConfiguration _config;
        private ILicensing _licensing;
        public IMongoDatabase db { get; set; }
        private MongoClient mongoClient;
        private DarlRunTime runtime = new DarlRunTime();
        private ILogger _logger;
        private string backgroundUserId;

        public static readonly string botModelCollection = "botmodel";
        public static readonly string mlModelCollection = "mlmodel";
        public static readonly string botConnectionCollection = "botconnection";
        public static readonly string botStateCollection = "botstate";
        public static readonly string defaultResponseCollection = "defaultresponse";
        public static readonly string rulesetCollection = "ruleset";
        public static readonly string contactCollection = "contact";
        public static readonly string userCollection = "user";
        public static readonly string defaultCollection = "default";
        public static readonly string collateralCollection = "collateral";
        public static readonly string conversationCollection = "conversation";
        public static readonly string updateCollection = "update";
        public static readonly string documentCollection = "document";


        public CosmosDBConnectivity(IConfiguration config, ILogger<CosmosDBConnectivity> logger, ILicensing licensing)
        {
            _config = config;
            _logger = logger;
            _licensing = licensing;
            string connectionString = _config["AppSettings:MongoConnectionString"];
            backgroundUserId = _config["AppSettings:boaiuserid"];
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            mongoClient = new MongoClient(settings);
            db = mongoClient.GetDatabase(_config["AppSettings:MongoDatabase"]);
            BsonClassMap.RegisterClassMap<BotTrigger>();
            BsonClassMap.RegisterClassMap<DarlVar>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.categories).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, double>>(DictionaryRepresentation.ArrayOfDocuments));
            });

        }

        public async Task<Authorization> CreateAuthorization(string userId, string botModelName, Authorization auth)
        {
            var collection = db.GetCollection<BotModel>(botModelCollection);
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Push("Authorizations", auth);
            await collection.FindOneAndUpdateAsync(filter, update);
            return auth;
        }

        public async Task<BotConnection> CreateBotConnection(string userId, string botModelName, string appId, string password)
        {
            var bm = await GetBotModel(userId, botModelName);
            if(bm != null)
            { 
                //create bot connection
                var bc = new BotConnection { AppId = appId, FriendlyName = botModelName, Password = password, UsageHistory = new List<UserUsage>(), botModel = bm.id, userId = userId };
                var collection = db.GetCollection<BotConnection>(botConnectionCollection);
                await collection.InsertOneAsync(bc);
                //bc is updated with id during insert
                //Add to BotModel
                var bmcollection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Push("botconnections", bc.id);
                await bmcollection.FindOneAndUpdateAsync(filter, update);
                return bc;
            }
            else
            {
                throw new ExecutionError($"{botModelName} doesn't exist in account {userId}");
            }
        }

        public async Task<BotModel> CreateBotModel(string userId, string name, byte[] lm)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var model = new BotModel { Name = name, userId = userId, Authorizations = new List<Authorization>(), serviceConnectivity = new ServiceConnectivity(), Model = lm, botconnections = new List<MongoDB.Bson.ObjectId>() };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            try
            {
                contact.Id = Guid.NewGuid().ToString();
                var mc = db.GetCollection<Contact>(contactCollection);
                await mc.InsertOneAsync(contact);
                return contact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreateContactAsync));
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<Default> CreateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>(defaultCollection);
            var model = new Default { Name = name, Value = value };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<BotModel> CreateDefaultModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var query = mc.AsQueryable().Where(p => p.userId == userId);
            var existing = await query.ToListAsync();
            if (existing.Any(a => a.Name == name))
            {
                throw new ExecutionError($"A bot model with name {name} already exists in your account.");
            }
            var bm = await GetBotModel(backgroundUserId, _config["AppSettings:ProvisionBotModel"]);
            var botModel = new BotModel { Name = name, userId = userId, Model = bm.Model };
            await mc.InsertOneAsync(botModel);
            return botModel;
        }

        public async Task<Models.MLModel> CreateEmptyMLModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var query = mc.AsQueryable().Where(p => p.userId == userId);
            var existing = await query.ToListAsync();
            if (existing.Any(a => a.Name == name))
            {
                throw new ExecutionError($"An ml model with name {name} already exists in your account.");
            }
            var model = new MLModel { Name = name, model = new DarlCommon.MLModel { name = name, percentTest = 0, sets = 3, darl = "ruleset newRuleSet supervised\n{\n}\n" }, results = new List<MLResult>(), userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<BotModel> CreateEmptyModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var query = mc.AsQueryable().Where(p => p.userId == userId);
            var existing = await query.ToListAsync();
            if (existing.Any(a => a.Name == name))
            {
                throw new ExecutionError($"A bot model with name {name} already exists in your account.");
            }
            var lm = new LineageModel();
            lm.ruleSkeleton = "ruleset botRuleset\n{\n /*%% rule_insertion_point %%*/\n}";
            lm.modelSettings.Add("name", $"{{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"{name}\"}}");
            lm.modelSettings.Add("copyright", "{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}");
            lm.modelSettings.Add("version", "{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}");
            lm.form = "{\"InputFormatList\": [], \"OutputFormatList\": [{\"Categories\": null,\"Sets\": null,\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\",\"ValueFormat\": null},{\"Categories\": null,\"Sets\": null,\"Name\": \"link\",\"OutputType\": \"textual\",\"displayType\": \"Link\",\"ValueFormat\": null}],\"Stores\": [\"UserData\",\"ConversationData\",\"PrivateConversationData\",\"Bot\",\"Value\",\"Call\",\"Word\",\"Rest\",\"Collateral\",\"Graph\"],\"Strings\": {}, \"Constants\": {}, \"Sequences\": {}}";
            lm.tree = new LineageMatchTree();
            lm.PhraseCreate("default:");
            lm.tree.SaveAttributes("default:", "if anything then response will be \"I don't know the answer to that\";", new List<string>(), new List<string>());
            var model = new BotModel { Name = name, userId = userId, Model = ConvertLineageModel(lm) };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<RuleSet> CreateEmptyRuleSet(string userId, string name)
        {
            var mc = db.GetCollection<RuleSet>(rulesetCollection);
            var query = mc.AsQueryable().Where(p => p.userId == userId);
            var existing = await query.ToListAsync();
            if(existing.Any( a => a.Name == name))
            {
                throw new ExecutionError($"A ruleset with name {name} already exists in your account.");
            }
            var model = new RuleSet { Name = name, userId = userId, Contents = new RuleForm { darl = "ruleset myRuleSet\n{\n}\n", trigger = new TriggerView()  } };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<LineageNodeDefinition> CreateLineageNode(string userId, string botModelName, string parent, string newName)
        {
            var currentModel = await GetLineageModel(userId, botModelName);
            var lmn = currentModel.tree.Add(parent, newName.Trim().ToLower());
            if (lmn != null)
            {
                await SaveLineageModel(userId, botModelName, currentModel);
                var att = new LineageNodeAttributes();
                att.definition = lmn.element.description;
                return new LineageNodeDefinition { text = lmn.element.lineage, id = lmn.element.lineage, attributes = att, children = false };
            }
            return new LineageNodeDefinition { children = false };
        }

        public async Task<MLModel> CreateMLModel(string userId, string name, DarlCommon.MLModel model)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var mm = new MLModel { Name = name, userId = userId, model = model };
            await mc.InsertOneAsync(mm);
            return mm;
        }

        public async Task<LineageNodeDefinition> CreatePhrase(string userId, string botModelName, string path, LineageNodeAttributes attribute)
        {
            var currentModel = await GetLineageModel(userId, botModelName);
            var lmn = currentModel.PhraseCreate(path);
            //Also save attribute at the same time
            var newCode = currentModel.ReconcileCode(attribute.darl, new BotFragment { CallRuleset = attribute.call, RandomResponses = attribute.randomResponses, Response = attribute.response }, path);
            currentModel.tree.SaveAttributes(path, newCode.Trim(), new List<string>(), attribute.accessRoles);
            await SaveLineageModel(userId, botModelName, currentModel);
            var att = new LineageNodeAttributes();
            att.definition = lmn.element.description;
            return new LineageNodeDefinition { text = lmn.element.lineage, id = lmn.element.lineage,  attributes = att, children = false };
        }

        public async Task<RuleForm> CreateRuleFormFromDarl(string userId, string name, string darl)
        {
            var rs = await GetRuleSet(userId, name);
            if (rs != null)
            {
                rs.Contents.darl = darl;
                if (string.IsNullOrEmpty(rs.Contents.name))
                    rs.Contents.name = name;
                try
                { 
                    var errors = await rs.Contents.UpdateFromCode();
                    if (errors.Count == 0)
                    {
                        var rc = db.GetCollection<RuleSet>(rulesetCollection);
                        await rc.UpdateOneAsync(Builders<RuleSet>.Filter.Where(x => x.Name == name && x.userId == userId),
                            Builders<RuleSet>.Update.Set(x => x.Contents, rs.Contents));
                        return rs.Contents;
                    }
                    else
                    {
                        int errorCount = 1;
                        var dict = new Dictionary<string, object>();
                        foreach (var err in errors)
                        {
                            dict.Add($"Error{errorCount++}", err);
                        }
                        throw new ExecutionError("Errors in updating I/O.", dict);
                    }
                }
                catch(Exception ex)
                {
                    throw new ExecutionError("Errors in Darl source", ex);
                }
            }
            return null;
        }

        public async Task<RuleSet> CreateRuleSet(string userId, string name, RuleForm rf, ServiceConnectivity sc)
        {
            var mc = db.GetCollection<RuleSet>(rulesetCollection);
            var model = new RuleSet { Name = name, userId = userId, Contents = rf, serviceConnectivity = sc };
            await mc.InsertOneAsync(model);
            return model;
        }
        public async Task<StringDoublePair> CreateUpdateConstant(string userId, string botModelName, string name, double value)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            StringDoublePair res = new StringDoublePair { name = name, value = form.Constants[name] };
            if (form != null)
            {
                if (form.Constants.ContainsKey(name))
                {
                    form.Constants[name] = value;
                }
                else
                {
                    form.Constants.Add(name, value);
                }
                await SaveBotFormat(userId, botModelName, form);
                return res;
            }
            return null;
        }
        public async Task<string> CreateUpdateStore(string userId, string botModelName, string name)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            if (form != null)
            {
                if (!form.Stores.Contains(name))
                {
                    form.Stores.Add(name);
                }
                await SaveBotFormat(userId, botModelName, form);
                return name;
            }
            return null;
        }

        public async Task<StringStringPair> CreateUpdateString(string userId, string botModelName, string name, string value)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            StringStringPair res = new StringStringPair(name, form.Strings[name]);
            if (form != null)
            {
                if (form.Strings.ContainsKey(name))
                {
                    form.Strings[name] = value;
                }
                else
                {
                    form.Strings.Add(name, value);
                }
                await SaveBotFormat(userId, botModelName, form);
                return res;
            }
            return null;
        }

        public async Task<DarlUser> CreateUserAsync(DarlUserInput user)
        {
            try
            {
                var mc = db.GetCollection<DarlUser>(userCollection);
                var duser = new DarlUser { Created = user.Created, current_period_end = user.current_period_end, InvoiceEmail = user.InvoiceEmail, InvoiceName = user.InvoiceName, InvoiceOrganization = user.InvoiceOrganization, Issuer = user.Issuer, PaidUsageStarted = user.PaidUsageStarted, StripeCustomerId = user.StripeCustomerId, UsageStripeSubscriptionItem = user.UsageStripeSubscriptionItem, userId = user.userId };
                await mc.InsertOneAsync(duser);
                return duser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreateUserAsync));
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<string> DeleteAuthorization(string userId, string name, string name1)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == name && x.userId == userId);
            var update = Builders<BotModel>.Update.PullFilter(p => p.Authorizations, f => f.name == name1);
            await mc.UpdateOneAsync(filter, update);
            return name1;
        }

        public async Task<AzureCredentials> DeleteAzureCredentials(string userId, string botModelName, ModelType modelType)
        {
            if(modelType == ModelType.botmodel)
            { 
                var mc = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.azurecred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            if(modelType == ModelType.ruleset)
            {
                var mc = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set(p => p.serviceConnectivity.azurecred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            return null;
        }

        public async Task<BotConnection> DeleteBotConnection(string userId, string botModelName, string appId)
        {
            //delete connection
            var bc = db.GetCollection<BotConnection>(botConnectionCollection);
            var query = bc.AsQueryable().Where(p => p.AppId == appId);
            var old = await query.FirstOrDefaultAsync();
            var bcfilter = Builders<BotConnection>.Filter.Where(x => x.AppId == appId);
            await bc.DeleteOneAsync(bcfilter);
            //delete reference
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.PullFilter(p => p.botconnections, f => f == old.id);
            await mc.UpdateOneAsync(filter, update);
            return old;
        }

        public async Task<BotModel> DeleteBotModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var query = mc.AsQueryable().Where(p => p.userId == userId && p.Name == name);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<BotModel>.Filter.Eq(r => r.userId, userId) & Builders<BotModel>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<StringDoublePair> DeleteConstant(string userId, string botModelName, string name)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            StringDoublePair res = null;
            if (form != null)
            {
                if (form.Constants.ContainsKey(name))
                {
                    res = new StringDoublePair { name = name, value = form.Constants[name] };
                    form.Constants.Remove(name);
                }
                await SaveBotFormat(userId, botModelName, form);
                return res;
            }
            return null;
        }

        public async Task<Contact> DeleteContactAsync(string email)
        {
            try
            {
                var mc = db.GetCollection<Contact>(contactCollection);
                var query = mc.AsQueryable().Where(p => p.Email == email);
                var old = await query.FirstOrDefaultAsync();
                DeleteResult res = await mc.DeleteOneAsync<Contact>(r => r.Email == email);
                return old;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(DeleteContactAsync));
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<Default> DeleteDefault(string name)
        {
            var mc = db.GetCollection<Default>(defaultCollection);
            var query = mc.AsQueryable().Where(p => p.Name == name);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Default>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<LineageNodeDefinition> DeleteLineageNode(string userId, string botModelName, string id)
        {
            var lm = await GetLineageModel(userId, botModelName);
            lm.tree.Delete(id);
            await SaveLineageModel(userId, botModelName, lm);
            return new LineageNodeDefinition();
        }

        public async Task<MLModel> DeleteMLModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<MLModel>.Filter.Eq(r => r.userId, userId) & Builders<MLModel>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<LineageNodeDefinition> DeletePhrase(string userId, string botModelName, string phrase)
        {
            var lm = await GetLineageModel(userId, botModelName);
            lm.PhraseDelete(phrase); 
            await SaveLineageModel(userId, botModelName, lm);
            return new LineageNodeDefinition();
        }

        public async Task<RuleSet> DeleteRuleSet(string userId, string name)
        {
            RuleSet old = null;
            var mc = db.GetCollection<RuleSet>(rulesetCollection);
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<RuleSet>.Filter.Eq(r => r.userId, userId) & Builders<RuleSet>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<SellerCenterCredentials> DeleteSellereCenterCredentials(string userId, string botModelName, ModelType modelType)
        {
            if(modelType == ModelType.botmodel)
            { 
                var mc = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.sellercred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            else if (modelType == ModelType.ruleset)
            {
                var mc = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set(p => p.serviceConnectivity.sellercred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            return null;
        }

        public async Task<SendGridCredentials> DeleteSendgridCredentials(string userId, string botModelName, ModelType modelType)
        {
            if (modelType == ModelType.botmodel)
            {
                var mc = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.sendgridcred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            else if (modelType == ModelType.ruleset)
            {
                var mc = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set(p => p.serviceConnectivity.sendgridcred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            return null;
        }

        public async Task<string> DeleteStore(string userId, string botModelName, string name)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            if (form != null)
            {
                if (form.Stores.Contains(name))
                {
                    form.Stores.Remove(name);
                }
                await SaveBotFormat(userId, botModelName, form);
                return name;
            }
            return null;
        }

        public async Task<StringStringPair> DeleteString(string userId, string botModelName, string name)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            StringStringPair res = null;
            if (form != null)
            {
                if (form.Constants.ContainsKey(name))
                {
                    res = new StringStringPair(name, form.Strings[name]);
                    form.Strings.Remove(name);
                }
                await SaveBotFormat(userId, botModelName, form);
                return res;
            }
            return null;
        }

        public async Task<TwilioCredentials> DeleteTwilioCredentials(string userId, string botModelName, ModelType modelType)
        {
            if (modelType == ModelType.botmodel)
            {
                var mc = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.twiliocred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            else if (modelType == ModelType.ruleset)
            {
                var mc = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set(p => p.serviceConnectivity.twiliocred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            return null;
        }

        public async Task<DarlUser> DeleteUser(string userId)
        {
            try
            {
                var mc = db.GetCollection<DarlUser>(userCollection);
                var query = mc.AsQueryable().Where(p => p.userId == userId);
                var old = await query.FirstOrDefaultAsync();
                DeleteResult res = await mc.DeleteOneAsync<DarlUser>(r => r.userId == userId);
                return old;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(DeleteUser));
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<LineageNodeAttributes> GetAttribute(string userId, string botModelName, string phrase)
        {
            if (!string.IsNullOrEmpty(phrase))
            {
                var currentModel = await GetLineageModel(userId, botModelName);
                var results = currentModel.Match(phrase, new List<DarlVar>());
                var att = results.Last();
                if (att.path.Contains("default:")) //no actual match
                {
                    var path = currentModel.BestMatch(phrase);
                    return new LineageNodeAttributes { path = path};
                }
                var code = string.Join("\n", att.annotation.darl);
                var bf = currentModel.BotFragmentBuilder(code);
                return new LineageNodeAttributes
                {
                    accessRoles = att.annotation.accessRoles,
                    call = bf.CallRuleset,
                    darl = code,
                    path = att.path,
                    implications = att.annotation.implications,
                    present = true,
                    randomResponse = bf.RandomResponses.Any(),
                    randomResponses = bf.RandomResponses,
                    response = bf.Response
                };
            }
            return new LineageNodeAttributes();
        }

        public async Task<LineageNodeAttributes> GetAttributeFromPath(string userId, string botModelName, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var currentModel = await GetLineageModel(userId, botModelName);
                var results = currentModel.tree.Navigate(path);
                if (results.Count > 0)
                {
                    var att = results.Last();
                    var code = string.Join("\n", att.annotation.darl);
                    var bf = currentModel.BotFragmentBuilder(code);
                    return new LineageNodeAttributes
                    {
                        accessRoles = att.annotation.accessRoles,
                        call = bf.CallRuleset,
                        darl = code,
                        implications = att.annotation.implications,
                        //present = att.present,
                        randomResponse = bf.RandomResponses.Any(),
                        randomResponses = bf.RandomResponses,
                        response = bf.Response
                    };
                }
            }
            return new LineageNodeAttributes();
        }

        public async Task<List<Authorization>> GetAuthorizations(string userId, string name)
        {
            var collection = db.GetCollection<BotModel>(botModelCollection);
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.Authorizations);
            return await query.SingleAsync();
        }

        public async Task<List<BotConnection>> GetBotConnectivity(string userId, string name)
        {
            var list = new List<BotConnection>();
            var collection = db.GetCollection<BotModel>(botModelCollection);
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.botconnections);
            var references = await query.SingleAsync();
            if (references.Count != 0)
            { 
                var bccollection = db.GetCollection<BotConnection>(botConnectionCollection);
                foreach(var r in references)
                {
                    var bcquery = bccollection.AsQueryable()
                        .Where(a => a.id == r);
                    list.Add(await bcquery.SingleAsync());
                }
            }
            return list;
        }

        public async Task<BotModel> GetBotModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == name);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<BotModel>> GetBotModelsAsync(string userId)
        {
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<List<UserUsage>> GetBotUsage(string appId)
        {
            var collection = db.GetCollection<BotConnection>(botConnectionCollection);
            var query = collection.AsQueryable()
                .Where(a => a.AppId == appId)
                .Select(c => c.UsageHistory);
            return query.FirstOrDefault().ToList();
        }

        /// <summary>
        /// Get nodes at a particular location 
        ///// </summary>
        /// <param name="botModelName"></param>
        /// <param name="path"></param>
        /// <param name="isRoot"></param>
        /// <returns></returns>
        public async Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string userId, string botModelName, string path, bool isRoot)
        {

            var currentModel = await GetLineageModel(userId, botModelName);
            if (isRoot)
            {
                var first = new List<LineageNodeDefinition>();

                foreach (var r in currentModel.tree.root.children.Values)
                {
                    AddLineageNodeDefinition(r, currentModel, first, r.element.lineage);
                }
                return first;
            }

            var next = new List<LineageNodeDefinition>();
            var children = currentModel.tree.Navigate(path);
            if (children.Any())
            {
                foreach (var r in children)
                {
                    AddLineageNodeDefinition(r, currentModel, next, $"{path}/{r.element.lineage}");
                }
            }
            else //update
            {
                var r = currentModel.tree.Find(path);
                if (r != null)
                {
                    AddLineageNodeDefinition(r, currentModel, next, path);
                }
            }
            return next;
        }

        private void AddLineageNodeDefinition(LineageMatchNode r, LineageModel lm, List<LineageNodeDefinition> next, string id)
        {

            var notleaf = r.children.Any();
            LineageNodeAttributes att = ConvertFromLineageMatchNode(r, lm);
            att.definition = r.element.description;
            next.Add(new LineageNodeDefinition
            {
                id = id,
                text = r.element.lineage,
                children = notleaf,
                attributes = att,
                icon = r.element.type == LineageType.Default ? "fa fa-stop-circle-o" : "fa fa-angle-right",
                type = r.element.type == LineageType.Default ? "nochild" : "default"
            });
        }

        private LineageNodeAttributes ConvertFromLineageMatchNode(LineageMatchNode r, LineageModel lm)
        {
            LineageNodeAttributes att = null;
            if (r.annotation != null)
            {
                var code = r.annotation != null ? string.Join("\n", r.annotation.darl) : "";
                var bf = new BotFragment();
                if (!string.IsNullOrEmpty(code.Trim()))
                {
                    bf = lm.BotFragmentBuilder(code);
                }
                att = new LineageNodeAttributes
                {
                    accessRoles = r.annotation.accessRoles,
                    call = bf.CallRuleset,
                    darl = code,
                    implications = r.annotation.implications,
                    //present = att.present,
                    randomResponse = bf.RandomResponses.Any(),
                    randomResponses = bf.RandomResponses,
                    response = bf.Response
                };
            }
            else
            {
                att = new LineageNodeAttributes();
            }
            return att;
        }

        public async Task<LineageModel> GetLineageModel(string userId, string botModelName)
        {
            var bm = await GetBotModel(userId, botModelName);
            LineageModel currentModel = null;
            using (var ms = new MemoryStream(bm.Model))
            {
                ms.Position = 0;
                currentModel = LineageModel.Load(ms);
            }
            return currentModel;
        }



        public async Task<Contact> GetContactByEmail(string email)
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Email.ToLower() == email.ToLower());
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Contact> GetContactById(string Id)
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Id == Id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Contact>> GetContacts()
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            var query = mc.AsQueryable(new AggregateOptions {  BatchSize = 10000});
            return await query.ToListAsync();
        }
        public async Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            var query = mc.AsQueryable()
            .Where(p => p.LastName.ToLower() == lastName.ToLower());
            return await query.ToListAsync();
        }

        public async Task<List<Default>> GetDefaults()
        {
            var mc = db.GetCollection<Default>(defaultCollection);
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<string> GetDefaultValue(string name)
        {
            var mc = db.GetCollection<Default>(defaultCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Name == name);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? string.Empty : def.Value;
        }

        public async Task<List<DarlVar>> GetExampleInputs(string userId, string ruleSetName)
        {
            if (!string.IsNullOrEmpty(ruleSetName))
            {
                var sm = await GetRuleSet(userId, ruleSetName);
                if (sm != null)
                {
                    var tree = runtime.CreateTreeEdit(sm.Contents.darl);
                    if (tree.HasErrors())
                    {
                        return null; //errors, just add them to the input and quit.
                    }
                    return GetInputs(tree);
                }
            }
            return null;
        }

        public async Task<List<LineageRecord>> GetLineagesForWord(string word, string isoLanguage = "en")
        {
            try
            {
                var offset = 0;
                return LineageLibrary.WordRecognizer(new List<string> { word }, ref offset, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bad lineage lookup for word {word} message: {ex.Message}");
                return new List<LineageRecord>();
            }
        }

        public async Task<string> GetTypeWordForLineage(string lineage, string isoLanguage = "en")
        {
            try
            {
                if(LineageLibrary.lineages.ContainsKey(lineage))
                    return LineageLibrary.lineages[lineage].typeWord;
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bad lineage lookup for lineage {lineage} message: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<Models.MLModel> GetMlModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Models.MLModel>> GetMlModelsAsync(string userId)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<RuleSet> GetRuleSet(string userId, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ExecutionError("Name is empty in GetRuleSet");
            var mc = db.GetCollection<RuleSet>(rulesetCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<RuleSet>> GetRuleSetsAsync(string userId)
        {
            var mc = db.GetCollection<RuleSet>(rulesetCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            try { 
            var mc = db.GetCollection<DarlUser>(userCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == id);
            return await query.FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Internal error in GetUserById {id}");
                throw new ExecutionError("Internal error in GetUserById");
            }
        }

        public async Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            var mc = db.GetCollection<DarlUser>(userCollection);
            var query = mc.AsQueryable()
            .Where(p => p.InvoiceEmail.ToLower() == email.ToLower());
            return await query.ToListAsync();
        }

        public async Task<List<DarlVar>> InferFromRuleSetDarlVar(string userId, string ruleSetName, List<DarlVarInput> inputs)
        {
            try
            {
                if (!string.IsNullOrEmpty(ruleSetName))
                {
                    var rs = await GetRuleSet(userId, ruleSetName);
                    if (rs != null)
                    {
                        var tree = runtime.CreateTreeEdit(rs.Contents.darl);
                        if (tree.HasErrors())
                        {
                            var errors = new List<DarlVar>();
                            int errorCount = 0;
                            foreach (var pm in tree.ParserMessages)
                            {
                                var level = pm.Level == ErrorLevel.Error ? "error" : "warning";
                                errors.Add(new DarlVar { name = $"error{errorCount++}", Value = $"line_no = {pm.Location.Line + 1}, column_no_start = {pm.Location.Column + 1}, column_no_stop = {pm.Location.Column + 2}, message = {pm.Message}, severity = {level}", dataType = DarlVar.DataType.textual });
                            }
                            _logger.LogError("DarlInf used with errors");
                            return errors; //errors, just add them to the input and quit.
                        }
                        var res = await ProcessValues(DarlVarExtensions.Convert(DarlVarInput.Convert(inputs)), tree);
                        _logger.LogWarning($"{nameof(InferFromRuleSetDarlVar)}: {userId}, {ruleSetName}");
                        return DarlVarExtensions.Convert(res);
                    }
                    else
                    {
                        return new List<DarlVar> { new DarlVar { name = "error", Value = $"RuleSet {ruleSetName} does not exist." } };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DarlInf Exception");
                var errors = new List<DarlVar>();
                errors.Add(new DarlVar { name = "error", Value = ex.Message });
                return errors;
            }
            return null;
        }

        public async Task<List<DarlLintView>> LintDarl(string darl, string skeleton, string insertion)
        {
            var errorList = new List<DarlLintView>();
            int rowoffset = 0;
            int coloffset = 0;
            if (!string.IsNullOrEmpty(skeleton) && !string.IsNullOrEmpty(insertion))
            {
                int offset = skeleton.IndexOf(insertion);
                var start = skeleton.Substring(0, offset);
                var lines = start.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                rowoffset = lines.Length - 1;
                coloffset = lines.Last().Length;
                darl = skeleton.Replace(insertion, darl);
            }
            if (!string.IsNullOrEmpty(darl))
            {
                var tree = runtime.CreateTreeEdit(darl);
                if (tree.HasErrors())
                {
                    foreach (var pm in tree.ParserMessages)
                    {
                        errorList.Add(new DarlLintView { line_no = pm.Location.Line + 1 - rowoffset, column_no_start = pm.Location.Column + 1 - coloffset, column_no_stop = pm.Location.Column + 2 - coloffset, message = pm.Message, severity = pm.Level == ErrorLevel.Error ? "error" : "warning" });
                    }
                }
            }
            return errorList;
        }

        /// <summary>
        /// run machine learning model.
        /// </summary>
        /// <param name="mlmodelname"></param>
        /// <returns></returns>
        public async Task<MLModel> MachineLearnModel(string userId, string mlmodelname)
        {
            var mc = db.GetCollection<MLModel>(mlModelCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Name == mlmodelname && p.userId == userId);
            var model = await query.FirstOrDefaultAsync();
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
            TimeSpan execTime = (end - start);
            _logger.LogWarning($"{nameof(MachineLearnModel)}: {userId}, {mlmodelname}, {execTime.TotalSeconds.ToString()}");
            //insert result into MLModel result array
            var mlr = new MLResult { code = rep.code, errorText = rep.errorText, executionDate = start, executionTime = execTime, trainPercent = rep.trainPercent, trainPerformance = rep.trainPerformance, testPerformance = rep.testPerformance, unknownResponsePercent = rep.unknownResponsePercent };
            var filter = Builders<MLModel>.Filter.Where(x => x.Name == mlmodelname && x.userId == userId);
            var update = Builders<MLModel>.Update.AddToSet("results", mlr);
            var options = new FindOneAndUpdateOptions<MLModel, MLModel> { IsUpsert = false, ReturnDocument = ReturnDocument.After };
            return await mc.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<LineageNodeDefinition> PasteLineageNode(string userId, string botModelName, string parent, List<string> nodes, string mode)
        {
            var lm = await GetLineageModel(userId, botModelName);
            lm.tree.Paste(parent, nodes, mode);
            await SaveLineageModel(userId, botModelName, lm);
            return new LineageNodeDefinition();
        }

        public async Task<LineageNodeDefinition> RenameLineageNode(string userId, string botModelName, string id, string newName)
        {
            var lm = await GetLineageModel(userId, botModelName);
            lm.tree.Rename(id, newName.Trim().ToLower());
            await SaveLineageModel(userId, botModelName, lm);
            return new LineageNodeDefinition();
        }

        public async Task<LineageNodeAttributes> UpdateAttribute(string userId, string botModelName, string path, LineageNodeAttributeUpdate attribute)
        {
            var lm = await GetLineageModel(userId, botModelName);
            var newCode = lm.ReconcileCode(attribute.darl, new BotFragment { CallRuleset = attribute.call, RandomResponses = attribute.randomResponses, Response = attribute.response }, path);
            lm.tree.SaveAttributes(path, newCode.Trim(), new List<string>(), attribute.accessRoles );
            await SaveLineageModel(userId, botModelName, lm); 
            return new LineageNodeAttributes { accessRoles = attribute.accessRoles, call = attribute.call, darl = attribute.darl, path = path, present = true, randomResponse = attribute.randomResponse, randomResponses = attribute.randomResponses, response = attribute.response };
        }

        public async Task<AzureCredentials> UpdateAzureCredentials(string userId, string botModelName, string apiKey, ModelType modelType)
        {
            var ac = new AzureCredentials { AzureAPIKey = apiKey };
            if(modelType == ModelType.botmodel)
            {
                var collection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set("serviceConnectivity.azurecred", ac);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            if(modelType == ModelType.ruleset)
            {
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("serviceConnectivity.azurecred", ac);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return ac;
        }

        public async Task<BotInputFormat> UpdateBotModelInputFormat(string userId, string botModelName, string inputName, InputFormatUpdate inputUpdate)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            if (form != null)
            {
                BotInputFormat inp = form.InputFormatList.FirstOrDefault(b => b.Name == inputName);
                if (inp == null)
                {
                    inp = new BotInputFormat();
                    form.InputFormatList.Add(inp);
                }
                inp.EnforceCrisp = inputUpdate.EnforceCrisp ?? inp.EnforceCrisp;
                inp.Increment = inputUpdate.Increment ?? inp.Increment;
                inp.MaxLength = inputUpdate.MaxLength ?? inp.MaxLength;
                inp.NumericMax = inputUpdate.NumericMax ?? inp.NumericMax;
                inp.NumericMin = inputUpdate.NumericMin ?? inp.NumericMin;
                inp.Regex = inputUpdate.Regex ?? inp.Regex;
                inp.ShowSets = inputUpdate.ShowSets ?? inp.ShowSets;
                //Save format
                await SaveBotFormat(userId, botModelName, form);
                return inp;
            }
            return null;
        }

        /// <summary>
        /// Needed because the bot format is string encoded.
        /// </summary>
        /// <param name="botModelName"></param>
        /// <returns></returns>
        private async Task<BotFormat> GetBotFormat(string userId, string botModelName)
        {
            var lm = await GetLineageModel(userId, botModelName);
            return string.IsNullOrEmpty(lm.form) ? null : JsonConvert.DeserializeObject<BotFormat>(lm.form, new StringEnumConverter());
        }

        /// <summary>
        /// Save a bot format to the lineage model and thence to the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="botModelName"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        private async Task SaveBotFormat(string userId, string botModelName, BotFormat form)
        {
            var lm = await GetLineageModel(userId, botModelName);
            var formString = JsonConvert.SerializeObject(form, new StringEnumConverter());
            lm.form = formString;
            await SaveLineageModel(userId, botModelName, lm);
        }

        /// <summary>
        /// Save a lineage model to the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="botModelName"></param>
        /// <param name="lm"></param>
        /// <returns></returns>
        private async Task SaveLineageModel(string userId, string botModelName, LineageModel lm)
        {
            var binaryLM = ConvertLineageModel(lm);
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("Model", binaryLM);
            await mc.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Get the byte representation of a lineage model
        /// </summary>
        /// <param name="lm"></param>
        /// <returns></returns>
        private byte[] ConvertLineageModel(LineageModel lm)
        {
            byte[] result;
            using (MemoryStream ms = new MemoryStream())
            {
                lm.Store(ms);
                ms.Position = 0;
                result = ms.ToArray();
            }
            return result;
        }



        public async Task<BotOutputFormat> UpdateBotModelOutputFormat(string userId, string botModelName, string outputName, BotOutputFormatUpdate outputUpdate)
        {
            BotFormat form = await GetBotFormat(userId, botModelName);
            if (form != null)
            {
                BotOutputFormat outp = form.OutputFormatList.FirstOrDefault(b => b.Name == outputName);
                if (outp == null)
                {
                    outp = new BotOutputFormat();
                    form.OutputFormatList.Add(outp);
                }
                outp.displayType = outputUpdate.displayType ?? outp.displayType;
                outp.ValueFormat = outputUpdate.ValueFormat ?? outp.ValueFormat;
                await SaveBotFormat(userId, botModelName, form);
                return outp;
            }
            return null;
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            var collection = db.GetCollection<Contact>(contactCollection);
            var filter = Builders<Contact>.Filter.Where(x => x.Email == contact.Email);
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

        public async Task<Default> UpdateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>(defaultCollection);
            var model = new Default { Name = name, Value = value };
            var filter = Builders<Default>.Filter.Where(x => x.Name == name);
            var update = Builders<Default>.Update.Set("Value", value);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Default, Default> { IsUpsert = false });
            return model;
        }

        public async Task<MLModel> UpdateMLSpec(string userId, string name, MLSpecUpdate mlspec)
        {
            var collection = db.GetCollection<MLModel>(mlModelCollection);
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

        public async Task<InputFormat> UpdateRuleFormInputFormat(string userId, string name, string inputName, InputFormatUpdate inputUpdate)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == name && x.userId == userId && x.Contents.format.InputFormatList.Any(i => i.Name == inputName));
            var updList = new List<UpdateDefinition<RuleSet>>();
            if (inputUpdate.EnforceCrisp != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].EnforceCrisp, inputUpdate.EnforceCrisp));
            if (inputUpdate.Increment != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].Increment, inputUpdate.Increment));
            if (inputUpdate.MaxLength != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].MaxLength, inputUpdate.MaxLength));
            if (inputUpdate.NumericMax != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].NumericMax, inputUpdate.NumericMax));
            if (inputUpdate.NumericMin != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].NumericMin, inputUpdate.NumericMin));
            if (inputUpdate.path != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].path, inputUpdate.path));
            if (inputUpdate.Regex != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].Regex, inputUpdate.Regex));
            if (inputUpdate.ShowSets != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.InputFormatList[-1].ShowSets, inputUpdate.ShowSets));
            var update = Builders<RuleSet>.Update.Combine(updList);
            var rs = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return new InputFormat { };
        }

        public async Task<LanguageText> UpdateRuleFormLanguageText(string userId, string ruleSetName, string languageName, string languageText)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.Any(i => i.Name == languageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList[-1].Text, languageText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new LanguageText { Name = languageName, Text = languageText };
        }

        public async Task<OutputFormat> UpdateRuleFormOutputFormat(string userId, string ruleSetName, string outputName, OutputFormatUpdate outputUpdate)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.format.OutputFormatList.Any(i => i.Name == outputName));
            var updList = new List<UpdateDefinition<RuleSet>>();
            if (outputUpdate.displayType != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].displayType.ToString(), outputUpdate.displayType.ToString()));
            if (outputUpdate.Hide != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].Hide, outputUpdate.Hide));
            if (outputUpdate.path != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].path, outputUpdate.path));
            if (outputUpdate.ScoreBarColor != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].ScoreBarColor, outputUpdate.ScoreBarColor));
            if (outputUpdate.ScoreBarMaxVal != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].ScoreBarMaxVal, outputUpdate.ScoreBarMaxVal));
            if (outputUpdate.ScoreBarMinVal != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].ScoreBarMinVal, outputUpdate.ScoreBarMinVal));
            if (outputUpdate.Uncertainty != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].Uncertainty, outputUpdate.Uncertainty));
            if (outputUpdate.ValueFormat != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.format.OutputFormatList[-1].ValueFormat, outputUpdate.ValueFormat));
            var update = Builders<RuleSet>.Update.Combine(updList);
            var rs = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return new OutputFormat { };
        }

        public async Task<VariantText> UpdateRuleFormVariantText(string userId, string ruleSetName, string languageName, string isoLanguageName, string variantText)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.First(i => i.Name == languageName).VariantList.Any(a => a.Language == isoLanguageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList[-1].VariantList[-1].Text, variantText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new VariantText { Language = isoLanguageName, Text = variantText };
        }

        public async Task<SellerCenterCredentials> UpdateSellerCenterCredentials(string userId, string botModelName, bool liveMode, string merchantId, string stripeApiKey, ModelType modelType)
        {
            var scc = new SellerCenterCredentials { LiveMode = liveMode, MerchantId = merchantId, StripeApiKey = stripeApiKey };
            if (modelType == ModelType.botmodel)
            {
                var collection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set("serviceConnectivity.sellercred", scc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else if(modelType == ModelType.ruleset)
            {
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("serviceConnectivity.sellercred", scc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return scc;
        }

        public async Task<SendGridCredentials> UpdateSendgridCredentials(string userId, string ModelName, string sendGridAPIKey, ModelType modelType)
        {
            var sgc = new SendGridCredentials { SendGridAPIKey = sendGridAPIKey };
            if(modelType == ModelType.botmodel)
            { 
                var collection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == ModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set("serviceConnectivity.sendgridcred", sgc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else if(modelType == ModelType.ruleset)
            {
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("serviceConnectivity.sendgridcred", sgc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return sgc;
        }

        public async Task<TwilioCredentials> UpdateTwilioCredentials(string userId, string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword, ModelType modelType)
        {
            var tc = new TwilioCredentials { SMSAccountFrom = sMSAccountFrom, SMSAccountIdentification = sMSAccountIdentification, SMSAccountPassword = sMSAccountPassword };
            if(modelType == ModelType.botmodel)
            { 
                var collection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set("serviceConnectivity.twiliocred", tc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else if(modelType == ModelType.ruleset)
            {
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("serviceConnectivity.twiliocred", tc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return tc;
        }

        public async Task<DarlUser> UpdateUserAsync(string userId, DarlUserUpdate user)
        {
            var collection = db.GetCollection<DarlUser>(userCollection);
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
            if (user.apiKey != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.APIKey, user.apiKey));
            if (user.subscriptionType != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.subscriptionType, user.subscriptionType));
            var update = Builders<DarlUser>.Update.Combine(updList);
            var newUser = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<DarlUser, DarlUser> { IsUpsert = false, ReturnDocument= ReturnDocument.After });
            return newUser;
        }

        public async Task<ZendeskCredentials> UpdateZendeskCredentials(string userId, string botModelName, string zendeskApiKey, string zendeskURL, string zendeskUser)
        {
            var collection = db.GetCollection<BotModel>(botModelCollection);
            var zc = new ZendeskCredentials { ZendeskApiKey = zendeskApiKey, ZendeskURL = zendeskURL, ZendeskUser = zendeskUser };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.zendeskcred", zc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return zc;
        }
        private static DarlVar.DataType ConvertDarlInputType(string ty)
        {
            if (ty.ToLower().Contains(DarlVar.DataType.categorical.ToString()))
                return DarlVar.DataType.categorical;
            if (ty.ToLower().Contains(DarlVar.DataType.numeric.ToString()))
                return DarlVar.DataType.numeric;
            if (ty.ToLower().Contains(DarlVar.DataType.sequence.ToString()))
                return DarlVar.DataType.sequence;
            return DarlVar.DataType.textual;
        }

        private List<DarlVar> GetInputs(ParseTree tree, bool random = false)
        {
            var rand = new Random();
            List<DarlVar> list = new List<DarlVar>();
            var inputs = runtime.GetInputNames(tree);
            inputs.Sort();
            foreach (var inp in inputs)
            {
                var dv = new DarlVar { dataType = ConvertDarlInputType(tree.GetMapInputType(inp)), approximate = false, name = inp, unknown = false, weight = 1.0, categories = new Dictionary<string, double>(), values = new List<double>() };
                //load any categories
                var cats = tree.GetMapInputCategories(inp);
                cats.Sort();
                foreach (var cat in cats)
                    dv.categories.Add(cat, 1.0);
                if (dv.dataType == DarlVar.DataType.categorical && dv.categories.Count > 0)
                {
                    dv.Value = random ? cats[rand.Next(cats.Count - 1)] : cats[0];
                }
                else if (dv.dataType == DarlVar.DataType.numeric) //load range
                {
                    var res = tree.GetMapInputRange(inp);
                    if (res.values.Count > 0)
                    {
                        dv.values.Add((double)res.values[0]);
                        dv.Value = dv.values[0].ToString();
                    }
                    if (res.values.Count > 1)
                    {
                        dv.values.Add((double)res.values.Last());
                        if (dv.values[0] == double.NegativeInfinity || dv.values[1] == double.PositiveInfinity)
                        {
                        }
                        else
                        {
                            dv.Value = random ? ((dv.values[1] - dv.values[0]) * rand.NextDouble() + dv.values[0]).ToString() : ((dv.values[0] + dv.values[1]) / 2.0).ToString();
                        }
                    }
                }
                else if (dv.dataType == DarlVar.DataType.textual)
                {
                    dv.Value = inp + "_text";
                }
                else if (dv.dataType == DarlVar.DataType.sequence)
                {
                    dv.sequence = new List<List<string>>();
                }
                list.Add(dv);
            }
            return list;
        }

        private async Task<List<DarlResult>> ProcessValues(List<DarlResult> Values, ParseTree tree)
        {
            var res = await runtime.Evaluate(tree, Values);
            var inputNames = runtime.GetInputNames(tree);
            var outputNames = new List<string>();
            foreach (var mo in tree.GetMapOutputs())
                outputNames.Add(mo.Name);
            foreach (var o in res.ToList())
            {
                if (!outputNames.Contains(o.name))
                {
                    res.Remove(o);
                }
            }
            return res;
        }

        public string GetCurrentUserId(object userContext)
        {
            if (userContext != null)
            {
                var ctxt = userContext as GraphQLUserContext;
                return ctxt.User.Identity.Name ?? backgroundUserId;                   
            }
            return backgroundUserId;
        }

        public async Task<DarlUser> CreateAndProvisionNewUser(DarlUserInput user)
        {
            // create stripe account
            var stripeVals = await CreateStripeCustomer(user.userId, user.InvoiceEmail, false, _config.GetValue<int>("AppSettings:StripeTrialPeriodDays"), user.InvoiceName);
            // provision account
            await ProvisionUser(user.userId);
            //create user
            var mc = db.GetCollection<DarlUser>(userCollection);
            var duser = new DarlUser { Created = DateTime.Now, current_period_end = DateTime.Now + new TimeSpan(_config.GetValue<int>("AppSettings:StripeTrialPeriodDays"), 0, 0, 0, 0), InvoiceEmail = user.InvoiceEmail, InvoiceName = user.InvoiceName, InvoiceOrganization = user.InvoiceOrganization, Issuer = user.Issuer, PaidUsageStarted = DateTime.MaxValue, StripeCustomerId = stripeVals.Item1, UsageStripeSubscriptionItem = stripeVals.Item2, userId = user.userId, subscriptionType = DarlUser.SubscriptionType.individual };
            await mc.InsertOneAsync(duser);
            _logger.LogWarning($"{nameof(CreateAndProvisionNewUser)}: {user.userId}, {user.Issuer}, {user.InvoiceEmail}");
            return duser;
        }

        private async Task<(string, string)> CreateStripeCustomer(string userId, string email, bool corporate, int trialPeriodDays, string name = "", string organization = "")
        {
            var sak = _config["AppSettings:StripeAPIKey"];
            if(!string.IsNullOrEmpty(sak))
            { 
                StripeConfiguration.ApiKey = sak;
                try
                {
                    var options = new CustomerCreateOptions
                    {
                        Email = email,
                        Description = userId,
                        Metadata = new Dictionary<string, string> { { nameof(userId), userId }, { nameof(name), name }, { nameof(organization), organization } }
                    };
                    var service = new CustomerService();
                    Customer customer = await service.CreateAsync(options);
                    var su = await AddSubscriptions(customer.Id, new List<string> { _config["AppSettings:StripeIndividualLicensePlan"], _config["AppSettings:StripeIndividualUsagePlan"] });
                    return (customer.Id, su);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(CreateStripeCustomer));
                }
            }
            return ("", "");
        }

        private async Task ProvisionUser(string userId)
        {
            //copy just the thousandquestions botmodel from the master account

            var bm = await GetBotModel(backgroundUserId, _config["AppSettings:ProvisionBotModel"]);
            await CreateBotModel(userId, _config["AppSettings:ProvisionBotModel"], bm.Model);
            // copy selected rulesets
            foreach (var r in _config["AppSettings:ProvisionRulesets"].Split(','))
            {
                var rs = await GetRuleSet(backgroundUserId, r);
                if(rs != null)
                    await CreateRuleSet(userId, r, rs.Contents, rs.serviceConnectivity);
            }
            foreach (var r in _config["AppSettings:ProvisionMLModels"].Split(','))
            {
                var rs = await GetMlModel(backgroundUserId, r);
                if (rs != null)
                    await CreateMLModel(userId, r, rs.model);
            }
            foreach (var r in _config["AppSettings:ProvisionCollateral"].Split(','))
            {
                var rs = await GetCollateral(backgroundUserId, r);
                if (rs != null)
                    await UpdateCollateral(userId, r, rs);
            }
        }

        public async Task<bool> FactoryReset(string userId)
        {
            try
            { 
                foreach(var bm in await GetBotModelsAsync(userId))
                {
                    await DeleteBotModel(userId, bm.Name);
                }
                foreach (var rs in await GetRuleSetsAsync(userId))
                {
                    await DeleteRuleSet(userId, rs.Name);
                }
                foreach (var ml in await GetMlModelsAsync(userId))
                {
                    await DeleteMLModel(userId, ml.Name);
                }
                foreach (var co in await GetCollaterals(userId))
                {
                    await DeleteCollateral(userId, co.Name);
                }
                await ProvisionUser(userId);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<string> GetDarlFromRuleset(string userId, string rulesetName)
        {
            if (!string.IsNullOrEmpty(rulesetName))
            {
                var rs = await GetRuleSet(userId, rulesetName);
                return rs.Contents.darl;
            }
            return string.Empty;
        }

        public async Task<string> UpdateDarlInRuleset(string userId, string ruleSetName, string darl)
        {
            if (!string.IsNullOrEmpty(ruleSetName))
            {
                var rs = await GetRuleSet(userId, ruleSetName);
                rs.Contents.darl = darl;
                var errors = await  rs.Contents.UpdateFromCode();
                if(errors.Count > 0)
                {
                    throw new ExecutionError($"{ruleSetName} has errors. Check with the on-line editor");
                }
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId );
                var update = Builders<RuleSet>.Update.Set(x => x.Contents, rs.Contents);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return darl;
        }


        public async Task<List<DarlUser>> GetUsers()
        {
            var mc = db.GetCollection<DarlUser>(userCollection);
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        /// <summary>
        /// Get the user if the API key matches and the account is valid.
        /// </summary>
        /// <param name="apiKey">The API key</param>
        /// <returns>the user</returns>
        public async Task<DarlUser> GetUserByApiKey(string apiKey)
        {
            try
            {
                var mc = db.GetCollection<DarlUser>(userCollection);
                var query = mc.AsQueryable()
                .Where(p => p.APIKey == apiKey && p.accountState != DarlUser.AccountState.suspended && p.accountState != DarlUser.AccountState.closed);
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal error in GetUserByApiKey {apiKey}");
                throw new ExecutionError("Internal error in GetUserByApiKey");
            }

        }

        public async Task<string> UpdateUserAPIKey(string userId)
        {
            var collection = db.GetCollection<DarlUser>(userCollection);
            var filter = Builders<DarlUser>.Filter.Where(x => x.userId == userId);
            var updList = new List<UpdateDefinition<DarlUser>>();
            var newAPIKey = Guid.NewGuid().ToString();
            updList.Add(Builders<DarlUser>.Update.Set(x => x.APIKey, newAPIKey));
            var update = Builders<DarlUser>.Update.Combine(updList);
            var newUser = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<DarlUser, DarlUser> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return newAPIKey;
        }

        public async Task<LineageNodeAttributeResources> getLineageNodeAttributeResources(string userId, string botModelName)
        {
            var lnar = new LineageNodeAttributeResources();
            var bm = await GetLineageModel(userId, botModelName);
            lnar.ruleSkeleton = bm.CreateCodeFromFormat();
            lnar.insertionPointText = LineageModel.insertionPointText;
            var rs = await GetRuleSetsAsync(userId);
            lnar.AllRulesets = rs.Select(c => c.Name).ToList();
            lnar.AllRoles = (await GetBotModel(userId, botModelName)).Authorizations.Select(c => c.name).ToList();
            return lnar;
        }

        public async Task<DarlUser> GetUserByStripeId(string stripeId)
        {
            var mc = db.GetCollection<DarlUser>(userCollection);
            var query = mc.AsQueryable()
            .Where(p => p.StripeCustomerId == stripeId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<string> GetCollateral(string userId, string name)
        {
            var mc = db.GetCollection<Collateral>(collateralCollection);
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? string.Empty : def.Value;
        }

        public async Task<Collateral> UpdateCollateral(string userId, string name, string value)
        {
            var mc = db.GetCollection<Collateral>(collateralCollection);
            var model = new Collateral { Name = name, Value = value, userId = userId  };
            var filter = Builders<Collateral>.Filter.Where(x => x.Name == name && x.userId == userId);
            var update = Builders<Collateral>.Update.Set("Value", value);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Collateral, Collateral> { IsUpsert = true });
            return model;
        }

        public async Task<Collateral> DeleteCollateral(string userId, string name)
        {
            var mc = db.GetCollection<Collateral>(collateralCollection);
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Collateral>.Filter.Eq(r => r.userId, userId) & Builders<Collateral>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<List<Collateral>> GetCollaterals(string userId)
        {//adapted to use cursors for large numbers and sizes of collateral
           var list = new List<Collateral>();
           try
           {
                var mc = db.GetCollection<Collateral>(collateralCollection);
                var filter = Builders<Collateral>.Filter.Where(x => x.userId == userId);
                using (var cursor = await mc.FindAsync(filter))
                {
                    await cursor.ForEachAsync(doc =>
                    {
                        list.Add(doc);
                    });
                }
                return list;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, nameof(GetCollaterals));
            }
            return list;
        }

        public async Task<DateTime> GetLastUpdate(string from, string to)
        {
            var mc = db.GetCollection<Update>(updateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.from == from && p.to == to);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? DateTime.MinValue : def.updated;
        }

        public async Task<DateTime> SetLastUpdate(string from, string to)
        {
            var mc = db.GetCollection<Update>(updateCollection);
            var now = DateTime.UtcNow;
            var filter = Builders<Update>.Filter.Where(x => x.from == from && x.to == to);
            var update = Builders<Update>.Update.Set("updated", now);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Update, Update> { IsUpsert = true });
            return now;
        }

        /// <summary>
        /// Create a bug using the VSTS.Net library
        /// </summary>
        /// <remarks>Uses VSTS.Net because Microsoft products don't work with .net core</remarks>
        /// <returns>nothing</returns>    
        public async Task<bool> CreateSupportRequest(string customerName, string customerEmail, string text, string project)
        {
            try
            { 
                var urlBuilderFactory = new OnlineUrlBuilderFactory(_config["AppSettings:AzureDevopsAccount"]);
                var client = VstsClient.Get(urlBuilderFactory, accessToken: _config["AppSettings:AzureDevopsPersonalAccessToken"]);
                await client.CreateWorkItemAsync(project, "bug", new WorkItem
                {
                    Fields = new Dictionary<string, string> {
                    {"System.Title", "User reported bug" },
                    {"Microsoft.VSTS.TCM.ReproSteps",  $"Customer {customerName} with email {customerEmail} reported the following: {text}"}
                }
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error inside of CreateSupportRequest");
                return false;
            }
            return true;
        }

        public async Task<List<Conversation>> GetConversations()
        {
            var mc = db.GetCollection<Conversation>(conversationCollection);
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<Conversation> CreateConversation(Conversation conversationInput)
        {
            var mc = db.GetCollection<Conversation>(conversationCollection);
            await mc.InsertOneAsync(conversationInput);
            return conversationInput;
        }

        public async Task<UserUsage> CreateUserUsage(DateTime date, int count, string userId)
        {
            var usage = new UserUsage(date, count);
            var existing = await GetUserById(userId);
            if (existing == null)
                return usage;
            if (existing.UsageHistory.Any(x => x.Date == date))
                return usage;
            var collection = db.GetCollection<DarlUser>(userCollection);
            var filter = Builders<DarlUser>.Filter.Where(x => x.userId == userId);
            var update = Builders<DarlUser>.Update.Push("UsageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<UserUsage> CreateBotUsage(DateTime date, int count, string userId, string botId)
        {
            var usage = new UserUsage(date, count);
            var collection = db.GetCollection<BotConnection>(botConnectionCollection);
            var filter = Builders<BotConnection>.Filter.Where(x => x.AppId == botId && x.userId == userId);
            var update = Builders<BotConnection>.Update.Push("usageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<BotRuntimeModel> GetBotModelFromAppId(string appId)
        {
            var collection = db.GetCollection<BotConnection>(botConnectionCollection);
            var query = collection.AsQueryable()
            .Where(p => p.AppId == appId);
            var botcon =  await query.SingleAsync();
            var mc = db.GetCollection<BotModel>(botModelCollection);
            var mcquery = mc.AsQueryable()
            .Where(p => p.id == botcon.id);
            var bot = await mcquery.SingleAsync();
            return new BotRuntimeModel { Authorizations = bot.Authorizations, botModelName = bot.Name, Model = bot.Model, password = botcon.Password, serviceConnectivity = bot.serviceConnectivity, userId = bot.userId };
        }

        public async Task<List<BotConnection>> GetBotConnectionsAsync()
        {
            var mc = db.GetCollection<BotConnection>(botConnectionCollection);
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<string> GetUserIdFromAppId(string appId)
        {
            var collection = db.GetCollection<BotConnection>(botConnectionCollection);
            var query = collection.AsQueryable()
            .Where(p => p.AppId == appId);
            var botcon = await query.SingleAsync();
            return botcon.userId;
        }

        public async Task<BotState> GetBotState(string userId, string conversationId)
        {
            var collection = db.GetCollection<BotState>(botStateCollection);
            var query = collection.AsQueryable()
            .Where(p => p.userId == userId && p.conversationId == conversationId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task SaveBotState(BotState bs)
        {
            var collection = db.GetCollection<BotState>(botStateCollection);
            var res = await collection.ReplaceOneAsync(doc => doc.conversationId == bs.conversationId && doc.userId == bs.userId, bs, new ReplaceOptions { IsUpsert = true });
        }

        public async Task CreateDefaultResponse(DefaultResponse response)
        {
            var collection = db.GetCollection<DefaultResponse>(defaultResponseCollection);
            await collection.InsertOneAsync(response);
        }

        public async Task<Document> GetDocument(string userId, string name)
        {
            var mc = db.GetCollection<Document>(documentCollection);
            var query = mc.AsQueryable()
            .Where(p => p.name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Document>> GetDocuments(string userId)
        {
            var mc = db.GetCollection<Document>(documentCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<Document> UpdateDocument(Document document)
        {
            var mc = db.GetCollection<Document>(documentCollection);
            var filter = Builders<Document>.Filter.Where(x => x.name == document.name && x.userId == document.userId);
            var update = Builders<Document>.Update.Set("content", document.content);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Document, Document> { IsUpsert = true });
            return document;
        }

        public async Task<Document> DeleteDocument(string userId, string name)
        {
            var mc = db.GetCollection<Document>(documentCollection);
            var query = mc.AsQueryable().Where(p => p.name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Document>.Filter.Eq(r => r.userId, userId) & Builders<Document>.Filter.Eq(r => r.name, name));
            return old;
        }

        public async Task<DarlVar> CreateRulesetPreload(string userId, string rulesetName, DarlVar preloadData)
        {
            if(string.IsNullOrEmpty(preloadData.name))
            {
                throw new ExecutionError($"preloadData name can't be empty.");
            }
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var rs = await GetRuleSet(userId, rulesetName);
            if (rs != null)
            {
                var list = new List<DarlVar>();
                if(rs.Contents.preload != null)
                {
                    list.AddRange(rs.Contents.preload);
                }
                //check if a DarlVar already exists with the same name and update if so.
                if(list.Any(a => a.name == preloadData.name))
                {
                    list.Remove(list.First(a => a.name == preloadData.name));
                }
                list.Add(preloadData);
                var filter = Builders<RuleSet>.Filter.Where(x => x.id == rs.id && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("Contents.preload", list);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else
            {
                throw new ExecutionError($"Can't Find {rulesetName}.");
            }
            return preloadData;
        }

        /// <summary>
        /// Write a json version of a ruleset to file.
        /// </summary>
        /// <returns></returns>
        public async Task WriteRuleFormForTest(string userId, string ruleset, string filename)
        {
            var rs = await GetRuleSet(userId, ruleset);
            if(rs != null)
                await System.IO.File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(rs.Contents, new StringEnumConverter()));
        }

        public async Task<TriggerView> UpdateRuleFormTrigger(string userId, string ruleSetName, TriggerViewInput trigger)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var rs = await GetRuleSet(userId, ruleSetName);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId);
            if (rs.Contents.trigger == null) //create a new TriggerView and add the data
            {
                var trigg = new TriggerView();
                trigg.addressSource =  DarlCommon.SourceType.fixedvalue;
                trigg.addressText = String.Empty;
                trigg.attachmentName = String.Empty;
                trigg.attachmentUri = String.Empty;
                trigg.bodySource = DarlCommon.SourceType.fixedvalue;
                trigg.bodyText = String.Empty;
                trigg.emailFrom = String.Empty;
                trigg.postData = String.Empty;
                trigg.postDataSource  = DarlCommon.SourceType.fixedvalue;
                trigg.postDataUri = String.Empty;
                trigg.postType = DarlCommon.PostType.darlvarlist;
                trigg.queueData = String.Empty;
                trigg.queueDataSource = DarlCommon.SourceType.fixedvalue;
                trigg.queueName = String.Empty;
                trigg.sendAttachment = String.Empty;
                trigg.sendAttachmentSource = DarlCommon.SourceType.fixedvalue;
                trigg.sendBug = String.Empty;
                trigg.sendBugSource = DarlCommon.SourceType.fixedvalue;
                trigg.sendEmail = String.Empty;
                trigg.sendEmailSource = DarlCommon.SourceType.fixedvalue;
                trigg.subjectSource = DarlCommon.SourceType.fixedvalue;
                trigg.subjectText = String.Empty;
                trigg.graphqlDataSource = DarlCommon.SourceType.fixedvalue;
                trigg.graphqlData = String.Empty;

                var update = Builders<RuleSet>.Update.Set("Contents.trigger", trigg);
                var rs1 = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { ReturnDocument = ReturnDocument.After });
                return rs1.Contents.trigger;
            }
            else
            {
                var updList = new List<UpdateDefinition<RuleSet>>();
                if (trigger.addressSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.addressSource, trigger.addressSource));
                if (trigger.addressText != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.addressText, trigger.addressText));
                if (trigger.attachmentName != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.attachmentName, trigger.attachmentName));
                if (trigger.attachmentUri != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.attachmentUri, trigger.attachmentUri));
                if (trigger.bodySource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.bodySource, trigger.bodySource));
                if (trigger.bodyText != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.bodyText, trigger.bodyText));
                if (trigger.emailFrom != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.emailFrom, trigger.emailFrom));
                if (trigger.postData != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.postData, trigger.postData));
                if (trigger.postDataSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.postDataSource, trigger.postDataSource));
                if (trigger.postDataUri != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.postDataUri, trigger.postDataUri));
                if (trigger.postType != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.postType, trigger.postType));
                if (trigger.queueData != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.queueData, trigger.queueData));
                if (trigger.queueDataSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.queueDataSource, trigger.queueDataSource));
                if (trigger.queueName != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.queueName, trigger.queueName));
                if (trigger.sendAttachment != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendAttachment, trigger.sendAttachment));
                if (trigger.sendAttachmentSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendAttachmentSource, trigger.sendAttachmentSource));
                if (trigger.sendBug != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendBug, trigger.sendBug));
                if (trigger.sendBugSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendBugSource, trigger.sendBugSource));
                if (trigger.sendEmail != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendEmail, trigger.sendEmail));
                if (trigger.sendEmailSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.sendEmailSource, trigger.sendEmailSource));
                if (trigger.subjectSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.subjectSource, trigger.subjectSource));
                if (trigger.subjectText != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.subjectText, trigger.subjectText));
                if (trigger.graphqlDataSource != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.graphqlDataSource, trigger.graphqlDataSource));
                if (trigger.graphqlData != null)
                    updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.trigger.graphqlData, trigger.graphqlData));
                var update = Builders<RuleSet>.Update.Combine(updList);
                var rs1 = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
                return rs1.Contents.trigger;
            }
        }

        /// <summary>
        /// Create a version of this resource in the reserve account
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="resourceType"></param>
        /// <param name="name"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<string> CopyToReserveAccount(string userId, ResourceType resourceType, string name, string newName)
        {
            var destName = string.IsNullOrEmpty(newName) ? name : newName;
            switch (resourceType)
            {
                case ResourceType.botmodel:
                    {
                        var bm = await GetBotModel(userId, name);
                        if(bm == null)
                        {
                            throw new ExecutionError($"Botmodel {name} doesn't exist in account {userId}");
                        }
                        await CreateBotModel(backgroundUserId, destName, bm.Model);
                    }
                    break;
                case ResourceType.ruleset:
                    {
                        var rs = await GetRuleSet(userId, name);
                        if (rs == null)
                        {
                            throw new ExecutionError($"Ruleset {name} doesn't exist in account {userId}");
                        }
                        await CreateRuleSet(backgroundUserId, destName, rs.Contents, new ServiceConnectivity());
                    }
                    break;
                case ResourceType.mlmodel:
                    {
                        var ml = await GetMlModel(userId, name);
                        if (ml == null)
                        {
                            throw new ExecutionError($"MLModel {name} doesn't exist in account {userId}");
                        }
                        await CreateMLModel(backgroundUserId, destName, ml.model);
                    }
                    break;
                case ResourceType.document:
                    {
                        var doc = await GetDocument(userId, name);
                        if (doc == null)
                        {
                            throw new ExecutionError($"Document {name} doesn't exist in account {userId}");
                        }
                        doc.userId = backgroundUserId;
                        await UpdateDocument(doc);
                    }
                    break;
                case ResourceType.collateral:
                    {
                        var coll = await GetCollateral(userId, name);
                        if (coll == null)
                        {
                            throw new ExecutionError($"Collateral {name} doesn't exist in account {userId}");
                        }
                        await UpdateCollateral(backgroundUserId, destName, coll);
                    }
                    break;

            }
            return destName;
        }

        public async Task<List<Update>> GetUpdates()
        {
            var mc = db.GetCollection<Update>(updateCollection);
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        //create contact if not found, add purchase.
        public async Task<Purchase> ReportPurchase(string email, string name, string sessionId, DateTime date)
        {
            var purchase = new Purchase { date = date, sessionId = sessionId };
            var contact = await GetContactByEmail(email);
            if(contact != null) //existing contact
            {
                //add purchase - handle empty purchases list
                var collection = db.GetCollection<Contact>(contactCollection);
                var filter = Builders<Contact>.Filter.Where(x => x.Email == email);
                var update = Builders<Contact>.Update.Push("purchases", purchase);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else
            {
                contact = new Contact { Email = email, FirstName = name, Created = date, Source = "Online purchase", Sector = "Advice", IntroSent = true, purchases = new List<Purchase> {purchase} };
                await CreateContactAsync(contact);
            }
            return purchase;
        }

        //create contact if not found, add purchase.
        public async Task<DarlLicense> ReportLicense(string email, string company, string licensekey, DateTime endDate)
        {
            var dlicense = new DarlLicense { created = DateTime.UtcNow, licensekey = licensekey, terminates = endDate };
            var contact = await GetContactByEmail(email);
            if (contact != null) //existing contact
            {
                //add purchase - handle empty purchases list
                var collection = db.GetCollection<Contact>(contactCollection);
                var filter = Builders<Contact>.Filter.Where(x => x.Email == email);
                var update = Builders<Contact>.Update.Push("licenses", dlicense);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else
            {
                contact = new Contact { Email = email, Company = company, Created = DateTime.UtcNow, Source = "Online license", Sector = "Advice", IntroSent = true, licenses = new List<DarlLicense> { dlicense } };
                await CreateContactAsync(contact);
            }
            return dlicense;
        }

        public async Task<bool> CheckEmail(string email, string ipaddress = "")
        {
            var zeroBounceAPI = new ZeroBounce.ZeroBounceAPI();
            zeroBounceAPI.api_key = _config["AppSettings:ZeroBounceAPIKey"];
            zeroBounceAPI.RequestTimeOut = 150000; // Any integer value in milliseconds
            zeroBounceAPI.EmailToValidate = email;
            zeroBounceAPI.ip_address = ipaddress;
            var apiProperties = await zeroBounceAPI.ValidateEmailAsync();
            switch (apiProperties.status.ToLower())
            {
                case "unknown":
                case "valid":
                case "catch-all":
                    return true;
                default:
                    return false;
            }
        }

        public async Task<List<DarlVar>> InferFromDarlDarlVar(string userId, string code, List<DarlVarInput> inputs)
        {
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                        var tree = runtime.CreateTreeEdit(code);
                        if (tree.HasErrors())
                        {
                            var errors = new List<DarlVar>();
                            int errorCount = 0;
                            foreach (var pm in tree.ParserMessages)
                            {
                                var level = pm.Level == ErrorLevel.Error ? "error" : "warning";
                                errors.Add(new DarlVar { name = $"error{errorCount++}", Value = $"line_no = {pm.Location.Line + 1}, column_no_start = {pm.Location.Column + 1}, column_no_stop = {pm.Location.Column + 2}, message = {pm.Message}, severity = {level}", dataType = DarlVar.DataType.textual });
                            }
                            return errors; //errors, just add them to the input and quit.
                        }
                        //now convert the inputs to DarlVars
                        var res = await ProcessValues(DarlVarExtensions.Convert(DarlVarInput.Convert(inputs)), tree);
                        _logger.LogWarning($"{nameof(InferFromDarlDarlVar)}: {userId}");

                    return DarlVarExtensions.Convert(res);
                 }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(InferFromDarlDarlVar));
                var errors = new List<DarlVar>();
                errors.Add(new DarlVar { name = "error", Value = ex.Message });
                return errors;
            }
            return null;
        }

        public async Task<ModelDetails> CreateRulesetDetails(string userId, string rulesetName, ModelDetails details)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var rs = await GetRuleSet(userId, rulesetName);
            if (rs.Contents.trigger == null)
                await UpdateRuleFormTrigger(userId, rulesetName, null);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == rulesetName && x.userId == userId);
            var updList = new List<UpdateDefinition<RuleSet>>();
            if (details.author != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.author, details.author));
            if (details.copyright != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.copyright, details.copyright));
            if (details.description != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.description, details.description));
            if (details.license != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.license, details.license));
            if (details.version != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.version, details.version));
            if (details.price != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.price, details.price));
            if (details.currency != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.currency, details.currency));
             var update = Builders<RuleSet>.Update.Combine(updList);
            var rs1 = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return new ModelDetails { author = rs1.Contents.author, copyright = rs1.Contents.copyright, description = rs1.Contents.description, license = rs1.Contents.license, version = rs1.Contents.version };
        }

        public async Task<GraphQLCredentials> UpdateGraphQLCredentials(string userId, string modelName, string url, string header, ModelType modelType)
        {
            var sgc = new GraphQLCredentials { url = url, header = header };
            if (modelType == ModelType.botmodel)
            {
                var collection = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == modelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set("serviceConnectivity.graphqlcred", sgc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            else if (modelType == ModelType.ruleset)
            {
                var collection = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == modelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set("serviceConnectivity.graphqlcred", sgc);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return sgc;
        }

        public async Task<GraphQLCredentials> DeleteGraphQLCredentials(string userId, string botModelName, ModelType modelType)
        {
            if (modelType == ModelType.botmodel)
            {
                var mc = db.GetCollection<BotModel>(botModelCollection);
                var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.graphqlcred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            else if (modelType == ModelType.ruleset)
            {
                var mc = db.GetCollection<RuleSet>(rulesetCollection);
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
                var update = Builders<RuleSet>.Update.Set(p => p.serviceConnectivity.graphqlcred, null);
                await mc.UpdateOneAsync(filter, update);
            }
            return null;
        }

        public async Task<long> GetContactsCount(string userId)
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            return await mc.CountAsync(new BsonDocument());
        }

        public async Task<long> GetContactsMonthCount(string userId)
        {
            var mc = db.GetCollection<Contact>(contactCollection);
            var oneMonthBefore = DateTime.UtcNow - new TimeSpan(30, 0, 0, 0);
            return mc.AsQueryable().Where(x => x.Created > oneMonthBefore).Count();
        }

        public async Task<long> GetContactsDayCount(string userId)
        {
            var oneDayBefore = DateTime.UtcNow - new TimeSpan(1, 0, 0, 0);
            var mc = db.GetCollection<Contact>(contactCollection);
            return mc.AsQueryable().Where(x => x.Created > oneDayBefore).Count();
        }

        public async Task<long> GetUserCount(string userId)
        {
            var mc = db.GetCollection<DarlUser>(userCollection);
            return await mc.CountDocumentsAsync(new BsonDocument());
        }

        public async Task<long> GetConversationCount(string userId)
        {
            var mc = db.GetCollection<Conversation>(conversationCollection);
            return await mc.CountDocumentsAsync(new BsonDocument());
        }

        public async Task<UserUsage> CreateSimulationUsage(DateTime date, int count, string userId, string model)
        {
            throw new NotImplementedException();
        }

        public async Task<UserUsage> CreateMLModelUsage(DateTime date, int count, string userId, string model)
        {
            var usage = new UserUsage(date, count);
            //add no overwrite facility 
            var existing = await GetMlModel(userId, model);
            if (existing == null)
                return usage;
            if (existing.UsageHistory.Any(x => x.Date == date))
                return usage;
            var collection = db.GetCollection<MLModel>(mlModelCollection);
            var filter = Builders<MLModel>.Filter.Where(x => x.userId == userId && x.Name == model);
            var update = Builders<MLModel>.Update.Push("UsageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<UserUsage> CreateRuleSetUsage(DateTime date, int count, string userId, string model)
        {
            var usage = new UserUsage(date, count);
            var existing = await GetRuleSet(userId, model);
            if (existing == null)
                return usage;
            if (existing.UsageHistory.Any(x => x.Date == date))
                return usage;
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.userId == userId && x.Name == model);
            var update = Builders<RuleSet>.Update.Push("UsageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<UserUsage> CreateBotModelUsage(DateTime date, int count, string userId, string model)
        {
            var usage = new UserUsage(date, count);
            var existing = await GetBotModel(userId, model);
            if (existing == null)
                return usage;
            if (existing.UsageHistory.Any(x => x.Date == date))
                return usage;
            var collection = db.GetCollection<BotModel>(botModelCollection);
            var filter = Builders<BotModel>.Filter.Where(x => x.userId == userId && x.Name == model);
            var update = Builders<BotModel>.Update.Push("UsageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<DarlUser.SubscriptionType> GetSubscriptionType(string userId)
        {
            var sak = _config["AppSettings:StripeAPIKey"];
            if (String.IsNullOrEmpty(sak))
                throw new ExecutionError("Subscriptions not enabled");
            var user = await GetUserById(userId);
            if(user != null)
            {
                return user.subscriptionType ?? DarlUser.SubscriptionType.individual;
            }
            throw new ExecutionError("User not found");
        }

        /// <summary>
        /// Updates subscriptions as permitted
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<DarlUser.SubscriptionType> UpdateSubscriptionType(string userId, DarlUser.SubscriptionType type)
        {
            var sak = _config["AppSettings:StripeAPIKey"];
            try
            {
                if (String.IsNullOrEmpty(sak))
                    throw new ExecutionError("Subscriptions not enabled");
                StripeConfiguration.ApiKey = sak;
                var user = await GetUserById(userId);
                if(user == null)
                {
                    throw new ExecutionError($"user {userId} doesn't exist");
                }
                if(string.IsNullOrEmpty(user.StripeCustomerId)) //somehow a customerId was not created.
                {
                    var stripeVals = await CreateStripeCustomer(user.userId, user.InvoiceEmail, false, _config.GetValue<int>("AppSettings:StripeTrialPeriodDays"), user.InvoiceName);
                    await UpdateUserAsync(userId, new DarlUserUpdate { StripeCustomerId = stripeVals.Item1, UsageStripeSubscriptionItem = stripeVals.Item2 });
                }
                var currentSubscription = await GetSubscriptionType(userId);
                if (currentSubscription == type)
                    return type;
                switch (currentSubscription)
                {
                    case DarlUser.SubscriptionType.individual:
                        {
                            string newSubscription = string.Empty;
                            switch (type)
                            {
                                case DarlUser.SubscriptionType.corporate:
                                    await RemoveSubscriptions(user.StripeCustomerId);
                                    newSubscription = await AddSubscriptions(user.StripeCustomerId, new List<string> { _config["AppSettings:StripeCorporateLicensePlan"], _config["AppSettings:StripeCorporateUsagePlan"] });
                                    await UpdateSubsciption(userId, newSubscription, type);
                                    return type;
                                case DarlUser.SubscriptionType.embedded:
                                    await RemoveSubscriptions(user.StripeCustomerId);
                                    await AddSubscriptions(user.StripeCustomerId, new List<string> { _config["AppSettings:StripeEmbeddedPlan"] });
                                    return type;
                                case DarlUser.SubscriptionType.inhouse:
                                    if (user.accountState == DarlUser.AccountState.admin)
                                    {
                                        await RemoveSubscriptions(user.StripeCustomerId);
                                        newSubscription = await AddSubscriptions(user.StripeCustomerId, new List<string> { _config["AppSettings:StripeInHousePlan"], _config["AppSettings:StripeInHouseUsage"] });
                                        await UpdateSubsciption(userId, newSubscription, type);
                                        return type;
                                    }
                                    break;
                            }
                        }
                        break;
                    case DarlUser.SubscriptionType.corporate:
                        {
                            if (type == DarlUser.SubscriptionType.embedded)
                            {
                                await RemoveSubscriptions(user.StripeCustomerId);
                                var newSubscription = await AddSubscriptions(user.StripeCustomerId, new List<string> { _config["StripeEmbeddedPlan"] });
                                await UpdateSubsciption(userId, newSubscription, type);
                                return type;
                            }
                        }
                        break;
                }
                throw new ExecutionError($"You can't go from subscription type {currentSubscription} to {type}");
            }
            catch(Exception ex)
            {
                throw new ExecutionError($"Update subscriptionType failed: {ex.Message}", ex);
            }
        }

        private async Task UpdateSubsciption(string userId, string newSubscription, DarlUser.SubscriptionType subsType)
        {
            await UpdateUserAsync(userId, new DarlUserUpdate { UsageStripeSubscriptionItem = newSubscription, subscriptionType = subsType });
        }

        /// <summary>
        /// Cancels all subscriptions and generates immediate prorata bills.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task RemoveSubscriptions(string customerId)
        {
            var sak = _config["AppSettings:StripeAPIKey"];
            if (String.IsNullOrEmpty(sak))
                throw new ExecutionError("Subscriptions not enabled");
            StripeConfiguration.ApiKey = sak;
            var service = new CustomerService();
            var cust = await service.GetAsync(customerId);
            var subService = new SubscriptionService();
            foreach (var sub in cust.Subscriptions)
            {
                subService.Cancel(sub.Id, new SubscriptionCancelOptions { InvoiceNow = true, Prorate = true });
            }
        }

        /// <summary>
        /// Adds the subscriptions based on the plans passed
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="plans"></param>
        /// <returns></returns>
        private async Task<string> AddSubscriptions(string customerId, List<string> plans)
        {
            var items = new List<SubscriptionItemOptions>();
            foreach(var p in plans)
            {
                items.Add(new SubscriptionItemOptions { Plan = p });
            }            
            var subsoptions = new SubscriptionCreateOptions
            {
                Items = items,
                TrialPeriodDays = _config.GetValue<int>("AppSettings:StripeTrialPeriodDays"),
                Customer = customerId,
                CollectionMethod = "send_invoice",
                DaysUntilDue = 14,
            };
            var subsservice = new SubscriptionService();
            Subscription subscription = await subsservice.CreateAsync(subsoptions);
            if (plans.Count == 2)
            {
                //extract the subscription item id for the usage so we can register usages with stripe
                return subscription.Items.Last().Id;
            }
            return string.Empty;
        }
        /// <summary>
        /// Closes an account and creates immediate billing.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> CloseAccount(string userId)
        {
            var user = await GetUserById(userId);
            if (user != null)
            {
                //if subscriptions enabled cancel all subscriptions and create final bills.
                var sak = _config["AppSettings:StripeAPIKey"];
                if (!String.IsNullOrEmpty(sak))
                { 
                    StripeConfiguration.ApiKey = sak;
                    await RemoveSubscriptions(user.StripeCustomerId);
                }
                await UpdateUserAsync(userId, new DarlUserUpdate { accountState = DarlUser.AccountState.closed });
                return true;
            }
            throw new ExecutionError("User does not exist");
        }

        public async Task<string> CreateKey(string userId, string company, string email, DateTime endDate)
        {
            var key = _licensing.CreateKey(endDate, company, email);
            await ReportLicense(email, company, key, endDate);
            return key;
        }

        public async Task<bool> CheckKey(string userId, string key)
        {
            await GetUserById(userId);
            return _licensing.CheckKey(key);
        }

        public async Task<ModelDetails> UpdateRuleFormDetails(string userId, string ruleSetName, ModelDetails details)
        {
            var collection = db.GetCollection<RuleSet>(rulesetCollection);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId );
            var updList = new List<UpdateDefinition<RuleSet>>();
            if (details.author != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.author, details.author));
            if (details.copyright != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.copyright, details.copyright));
            if (details.currency != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.currency, details.currency));
            if (details.description != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.description, details.description));
            if (details.license != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.license, details.license));
            if (details.price != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.price, details.price));
            if (details.version != null)
                updList.Add(Builders<RuleSet>.Update.Set(x => x.Contents.version, details.version));
            var update = Builders<RuleSet>.Update.Combine(updList);
            var rs = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<RuleSet, RuleSet> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
            return new ModelDetails { author = rs.Contents.author, copyright = rs.Contents.copyright, currency = rs.Contents.currency, description = rs.Contents.description, license = rs.Contents.license, price = rs.Contents.price, version = rs.Contents.version };
        }
    }
}