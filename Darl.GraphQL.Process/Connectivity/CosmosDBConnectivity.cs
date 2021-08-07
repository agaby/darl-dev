using Darl.Forms;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
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

namespace Darl.GraphQL.Models.Connectivity
{
    public class CosmosDBConnectivity : IConnectivity
    {
        private IConfiguration _config;
        private ILicensing _licensing;
        public IMongoDatabase db { get; set; }
        private MongoClient mongoClient;
        private DarlRunTime runtime = new DarlRunTime();
        private DarlMetaRunTime metaRuntime = new DarlMetaRunTime(new MetaStructureHandler());
        private IDistributedCache _cache;
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
        public static readonly string knowledgestateCollection = "kstate";
        public static readonly string kgraphcollection = "kgraph";


        public CosmosDBConnectivity(IConfiguration config, ILogger<CosmosDBConnectivity> logger, ILicensing licensing, IDistributedCache cache)
        {
            _config = config;
            _logger = logger;
            _licensing = licensing;
            _cache = cache;
            string connectionString = _config["AppSettings:MongoConnectionString"];
            backgroundUserId = _config["AppSettings:boaiuserid"];
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            mongoClient = new MongoClient(settings);
            db = mongoClient.GetDatabase(_config["AppSettings:MongoDatabase"]);
            BsonClassMap.RegisterClassMap<DarlVar>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.categories).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, double>>(DictionaryRepresentation.ArrayOfDocuments));
            });

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

        private async Task<List<DarlLanguage.Processing.DarlResult>> ProcessValues(List<DarlLanguage.Processing.DarlResult> Values, ParseTree tree)
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


        public async Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state)
        {
            var kstate = new KnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = state.subjectId, userId = userId, created = DateTime.UtcNow };
            foreach(var s in state.data)
            {
                if(!kstate.data.ContainsKey(s.name))
                {
                    kstate.data.Add(s.name, new List<GraphAttribute>());
                    foreach (var g in s.value)
                    {
                        kstate.data[s.name].Add(new GraphAttribute { confidence = g.confidence ?? 1.0, existence = g.existence, id = Guid.NewGuid().ToString(), inferred = g.inferred ?? false, lineage = g.lineage, name = g.name, value = g.value, type = g.type });
                    }
                }
            }
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            //ensure user/graphName/subjectId combination is unique
            await mc.DeleteManyAsync(Builders<KnowledgeState>.Filter.Eq(r => r.userId, userId) & Builders<KnowledgeState>.Filter.Eq(r => r.subjectId, state.subjectId) & Builders<KnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, state.knowledgeGraphName));
            await mc.InsertOneAsync(kstate);
            return kstate;
        }

        public async Task<KnowledgeState> GetKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.subjectId == ksId && p.userId == userId && p.knowledgeGraphName == graphName);
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get a set of KSs by subjectId
        /// </summary>
        /// <param name="userId">The userId</param>
        /// <param name="ksIds">the list of subject Ids</param>
        /// <param name="graphName">the graph name</param>
        /// <returns>a list of KSs</returns>
        /// <remarks>Not paged, so limited to a single MongoDB page.</remarks>
        public async Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var filter1 = Builders<KnowledgeState>.Filter.In(x => x.subjectId,ksIds);
            var filter2 = Builders<KnowledgeState>.Filter.Where(x => x.knowledgeGraphName == graphName && x.userId == userId);
            var filter3 = Builders<KnowledgeState>.Filter.And(filter1, filter2);
            var cursor = await mc.FindAsync<KnowledgeState>(filter3);
            await cursor.MoveNextAsync();
            return cursor.Current.ToList();
        }

        /// <summary>
        /// Returns an IAsyncCursor<<KnowledgeState>> you can iterate to get all the states
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        public async Task<IAsyncCursor<KnowledgeState>> GetKnowledgeStatesBatched(string userId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var filter = Builders<KnowledgeState>.Filter.Where(x => x.knowledgeGraphName == graphName && x.userId == userId);
            return  await mc.FindAsync<KnowledgeState>(filter);
        }

        public async Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage && a.value == attValue));
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage && a.value == attValue));
            return await query.ToListAsync();
        }


        public async Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId));
            return await query.ToListAsync();
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Update the data field in the KnowledgeState
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state)
        {
            var updList = new List<UpdateDefinition<KnowledgeState>>();
            if (!string.IsNullOrEmpty(state.knowledgeGraphName))
                updList.Add(Builders<KnowledgeState>.Update.Set("knowledgeGraphName", state.knowledgeGraphName));
            if (state.data != null)
                updList.Add(Builders<KnowledgeState>.Update.Set("data", state.data));
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var filter = Builders<KnowledgeState>.Filter.Where(x => x.subjectId == ksId && x.userId == userId);
            var update = Builders<KnowledgeState>.Update.Combine(updList);
            var updated = await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<KnowledgeState, KnowledgeState> { IsUpsert = true });
            return updated;
        }

        public async Task<KnowledgeState?> UpdateKnowledgeStateAttribute(string userId, string ksId, KnowledgeStateUpdate state, string dataId, string attLineage, string attvalue)
        {
            var updList = new List<UpdateDefinition<KnowledgeState>>();
            updList.Add(Builders<KnowledgeState>.Update.Set("value", attvalue));
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var filter = Builders<KnowledgeState>.Filter.Where(p => p.userId == userId && p.knowledgeGraphName == state.knowledgeGraphName && p.data.ContainsKey(dataId) && p.data[dataId].Any(a => a.lineage == attLineage ));
            var update = Builders<KnowledgeState>.Update.Combine(updList);
            var updated = await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<KnowledgeState, KnowledgeState> { IsUpsert = true });
            return updated;
        }

        public async Task<KnowledgeState> DeleteKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable().Where(p => p.subjectId == ksId && p.userId == userId && p.knowledgeGraphName == graphName);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteManyAsync(Builders<KnowledgeState>.Filter.Eq(r => r.userId, userId) & Builders<KnowledgeState>.Filter.Eq(r => r.subjectId, ksId) & Builders<KnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, graphName));
            return old;
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
                        var res = await ProcessValues(Lineage.Bot.DarlVarExtensions.Convert(DarlVarInput.Convert(inputs)), tree);
                        _logger.LogWarning($"{nameof(InferFromDarlDarlVar)}: {userId}");

                    return Lineage.Bot.DarlVarExtensions.Convert(res);
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


        private Task ReportLicense(string email, string company, string key, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckKey(string userId, string key)
        {
            return _licensing.CheckKey(key);
        }


        public async Task<List<KGraph>> GetKGraphsAsync(string userId)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && !(p.hidden == true));
            return await query.ToListAsync();
        }

        public async Task<int> GetKGraphCountAsync(string userId)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && !(p.hidden == true));
            return await query.CountAsync();
        }

        public async Task<KGraph> GetKGModel(string userId, string model)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == model);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<KGraph> CreateKGraph(string userId, string name)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var model = new KGraph { Name = name, userId = userId};
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<KGraph?> UpdateKGraph(string userId, string name, KGraphUpdate kgupdate)
        {
            var existing = await GetKGModel(userId, name);
            if (existing == null)
                return existing;
            var collection = db.GetCollection<KGraph>(kgraphcollection);
            var filter = Builders<KGraph>.Filter.Where(x => x.userId == userId && x.Name == name);
            var updList = new List<UpdateDefinition<KGraph>>();
            if (kgupdate.Description != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.Description, kgupdate.Description));
            if (kgupdate.ReadOnly != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.ReadOnly, kgupdate.ReadOnly));
            if (kgupdate.dateDisplay != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.dateDisplay, kgupdate.dateDisplay));
            if (kgupdate.inferenceTime != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.inferenceTime, kgupdate.inferenceTime));
            if (kgupdate.fixedTime != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.fixedTime, kgupdate.fixedTime));
            if (kgupdate.InitialText != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.InitialText, kgupdate.InitialText));
            if (kgupdate.hidden != null)
                updList.Add(Builders<KGraph>.Update.Set(x => x.hidden, kgupdate.hidden));
            var update = Builders<KGraph>.Update.Combine(updList);
            return await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<KGraph, KGraph> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
        }

        public Task<List<DarlLintView>> LintDarlMeta(string darl)
        {
            var errorList = new List<DarlLintView>();
            int rowoffset = 0;
            int coloffset = 0;
            if (!string.IsNullOrEmpty(darl))
            {
                try
                {
                    var tree = metaRuntime.CreateTreeEdit(darl);
                    if (tree.HasErrors())
                    {
                        foreach (var pm in tree.ParserMessages)
                        {
                            errorList.Add(new DarlLintView { line_no = pm.Location.Line + 1 - rowoffset, column_no_start = pm.Location.Column + 1 - coloffset, column_no_stop = pm.Location.Column + 2 - coloffset, message = pm.Message, severity = pm.Level == ErrorLevel.Error ? "error" : "warning" });
                        }
                    }
                }
                catch(Exception ex)
                {

                }
 
            }
            return Task.FromResult(errorList);
        }



        public async Task<KGraph> DeleteKGraph(string userId, string name)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == name);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<KGraph>.Filter.Eq(r => r.userId, userId) & Builders<KGraph>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage));
            return await query.ToListAsync();
        }

        public async Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            var model = await GetKGModel(userId, name);
            if (model == null)
                return "failed";
            //create a record with sharerId as userId and Shared set.
            var kg = new KGraph { Name = name, OwnerId = userId, userId = sharerId, Shared = true, ReadOnly = readOnly, Description = model.Description, hidden = hidden };
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            await mc.InsertOneAsync(kg);
            return "success";
        }

        public async Task<long> DeleteAllKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var ds = await mc.DeleteManyAsync(Builders<KnowledgeState>.Filter.Eq(r => r.userId, userId) & Builders<KnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, graphName));
            return ds.DeletedCount;
        }
    }
}