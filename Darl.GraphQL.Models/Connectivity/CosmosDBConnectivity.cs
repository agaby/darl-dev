using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using DarlCommon;
using DarlCompiler;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Default = Darl.GraphQL.Models.Models.Default;
using MLModel = Darl.GraphQL.Models.Models.MLModel;
using Darl_standard.Darl.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using Stripe;
using System.Security.Claims;
using VSTS.Net.Types;
using VSTS.Net;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.WorkItems;
using Darl.Lineage.Bot.Stores;
using Darl.GraphQL.Models.Schemata;

namespace Darl.GraphQL.Models.Connectivity
{
    public class CosmosDBConnectivity : IConnectivity
    {
        private IOptions<AppSettings> _opt;
        public IMongoDatabase db { get; set; }
        private MongoClient mongoClient;
        private DarlRunTime runtime = new DarlRunTime();
        private TelemetryClient telemetry = new TelemetryClient();
        public CosmosDBConnectivity(IOptions<AppSettings> optionsAccessor)
        {
            _opt = optionsAccessor;

            string connectionString = _opt.Value.MongoConnectionString;
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            mongoClient = new MongoClient(settings);
            db = mongoClient.GetDatabase(_opt.Value.MongoDatabase);
        }

        public async Task<Authorization> CreateAuthorization(string userId, string botModelName, Authorization auth)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
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
                var collection = db.GetCollection<BotConnection>("botconnection");
                await collection.InsertOneAsync(bc);
                //bc is updated with id during insert
                //Add to BotModel
                var bmcollection = db.GetCollection<BotModel>("botmodel");
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
            var mc = db.GetCollection<BotModel>("botmodel");
            var model = new BotModel { Name = name, userId = userId, Authorizations = new List<Authorization>(), serviceConnectivity = new ServiceConnectivity(), Model = lm, botconnections = new List<MongoDB.Bson.ObjectId>() };
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
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<Default> CreateDefault(string name, string value)
        {
            var mc = db.GetCollection<Default>("default");
            var model = new Default { Name = name, Value = value };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<BotModel> CreateDefaultModel(string userId, string name)
        {
            var botModel = new BotModel { Name = name, userId = userId };
            var mc = db.GetCollection<BotModel>("botmodel");
            await mc.InsertOneAsync(botModel);
            return botModel;
        }

        public async Task<Models.MLModel> CreateEmptyMLModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var model = new MLModel { Name = name, model = new DarlCommon.MLModel { name = name, percentTest = 0, sets = 3, darl = "ruleset newRuleSet supervised\n{\n}\n" }, results = new List<MLResult>(), userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<BotModel> CreateEmptyModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var lm = new LineageModel();
            var model = new BotModel { Name = name, userId = userId, Model = ConvertLineageModel(lm) };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<RuleSet> CreateEmptyRuleSet(string userId, string name)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var model = new RuleSet { Name = name, userId = userId };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<LineageNodeDefinition> CreateLineageNode(string userId, string botModelName, string parent, string newName)
        {
            var currentModel = await GetLineageModel(userId, botModelName);
            var lmn = currentModel.tree.Add(parent, newName.Trim().ToLower());
            await SaveLineageModel(userId, botModelName, currentModel);
            return new LineageNodeDefinition { text = lmn.element.lineage, id = lmn.element.lineage, definition = lmn.element.description, attributes = null, children = false };
        }

        public async Task<MLModel> CreateMLModel(string userId, string name, DarlCommon.MLModel model)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
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
            return new LineageNodeDefinition { text = lmn.element.lineage, id = lmn.element.lineage, definition = lmn.element.description, attributes = null, children = false };
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
                        var rc = db.GetCollection<RuleSet>("ruleset");
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
                    throw new ExecutionError("Errors in Darl source");
                }
            }
            return null;
        }

