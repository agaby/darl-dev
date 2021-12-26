using Darl.Common;
using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using DarlCommon;
using DarlCompiler;
using DarlCompiler.Parsing;
using DarlLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class CosmosDBConnectivity : IConnectivity
    {
        private readonly IConfiguration _config;
        public IMongoDatabase db { get; set; }
        private readonly MongoClient mongoClient;
        private readonly DarlRunTime runtime = new DarlRunTime();
        private readonly ILogger _logger;
        private readonly string backgroundUserId;

        public static readonly string knowledgestateCollection = "kstate";
        public static readonly string kgraphcollection = "kgraph";


        public CosmosDBConnectivity(IConfiguration config, ILogger<CosmosDBConnectivity> logger)
        {
            _config = config;
            _logger = logger;

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
            BsonClassMap.RegisterClassMap<DarlTime>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(m => m.dateTime);
                cm.UnmapMember(m => m.dateTimeOffset);
                cm.UnmapMember(m => m.year);
                cm.UnmapMember(m => m.season);
            });
            BsonClassMap.RegisterClassMap<CosmosKGraph>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });


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

        public async Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state)
        {
            var kstate = new CosmosKnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = state.subjectId, userId = userId, created = DateTime.UtcNow };
            foreach (var s in state.data)
            {
                if (!kstate.data.ContainsKey(s.name))
                {
                    kstate.data.Add(s.name, new List<GraphAttribute>());
                    foreach (var g in s.value)
                    {
                        kstate.data[s.name].Add(new GraphAttribute { confidence = g.confidence ?? 1.0, existence = g.existence, id = Guid.NewGuid().ToString(), inferred = g.inferred ?? false, lineage = g.lineage, name = g.name, value = g.value, type = g.type });
                    }
                }
            }
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            //ensure user/graphName/subjectId combination is unique
            await mc.DeleteManyAsync(Builders<CosmosKnowledgeState>.Filter.Eq(r => r.userId, userId) & Builders<CosmosKnowledgeState>.Filter.Eq(r => r.subjectId, state.subjectId) & Builders<CosmosKnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, state.knowledgeGraphName));
            await mc.InsertOneAsync(kstate);
            return kstate;
        }

        public async Task<KnowledgeState> GetKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
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
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter1 = Builders<CosmosKnowledgeState>.Filter.In(x => x.subjectId, ksIds);
            var filter2 = Builders<CosmosKnowledgeState>.Filter.Where(x => x.knowledgeGraphName == graphName && x.userId == userId);
            var filter3 = Builders<CosmosKnowledgeState>.Filter.And(filter1, filter2);
            var cursor = await mc.FindAsync<CosmosKnowledgeState>(filter3);
            await cursor.MoveNextAsync();
            return cursor.Current.ToList<KnowledgeState>();
        }

        /// <summary>
        /// Handle a set of requests that may combine GraphObjects and KnowledgeStates mixed in.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ksIds"></param>
        /// <param name="graphName"></param>
        /// <param name="notFound">ids not found, probably graphObjects</param>
        /// <returns></returns>
        public async Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName, List<string> notFound)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter1 = Builders<CosmosKnowledgeState>.Filter.In(x => x.subjectId, ksIds);
            var filter2 = Builders<CosmosKnowledgeState>.Filter.Where(x => x.knowledgeGraphName == graphName && x.userId == userId);
            var filter3 = Builders<CosmosKnowledgeState>.Filter.And(filter1, filter2);
            var cursor = await mc.FindAsync<CosmosKnowledgeState>(filter3);
            await cursor.MoveNextAsync();
            var res =  cursor.Current.ToList<KnowledgeState>();
            foreach(var r in ksIds)
            {
                if(!res.Any(a => a.subjectId == r))
                    notFound.Add(r);
            }
            var list = new List<GraphAbstraction>();
            foreach(var rs in res)
            {
                list.Add(rs);
            }
            return list;
        }

        /// <summary>
        /// Returns an IAsyncCursor<<KnowledgeState>> you can iterate to get all the states
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        public async Task<IAsyncCursor<KnowledgeState>> GetKnowledgeStatesBatched(string userId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter = Builders<CosmosKnowledgeState>.Filter.Where(x => x.knowledgeGraphName == graphName && x.userId == userId);
            return await mc.FindAsync<CosmosKnowledgeState>(filter);
        }

        public async Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage && a.value == attValue));
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage && a.value == attValue));
            var states = await query.ToListAsync();
            return states.ToList<KnowledgeState>();
        }


        public async Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId));
            var states = await query.ToListAsync();
            return states.ToList<KnowledgeState>();
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName);
            var states = await query.ToListAsync();
            return states.ToList<KnowledgeState>();
        }

        /// <summary>
        /// Update the data field in the KnowledgeState
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state)
        {
            var updList = new List<UpdateDefinition<CosmosKnowledgeState>>();
            if (!string.IsNullOrEmpty(state.knowledgeGraphName))
                updList.Add(Builders<CosmosKnowledgeState>.Update.Set("knowledgeGraphName", state.knowledgeGraphName));
            if (state.data != null)
                updList.Add(Builders<CosmosKnowledgeState>.Update.Set("data", state.data));
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter = Builders<CosmosKnowledgeState>.Filter.Where(x => x.subjectId == ksId && x.userId == userId);
            var update = Builders<CosmosKnowledgeState>.Update.Combine(updList);
            var updated = await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<CosmosKnowledgeState, CosmosKnowledgeState> { IsUpsert = true });
            return updated;
        }

        public async Task<KnowledgeState?> UpdateKnowledgeStateAttribute(string userId, string ksId, KnowledgeStateUpdate state, string dataId, string attLineage, string attvalue)
        {
            var updList = new List<UpdateDefinition<CosmosKnowledgeState>>();
            updList.Add(Builders<CosmosKnowledgeState>.Update.Set("value", attvalue));
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter = Builders<CosmosKnowledgeState>.Filter.Where(p => p.userId == userId && p.knowledgeGraphName == state.knowledgeGraphName && p.data.ContainsKey(dataId) && p.data[dataId].Any(a => a.lineage == attLineage));
            var update = Builders<CosmosKnowledgeState>.Update.Combine(updList);
            var updated = await mc.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<CosmosKnowledgeState, CosmosKnowledgeState> { IsUpsert = true });
            return updated;
        }

        public async Task<KnowledgeState> DeleteKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable().Where(p => p.subjectId == ksId && p.userId == userId && p.knowledgeGraphName == graphName);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteManyAsync(Builders<CosmosKnowledgeState>.Filter.Eq(r => r.userId, userId) & Builders<CosmosKnowledgeState>.Filter.Eq(r => r.subjectId, ksId) & Builders<CosmosKnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, graphName));
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

        public async Task<List<KGraph>> GetKGraphsAsync(string userId)
        {
            var mc = db.GetCollection<CosmosKGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && !(p.hidden == true));
            var graphs = await query.ToListAsync();
            return graphs.OrderBy(a => a.Name).ToList<KGraph>();
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
            var mc = db.GetCollection<CosmosKGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == model);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<KGraph> CreateKGraph(string userId, string name)
        {
            var mc = db.GetCollection<CosmosKGraph>(kgraphcollection);
            var model = new CosmosKGraph { Name = name, userId = userId, };
            await mc.InsertOneAsync(model);
            return model;
        }

        public async Task<KGraph?> UpdateKGraph(string userId, string name, KGraphUpdate kgupdate)
        {
            var existing = await GetKGModel(userId, name);
            if (existing == null)
                return existing;
            var collection = db.GetCollection<CosmosKGraph>(kgraphcollection);
            var filter = Builders<CosmosKGraph>.Filter.Where(x => x.userId == userId && x.Name == name);
            var updList = new List<UpdateDefinition<CosmosKGraph>>();
            if (kgupdate.ReadOnly != null)
                updList.Add(Builders<CosmosKGraph>.Update.Set(x => x.ReadOnly, kgupdate.ReadOnly));
            if (kgupdate.hidden != null)
                updList.Add(Builders<CosmosKGraph>.Update.Set(x => x.hidden, kgupdate.hidden));
            var update = Builders<CosmosKGraph>.Update.Combine(updList);
            return await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<CosmosKGraph, CosmosKGraph> { IsUpsert = false, ReturnDocument = ReturnDocument.After });
        }

        public async Task<KGraph> DeleteKGraph(string userId, string name)
        {
            var mc = db.GetCollection<CosmosKGraph>(kgraphcollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.Name == name);
            var old = await query.FirstOrDefaultAsync();
            await mc.DeleteOneAsync(Builders<CosmosKGraph>.Filter.Eq(r => r.userId, userId) & Builders<CosmosKGraph>.Filter.Eq(r => r.Name, name));
            return old;
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var query = mc.AsQueryable()
            .Where(p => p.userId == userId && p.knowledgeGraphName == graphName && p.data.ContainsKey(objectId) && p.data[objectId].Any(a => a.lineage == attLineage));
            var states = await query.ToListAsync();
            return states.ToList<KnowledgeState>();
        }

        public async Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            var model = await GetKGModel(userId, name);
            if (model == null)
                return "failed";
            //create a record with sharerId as userId and Shared set.
            var kg = new CosmosKGraph { Name = name, OwnerId = userId, userId = sharerId, Shared = true, ReadOnly = readOnly, hidden = hidden };
            var mc = db.GetCollection<CosmosKGraph>(kgraphcollection);
            await mc.InsertOneAsync(kg);
            return "success";
        }

        public async Task<long> DeleteAllKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<CosmosKnowledgeState>(knowledgestateCollection);
            var filter = Builders<CosmosKnowledgeState>.Filter
                .And(
                    Builders<CosmosKnowledgeState>.Filter.Eq(r => r.userId, userId),
                    Builders<CosmosKnowledgeState>.Filter.Eq(r => r.knowledgeGraphName, graphName)
                    );
                var count = await mc.Find(filter).CountDocumentsAsync();
            var ds = await mc.DeleteManyAsync(filter);
            return ds.DeletedCount;
        }

    }
}