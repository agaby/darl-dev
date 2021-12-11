using Darl.Common;
using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class LocalConnectivity : IConnectivity
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private readonly LiteDatabase db;

        public static readonly string knowledgestateCollection = "kstate";
        public static readonly string kgraphcollection = "kgraph";


        public LocalConnectivity(IConfiguration config, ILogger<LocalConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            db = new LiteDatabase(_config["LOCALDATABASEPATH"]);
            db.UtcDate = true;
            var mapper = BsonMapper.Global;
            mapper.Entity<LiteKGraph>().Id(x => x.Id);
            mapper.Entity<DarlTime>().Ignore(x => x.dateTime).Ignore(x => x.dateTimeOffset).Ignore(x => x.year).Ignore(x => x.season);
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            mc.EnsureIndex(x => x.subjectId);
            mc.EnsureIndex(x => x.knowledgeGraphName);
        }
        public Task<KGraph> CreateKGraph(string userId, string name)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            var model = new LiteKGraph { Name = name, userId = userId };
            mc.Insert(model);
            return Task.FromResult(model as KGraph);
        }

        public Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state)
        {
            var kstate = new LiteKnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = state.subjectId, userId = userId, created = DateTime.UtcNow };
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
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            mc.DeleteMany(x => x.subjectId == state.subjectId && x.knowledgeGraphName == state.knowledgeGraphName);
            mc.Insert(kstate);
            return Task.FromResult(kstate as KnowledgeState);
        }

        public Task<long> DeleteAllKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            var ds = mc.DeleteMany(x => x.knowledgeGraphName == graphName);
            return Task.FromResult<long>(ds);
        }

        public Task<KGraph> DeleteKGraph(string userId, string name)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            mc.EnsureIndex(x => x.Name);
            var old = mc.FindOne(x => x.Name == name);
            mc.DeleteMany(x => x.Name == name);
            return Task.FromResult(old as KGraph);
        }

        public Task<KnowledgeState> DeleteKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            var old = mc.FindOne(x => x.subjectId == ksId && x.knowledgeGraphName == graphName); ;
            mc.DeleteMany(x => x.subjectId == ksId && x.knowledgeGraphName == graphName);
            return Task.FromResult(old as KnowledgeState);
        }

        public Task<KGraph> GetKGModel(string userId, string model)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            return Task.FromResult(mc.FindOne(x => x.Name == model) as KGraph);
        }

        public Task<int> GetKGraphCountAsync(string userId)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            return Task.FromResult(mc.Count());
        }

        public Task<List<KGraph>> GetKGraphsAsync(string userId)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            return Task.FromResult(mc.FindAll().ToList<KGraph>());
        }

        public Task<KnowledgeState> GetKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            return Task.FromResult(mc.FindOne(x => x.subjectId == ksId && x.knowledgeGraphName == graphName) as KnowledgeState);
        }

        public Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            return Task.FromResult(mc.Find(x => x.knowledgeGraphName == graphName).ToList<KnowledgeState>());
        }

        public Task<IAsyncCursor<KnowledgeState>> GetKnowledgeStatesBatched(string userId, string graphName)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName)
        {
            var list = new List<KnowledgeState>();
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            foreach(var k in ksIds)
            {
                var r  = mc.FindOne(x => x.subjectId == k && x.knowledgeGraphName == graphName) as KnowledgeState;
                if(r != null)
                    list.Add(r);
            }
            return Task.FromResult(list);
        }
        /// <summary>
        /// Handle a set of requests that may combine GraphObjects and KnowledgeStates mixed in.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ksIds"></param>
        /// <param name="graphName"></param>
        /// <param name="notFound">ids not found, probably graphObjects</param>
        /// <returns></returns>
        public Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName, List<string> notFound)
        {
            var list = new List<GraphAbstraction>();
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            foreach (var k in ksIds)
            {
                var r = mc.FindOne(x => x.subjectId == k && x.knowledgeGraphName == graphName) as KnowledgeState;
                if (r != null)
                    list.Add(r);
                else
                    notFound.Add(k);
            }
            return Task.FromResult(list);

        }

        public Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            throw new NotImplementedException();
        }

        public Task<KGraph> UpdateKGraph(string userId, string name, KGraphUpdate kgupdate)
        {
            var mc = db.GetCollection<LiteKGraph>(kgraphcollection);
            var existing = mc.FindOne(x => x.Name == name);
            if (existing == null)
                return Task.FromResult<KGraph>(null);
            if (kgupdate.ReadOnly != null)
                existing.ReadOnly = kgupdate.ReadOnly;
            if (kgupdate.hidden != null)
                existing.hidden = kgupdate.hidden;
            mc.Update(existing);
            return Task.FromResult(existing as KGraph);
        }

        public Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state)
        {
            var mc = db.GetCollection<LiteKnowledgeState>(knowledgestateCollection);
            var ks = mc.FindOne(x => x.subjectId == ksId && x.knowledgeGraphName == state.knowledgeGraphName);
            if (ks == null)
            {
                ks = new LiteKnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = ksId, userId = userId, created = DateTime.UtcNow };
            }
            if (state.data != null)
            {
                ks.data = state.data;
            }
            mc.Upsert(ks);
            return Task.FromResult(ks as KnowledgeState);
        }
    }
}