        public async Task<RuleSet> CreateRuleSet(string userId, string name, RuleForm rf, ServiceConnectivity sc)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
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
                var mc = db.GetCollection<DarlUser>("user");
                var duser = new DarlUser { Created = user.Created, current_period_end = user.current_period_end, InvoiceEmail = user.InvoiceEmail, InvoiceName = user.InvoiceName, InvoiceOrganization = user.InvoiceOrganization, Issuer = user.Issuer, PaidUsageStarted = user.PaidUsageStarted, StripeCustomerId = user.StripeCustomerId, UsageStripeSubscriptionItem = user.UsageStripeSubscriptionItem, userId = user.userId };
                await mc.InsertOneAsync(duser);
                return duser;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw new ExecutionError("Duplicate or malformed data");
            }
        }

        public async Task<string> DeleteAuthorization(string userId, string name, string name1)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == name && x.userId == userId);
            var update = Builders<BotModel>.Update.PullFilter(p => p.Authorizations, f => f.name == name1);
            await mc.UpdateOneAsync(filter, update);
            return name1;
        }

        public async Task<AzureCredentials> DeleteAzureCredentials(string userId, string botModelName)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.azurecred, null);
            await mc.UpdateOneAsync(filter, update);
            return null;
        }

        public async Task<BotConnection> DeleteBotConnection(string userId, string botModelName, string appId)
        {
            //delete connection
            var bc = db.GetCollection<BotConnection>("botconnection");
            var query = bc.AsQueryable().Where(p => p.AppId == appId);
            var old = await query.FirstOrDefaultAsync();
            var bcfilter = Builders<BotConnection>.Filter.Where(x => x.AppId == appId);
            await bc.DeleteOneAsync(bcfilter);
            //delete reference
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.PullFilter(p => p.botconnections, f => f == old.id);
            await mc.UpdateOneAsync(filter, update);
            return old;
        }

        public async Task<BotModel> DeleteBotModel(string userId, string name)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
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
                var mc = db.GetCollection<Contact>("contact");
                var query = mc.AsQueryable().Where(p => p.Email == email);
                var old = await query.FirstOrDefaultAsync();
                DeleteResult res = await mc.DeleteOneAsync<Contact>(r => r.Email == email);
                return old;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
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

        public async Task<LineageNodeDefinition> DeleteLineageNode(string userId, string botModelName, string id)
        {
            var lm = await GetLineageModel(userId, botModelName);
            lm.tree.Delete(id);
            await SaveLineageModel(userId, botModelName, lm);
            return new LineageNodeDefinition();
        }

        public async Task<MLModel> DeleteMLModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
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
            var mc = db.GetCollection<RuleSet>("ruleset");
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<RuleSet>.Filter.Eq(r => r.userId, userId) & Builders<RuleSet>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<SellerCenterCredentials> DeleteSellereCenterCredentials(string userId, string botModelName)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.sellercred, null);
            await mc.UpdateOneAsync(filter, update);
            return null;
        }

        public async Task<SendGridCredentials> DeleteSendgridCredentials(string userId, string botModelName)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.sendgridcred, null);
            await mc.UpdateOneAsync(filter, update);
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

        public async Task<TwilioCredentials> DeleteTwilioCredentials(string userId, string botModelName)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set(p => p.serviceConnectivity.twiliocred, null);
            await mc.UpdateOneAsync(filter, update);
            return null;
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
                telemetry.TrackException(ex);
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
            var collection = db.GetCollection<BotModel>("botmodel");
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.Authorizations);
            return await query.SingleAsync();
        }

        public async Task<List<BotConnection>> GetBotConnectivity(string userId, string name)
        {
            var list = new List<BotConnection>();
            var collection = db.GetCollection<BotModel>("botmodel");
            var query = collection.AsQueryable()
                .Where(a => a.userId == userId && a.Name == name)
                .Select(a => a.botconnections);
            var references = await query.SingleAsync();
            if (references.Count != 0)
            { 
                var bccollection = db.GetCollection<BotConnection>("botconnection");
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
            var mc = db.GetCollection<BotModel>("botmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == name);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<BotModel>> GetBotModelsAsync(string userId)
        {
            var mc = db.GetCollection<BotModel>("botmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<List<UserUsage>> GetBotUsage(string appId)
        {
            var collection = db.GetCollection<BotConnection>("botconnection");
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
            next.Add(new LineageNodeDefinition { id = id, text = r.element.lineage, children = notleaf, definition = r.element.description, attributes = att });
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
            var mc = db.GetCollection<Contact>("contact");
            var query = mc.AsQueryable()
            .Where(p => p.Email.ToLower() == email.ToLower());
            return await query.FirstOrDefaultAsync();
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
            var query = mc.AsQueryable(new AggregateOptions {  BatchSize = 10000});
            return await query.ToListAsync();
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
                telemetry.TrackEvent($"Bad lineage lookup for word {word} message: {ex.Message}");
                return new List<LineageRecord>();
            }
        }

        public async Task<Models.MLModel> GetMlModel(string userId, string name)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Models.MLModel>> GetMlModelsAsync(string userId)
        {
            var mc = db.GetCollection<MLModel>("mlmodel");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<RuleSet> GetRuleSet(string userId, string name)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<RuleSet>> GetRuleSetsAsync(string userId)
        {
            var mc = db.GetCollection<RuleSet>("ruleset");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<DarlUser> GetUserById(string id)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.userId == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<DarlUser>> GetUsersByEmail(string email)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.InvoiceEmail.ToLower() == email.ToLower());
            return await query.ToListAsync();
        }

        public async Task<List<DarlVar>> InferFromRuleSetDarlVar(string userId, string ruleSetName, List<DarlVar> inputs)
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
                            telemetry.TrackEvent("DarlInf used with errors");
                            return errors; //errors, just add them to the input and quit.
                        }
                        var res = await ProcessValues(DarlVarExtensions.Convert(inputs), tree);
                        telemetry.TrackEvent("InferFromRuleSetDarlVar used");
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
                telemetry.TrackEvent("DarlInf Exception");
                var errors = new List<DarlVar>();
                errors.Add(new DarlVar { name = "error", Value = ex.Message });
                return errors;
            }
            return inputs;
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
            var mc = db.GetCollection<MLModel>("mlmodel");
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
            //insert result into MLModel result array
            var mlr = new MLResult { code = rep.code, errorText = rep.errorText, executionDate = start, executionTime = (end - start), trainPercent = rep.trainPercent, trainPerformance = rep.trainPerformance, testPerformance = rep.testPerformance, unknownResponsePercent = rep.unknownResponsePercent };
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

        public async Task<LineageNodeAttributeUpdate> UpdateAttribute(string userId, string botModelName, string path, LineageNodeAttributeUpdate attribute)
        {
            var lm = await GetLineageModel(userId, botModelName);
            var newCode = lm.ReconcileCode(attribute.darl, new BotFragment { CallRuleset = attribute.call, RandomResponses = attribute.randomResponses, Response = attribute.response }, path);
            lm.tree.SaveAttributes(path, newCode.Trim(), new List<string>(), attribute.accessRoles );
            await SaveLineageModel(userId, botModelName, lm); 
            return attribute;
        }

        public async Task<AzureCredentials> UpdateAzureCredentials(string userId, string botModelName, string apiKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var ac = new AzureCredentials { AzureAPIKey = apiKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.azurecred", ac);
            await collection.FindOneAndUpdateAsync(filter, update);
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
        /// Needed because the botformat is string encoded.
        /// </summary>
        /// <param name="botModelName"></param>
        /// <returns></returns>
        private async Task<BotFormat> GetBotFormat(string userId, string botModelName)
        {
            var lm = await GetLineageModel(userId, botModelName);
            return string.IsNullOrEmpty(lm.form) ? null : JsonConvert.DeserializeObject<BotFormat>(lm.form, new StringEnumConverter());
        }

        /// <summary>
        /// Save a botformat to the lineage model and thence to the database
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
            var mc = db.GetCollection<BotModel>("botmodel");
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("Model", binaryLM);
            await mc.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Get the byte represetation of a lineagemodel
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
            var collection = db.GetCollection<Contact>("contact");
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
            var mc = db.GetCollection<Default>("default");
            var model = new Default { Name = name, Value = value };
            var filter = Builders<Default>.Filter.Where(x => x.Name == name);
            var update = Builders<Default>.Update.Set("Value", value);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Default, Default> { IsUpsert = false });
            return model;
        }

        public async Task<MLModel> UpdateMLSpec(string userId, string name, MLSpecUpdate mlspec)
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

        public async Task<InputFormat> UpdateRuleFormInputFormat(string userId, string name, string inputName, InputFormatUpdate inputUpdate)
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

        public async Task<LanguageText> UpdateRuleFormLanguageText(string userId, string ruleSetName, string languageName, string languageText)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.Any(i => i.Name == languageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList[-1].Text, languageText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new LanguageText { Name = languageName, Text = languageText };
        }

        public async Task<OutputFormat> UpdateRuleFormOutputFormat(string userId, string ruleSetName, string outputName, OutputFormatUpdate outputUpdate)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
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
            var collection = db.GetCollection<RuleSet>("ruleset");
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId && x.Contents.language.LanguageList.First(i => i.Name == languageName).VariantList.Any(a => a.Language == isoLanguageName));
            var update = Builders<RuleSet>.Update.Set(x => x.Contents.language.LanguageList[-1].VariantList[-1].Text, variantText);
            await collection.FindOneAndUpdateAsync(filter, update);
            return new VariantText { Language = isoLanguageName, Text = variantText };
        }

        public async Task<SellerCenterCredentials> UpdateSellerCenterCredentials(string userId, string botModelName, bool liveMode, string merchantId, string stripeApiKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var scc = new SellerCenterCredentials { LiveMode = liveMode, MerchantId = merchantId, StripeApiKey = stripeApiKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.sellercred", scc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return scc;
        }

        public async Task<SendGridCredentials> UpdateSendgridCredentials(string userId, string botModelName, string sendGridAPIKey)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var sgc = new SendGridCredentials { SendGridAPIKey = sendGridAPIKey };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.sendgridcred", sgc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return sgc;
        }

        public async Task<TwilioCredentials> UpdateTwilioCredentials(string userId, string botModelName, string sMSAccountFrom, string sMSAccountIdentification, string sMSAccountPassword)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
            var tc = new TwilioCredentials { SMSAccountFrom = sMSAccountFrom, SMSAccountIdentification = sMSAccountIdentification, SMSAccountPassword = sMSAccountPassword };
            var filter = Builders<BotModel>.Filter.Where(x => x.Name == botModelName && x.userId == userId);
            var update = Builders<BotModel>.Update.Set("serviceConnectivity.twiliocred", tc);
            await collection.FindOneAndUpdateAsync(filter, update);
            return tc;
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
            if (user.apiKey != null)
                updList.Add(Builders<DarlUser>.Update.Set(x => x.APIKey, user.apiKey));
            var update = Builders<DarlUser>.Update.Combine(updList);
            var newUser = await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<DarlUser, DarlUser> { IsUpsert = false, ReturnDocument= ReturnDocument.After });
            return newUser;
        }

        public async Task<ZendeskCredentials> UpdateZendeskCredentials(string userId, string botModelName, string zendeskApiKey, string zendeskURL, string zendeskUser)
        {
            var collection = db.GetCollection<BotModel>("botmodel");
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
                return ctxt.User.Identity.Name ?? _opt.Value.boaiuserid;                   
            }
            return _opt.Value.boaiuserid;
        }

        public async Task<DarlUser> CreateAndProvisionNewUser(DarlUserInput user)
        {
            // create stripe account
            var stripeVals = await CreateStripeCustomer(user.userId, user.InvoiceEmail, false, _opt.Value.StripeTrialPeriodDays, user.InvoiceName);
            // provision account
            await ProvisionUser(user.userId);
            //create user
            var mc = db.GetCollection<DarlUser>("user");
            var duser = new DarlUser { Created = DateTime.Now, current_period_end = DateTime.Now + new TimeSpan(_opt.Value.StripeTrialPeriodDays, 0, 0, 0, 0), InvoiceEmail = user.InvoiceEmail, InvoiceName = user.InvoiceName, InvoiceOrganization = user.InvoiceOrganization, Issuer = user.Issuer, PaidUsageStarted = DateTime.MaxValue, StripeCustomerId = stripeVals.Item1, UsageStripeSubscriptionItem = stripeVals.Item2, userId = user.userId };
            await mc.InsertOneAsync(duser);
            return duser;
        }

        private async Task<(string, string)> CreateStripeCustomer(string userId, string email, bool corporate, int trialPeriodDays, string name = "", string organization = "")
        {
            StripeConfiguration.SetApiKey(_opt.Value.StripeAPIKey);
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
                var items = corporate ?
                    new List<SubscriptionItemOption> { new SubscriptionItemOption { PlanId = _opt.Value.StripeCorporateLicensePlan},
                                                                new SubscriptionItemOption { PlanId = _opt.Value.StripeCorporateUsagePlan }} :
                    new List<SubscriptionItemOption> { new SubscriptionItemOption { PlanId = _opt.Value.StripeIndividualLicensePlan},
                                                                new SubscriptionItemOption { PlanId = _opt.Value.StripeIndividualUsagePlan }};
                var subsoptions = new SubscriptionCreateOptions
                {
                    Items = items,
                    TrialPeriodDays = trialPeriodDays,
                    CustomerId = customer.Id,
                    Billing = Billing.SendInvoice,
                    DaysUntilDue = 14,
                };
                var subsservice = new SubscriptionService();
                Subscription subscription = await subsservice.CreateAsync(subsoptions);
                //extract the subscription item id for the usage so we can register usages with stripe
                string stripeUsageSubsItemId = "";
                foreach (var subitem in subscription.Items)
                {
                    if (subitem.Plan.Id == (corporate ? _opt.Value.StripeCorporateUsagePlan : _opt.Value.StripeIndividualUsagePlan))
                    {
                        stripeUsageSubsItemId = subitem.Id;
                        break;
                    }
                }
                return (customer.Id, stripeUsageSubsItemId);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
            }
            return ("", "");
        }

        private async Task ProvisionUser(string userId)
        {
            //copy just the thousandquestions botmodel from the master account

            var bm = await GetBotModel(_opt.Value.boaiuserid, _opt.Value.ProvisionBotModel);
            await CreateBotModel(userId, _opt.Value.ProvisionBotModel, bm.Model);
            // copy selected rulesets
            foreach (var r in _opt.Value.ProvisionRulesets.Split(','))
            {
                var rs = await GetRuleSet(_opt.Value.boaiuserid, r);
                if(rs != null)
                    await CreateRuleSet(userId, r, rs.Contents, rs.serviceConnectivity);
            }
            foreach (var r in _opt.Value.ProvisionMLModels.Split(','))
            {
                var rs = await GetMlModel(_opt.Value.boaiuserid, r);
                if (rs != null)
                    await CreateMLModel(userId, r, rs.model);
            }
            foreach (var r in _opt.Value.ProvisionCollateral.Split(','))
            {
                var rs = await GetCollateral(_opt.Value.boaiuserid, r);
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
                    throw new ExecutionError($"{ruleSetName} has errors. Check with the online editor");
                }
                var collection = db.GetCollection<RuleSet>("ruleset");
                var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId );
                var update = Builders<RuleSet>.Update.Set(x => x.Contents, rs.Contents);
                await collection.FindOneAndUpdateAsync(filter, update);
            }
            return darl;
        }


        public async Task<List<DarlUser>> GetUsers()
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        /// <summary>
        /// Get the user if the API key matches and the account is valid.
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <returns>the user</returns>
        public async Task<DarlUser> GetUserByApiKey(string apiKey)
        {
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.APIKey == apiKey  && p.accountState != DarlUser.AccountState.suspended && p.accountState != DarlUser.AccountState.closed);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<string> UpdateUserAPIKey(string userId)
        {
            var collection = db.GetCollection<DarlUser>("user");
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
            var mc = db.GetCollection<DarlUser>("user");
            var query = mc.AsQueryable()
            .Where(p => p.StripeCustomerId == stripeId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<string> GetCollateral(string userId, string name)
        {
            var mc = db.GetCollection<Collateral>("collateral");
            var query = mc.AsQueryable()
            .Where(p => p.Name == name && p.userId == userId);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? string.Empty : def.Value;
        }

        public async Task<Collateral> UpdateCollateral(string userId, string name, string value)
        {
            var mc = db.GetCollection<Collateral>("collateral");
            var model = new Collateral { Name = name, Value = value, userId = userId  };
            var filter = Builders<Collateral>.Filter.Where(x => x.Name == name && x.userId == userId);
            var update = Builders<Collateral>.Update.Set("Value", value);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Collateral, Collateral> { IsUpsert = true });
            return model;
        }

        public async Task<Collateral> DeleteCollateral(string userId, string name)
        {
            var mc = db.GetCollection<Collateral>("collateral");
            var query = mc.AsQueryable().Where(p => p.Name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Collateral>.Filter.Eq(r => r.userId, userId) & Builders<Collateral>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<List<Collateral>> GetCollaterals(string userId)
        {
            var mc = db.GetCollection<Collateral>("collateral");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<DateTime> GetLastUpdate(string from, string to)
        {
            var mc = db.GetCollection<Update>("update");
            var query = mc.AsQueryable()
            .Where(p => p.from == from && p.to == to);
            var def = await query.FirstOrDefaultAsync();
            return def == null ? DateTime.MinValue : def.updated;
        }

        public async Task<DateTime> SetLastUpdate(string from, string to)
        {
            var mc = db.GetCollection<Update>("update");
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
                var urlBuilderFactory = new OnlineUrlBuilderFactory(_opt.Value.AzureDevopsAccount);
                var client = VstsClient.Get(urlBuilderFactory, accessToken: _opt.Value.AzureDevopsPersonalAccessToken);
                await client.CreateWorkItemAsync(project, "bug", new WorkItem
                {
                    Fields = new Dictionary<string, string> {
                    {"System.Title", "User reported bug" },
                    {"Microsoft.VSTS.TCM.ReproSteps",  $"Customer {customerName} with email {customerEmail} reported the following: {text}"}
                }
                });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<List<Conversation>> GetConversations()
        {
            var mc = db.GetCollection<Conversation>("conversation");
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<Conversation> CreateConversation(Conversation conversationInput)
        {
            var mc = db.GetCollection<Conversation>("conversation");
            await mc.InsertOneAsync(conversationInput);
            return conversationInput;
        }

        public async Task<UserUsage> CreateUserUsage(DateTime date, int count, string userId)
        {
            var usage = new UserUsage(date, count);
            var collection = db.GetCollection<DarlUser>("user");
            var filter = Builders<DarlUser>.Filter.Where(x => x.userId == userId);
            var update = Builders<DarlUser>.Update.Push("usageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<UserUsage> CreateBotUsage(DateTime date, int count, string userId, string botId)
        {
            var usage = new UserUsage(date, count);
            var collection = db.GetCollection<BotConnection>("botconnection");
            var filter = Builders<BotConnection>.Filter.Where(x => x.AppId == botId && x.userId == userId);
            var update = Builders<BotConnection>.Update.Push("usageHistory", usage);
            await collection.FindOneAndUpdateAsync(filter, update);
            return usage;
        }

        public async Task<BotRuntimeModel> GetBotModelFromAppId(string appId)
        {
            var collection = db.GetCollection<BotConnection>("botconnection");
            var query = collection.AsQueryable()
            .Where(p => p.AppId == appId);
            var botcon =  await query.SingleAsync();
            var mc = db.GetCollection<BotModel>("botmodel");
            var mcquery = mc.AsQueryable()
            .Where(p => p.id == botcon.id);
            var bot = await mcquery.SingleAsync();
            return new BotRuntimeModel { Authorizations = bot.Authorizations, botModelName = bot.Name, Model = bot.Model, password = botcon.Password, serviceConnectivity = bot.serviceConnectivity, userId = bot.userId };
        }

        public async Task<List<BotConnection>> GetBotConnectionsAsync()
        {
            var mc = db.GetCollection<BotConnection>("botconnection");
            var query = mc.AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<string> GetUserIdFromAppId(string appId)
        {
            var collection = db.GetCollection<BotConnection>("botconnection");
            var query = collection.AsQueryable()
            .Where(p => p.AppId == appId);
            var botcon = await query.SingleAsync();
            return botcon.userId;
        }

        public async Task<BotState> GetBotState(string userId, string conversationId)
        {
            var collection = db.GetCollection<BotState>("botstate");
            var query = collection.AsQueryable()
            .Where(p => p.userId == userId && p.conversationId == conversationId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task SaveBotState(BotState bs)
        {
            var collection = db.GetCollection<BotState>("botstate");
            await collection.ReplaceOneAsync( doc => doc.id == bs.id && doc.userId == bs.userId , bs, new UpdateOptions { IsUpsert = true });
        }

        public Task<DarlVar> InteractAsync(string userId, string botModelName, string conversationId, DarlVar conversationData)
        {
            throw new NotImplementedException();
        }

        public async Task CreateDefaultResponse(DefaultResponse response)
        {
            var collection = db.GetCollection<DefaultResponse>("defaultresponse");
            await collection.InsertOneAsync(response);
        }

        public async Task<Document> GetDocument(string userId, string name)
        {
            var mc = db.GetCollection<Document>("document");
            var query = mc.AsQueryable()
            .Where(p => p.name == name && p.userId == userId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Document>> GetDocuments(string userId)
        {
            var mc = db.GetCollection<Document>("document");
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId);
            return await query.ToListAsync();
        }

        public async Task<Document> UpdateDocument(Document document)
        {
            var mc = db.GetCollection<Document>("document");
            var filter = Builders<Document>.Filter.Where(x => x.name == document.name && x.userId == document.userId);
            var update = Builders<Document>.Update.Set("content", document.content);
            await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Document, Document> { IsUpsert = true });
            return document;
        }

        public async Task<Document> DeleteDocument(string userId, string name)
        {
            var mc = db.GetCollection<Document>("document");
            var query = mc.AsQueryable().Where(p => p.name == name && p.userId == userId);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<Document>.Filter.Eq(r => r.userId, userId) & Builders<Document>.Filter.Eq(r => r.name, name));
            return old;
        }

        public async Task<DarlVar> CreateRulesetPreload(string userId, string rulesetName, DarlVar preloadData)
        {
            var collection = db.GetCollection<RuleSet>("ruleset");
            var rs = await GetRuleSet(userId, rulesetName);
            var list = new List<DarlVar>();
            if(rs.Contents.preload != null)
            {
                list.AddRange(rs.Contents.preload);
            }
            list.Add(preloadData);
            if (rs != null)
            {
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
            var collection = db.GetCollection<RuleSet>("ruleset");
            var rs = await GetRuleSet(userId, ruleSetName);
            var filter = Builders<RuleSet>.Filter.Where(x => x.Name == ruleSetName && x.userId == userId);
            if (rs.Contents.trigger == null) //create a new TriggerView and add the data
            {
                var trigg = new TriggerView();
                if (trigger.addressSource != null)
                    trigg.addressSource =  trigger.addressSource ?? DarlCommon.SourceType._fixed;
                if (trigger.addressText != null)
                   trigg.addressText = trigger.addressText;
                if (trigger.attachmentName != null)
                    trigg.attachmentName = trigger.attachmentName;
                if (trigger.attachmentUri != null)
                    trigg.attachmentUri = trigger.attachmentUri;
                if (trigger.bodySource != null)
                    trigg.bodySource = trigger.bodySource ?? DarlCommon.SourceType._fixed;
                if (trigger.bodyText != null)
                   trigg.bodyText = trigger.bodyText;
                if (trigger.emailFrom != null)
                    trigg.emailFrom = trigger.emailFrom;
                if (trigger.postData != null)
                    trigg.postData = trigger.postData;
                if (trigger.postDataSource != null)
                    trigg.postDataSource  = trigger.postDataSource ?? DarlCommon.SourceType._fixed;
                if (trigger.postDataUri != null)
                    trigg.postDataUri = trigger.postDataUri;
                if (trigger.postType != null)
                    trigg.postType = trigger.postType ?? PostType.darlvarlist;
                if (trigger.queueData != null)
                    trigg.queueData = trigger.queueData;
                if (trigger.queueDataSource != null)
                   trigg.queueDataSource = trigger.queueDataSource ?? DarlCommon.SourceType._fixed;
                if (trigger.queueName != null)
                    trigg.queueName = trigger.queueName;
                if (trigger.sendAttachment != null)
                    trigg.sendAttachment = trigger.sendAttachment;
                if (trigger.sendAttachmentSource != null)
                    trigg.sendAttachmentSource = trigger.sendAttachmentSource ?? DarlCommon.SourceType._fixed;
                if (trigger.sendBug != null)
                    trigg.sendBug = trigger.sendBug;
                if (trigger.sendBugSource != null)
                    trigg.sendBugSource = trigger.sendBugSource ?? DarlCommon.SourceType._fixed;
                if (trigger.sendEmail != null)
                    trigg.sendEmail = trigger.sendEmail;
                if (trigger.sendEmailSource != null)
                    trigg.sendEmailSource = trigger.sendEmailSource ?? DarlCommon.SourceType._fixed;
                if (trigger.subjectSource != null)
                    trigg.subjectSource = trigger.subjectSource ?? DarlCommon.SourceType._fixed;
                if (trigger.subjectText != null)
                    trigg.subjectText = trigger.subjectText;
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
                        await CreateBotModel(_opt.Value.boaiuserid, destName, bm.Model);
                    }
                    break;
                case ResourceType.ruleset:
                    {
                        var rs = await GetRuleSet(userId, name);
                        if (rs == null)
                        {
                            throw new ExecutionError($"Ruleset {name} doesn't exist in account {userId}");
                        }
                        await CreateRuleSet(_opt.Value.boaiuserid, destName, rs.Contents, new ServiceConnectivity());
                    }
                    break;
                case ResourceType.mlmodel:
                    {
                        var ml = await GetMlModel(userId, name);
                        if (ml == null)
                        {
                            throw new ExecutionError($"MLModel {name} doesn't exist in account {userId}");
                        }
                        await CreateMLModel(_opt.Value.boaiuserid, destName, ml.model);
                    }
                    break;
                case ResourceType.document:
                    {
                        var doc = await GetDocument(userId, name);
                        if (doc == null)
                        {
                            throw new ExecutionError($"Document {name} doesn't exist in account {userId}");
                        }
                        doc.userId = _opt.Value.boaiuserid;
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
                        await UpdateCollateral(_opt.Value.boaiuserid, destName, coll);
                    }
                    break;

            }
            return destName;
        }
    }
}