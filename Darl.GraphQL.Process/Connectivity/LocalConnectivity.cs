using Darl.Common;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Thinkbase;
using DarlCommon;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class LocalConnectivity : IConnectivity
    {
        private IConfiguration _config;
        private ILogger _logger;

        private LiteDatabase db;

        public static readonly string knowledgestateCollection = "kstate";
        public static readonly string kgraphcollection = "kgraph";


        public LocalConnectivity(IConfiguration config, ILogger<LocalConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            db = new LiteDatabase(_config["LOCALDATABASEPATH"]);
            var mapper = BsonMapper.Global;
            mapper.Entity<KGraph>().Id(x => x.Id);
            mapper.Entity<DarlTime>().Ignore(x => x.dateTime).Ignore(x => x.dateTimeOffset).Ignore(x => x.year).Ignore(x => x.season);
        }
        public Task<KGraph> CreateKGraph(string userId, string name)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            var model = new KGraph { Name = name, userId = userId };
            mc.Insert(model);
            return Task.FromResult(model);
        }

        public Task<KnowledgeState> CreateKnowledgeState(string userId, KnowledgeStateInput state)
        {
            var kstate = new KnowledgeState { knowledgeGraphName = state.knowledgeGraphName, subjectId = state.subjectId, userId = userId, created = DateTime.UtcNow };
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
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            mc.EnsureIndex(x => x.subjectId);
            mc.EnsureIndex(x => x.knowledgeGraphName);
            //ensure user/graphName/subjectId combination is unique
            mc.DeleteMany(x => x.subjectId == state.subjectId && x.knowledgeGraphName == state.knowledgeGraphName);
            mc.Insert(kstate);
            return Task.FromResult(kstate);
        }

        public Task<long> DeleteAllKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var ds = mc.DeleteMany(x => x.knowledgeGraphName == graphName);
            return Task.FromResult<long>(ds);
        }

        public Task<KGraph> DeleteKGraph(string userId, string name)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            mc.EnsureIndex(x => x.Name);
            var old = mc.FindOne(x => x.Name == name);
            mc.DeleteMany(x =>x.Name == name);
            return Task.FromResult(old);
        }

        public Task<KnowledgeState> DeleteKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            var old = mc.FindOne(x => x.subjectId == ksId && x.knowledgeGraphName == graphName); ;
            mc.DeleteMany(x => x.subjectId == ksId && x.knowledgeGraphName == graphName);
            return Task.FromResult(old);
        }

        public Task<KGraph> GetKGModel(string userId, string model)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            return Task.FromResult(mc.FindOne(x => x.Name == model));
        }

        public Task<int> GetKGraphCountAsync(string userId)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            return Task.FromResult(mc.Count());
        }

        public Task<List<KGraph>> GetKGraphsAsync(string userId)
        {
            var mc = db.GetCollection<KGraph>(kgraphcollection);
            return Task.FromResult(mc.FindAll().ToList());
        }

        public Task<KnowledgeState> GetKnowledgeState(string userId, string ksId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            return Task.FromResult(mc.FindOne(x => x.subjectId == ksId && x.knowledgeGraphName == graphName));
        }

        public Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            throw new NotImplementedException();
        }

        public Task<List<KnowledgeState>> GetKnowledgeStates(string userId, string graphName)
        {
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            return Task.FromResult(mc.Find(x => x.knowledgeGraphName == graphName).ToList());
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
            throw new NotImplementedException();
        }

        public Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            throw new NotImplementedException();
        }

        public Task<KGraph> UpdateKGraph(string userId, string name, KGraphUpdate kgupdate)
        {
            var existing = GetKGModel(userId, name).Result;
            if (existing == null)
                return Task.FromResult<KGraph>(null);
            var collection = db.GetCollection<KGraph>(kgraphcollection);
            if (kgupdate.Description != null)
                existing.Description=  kgupdate.Description;
            if (kgupdate.ReadOnly != null)
                existing.ReadOnly = kgupdate.ReadOnly;
            if (kgupdate.dateDisplay != null)
                existing.dateDisplay = kgupdate.dateDisplay;
            if (kgupdate.inferenceTime != null)
                existing.inferenceTime = kgupdate.inferenceTime;
            if (kgupdate.fixedTime != null)
                existing.fixedTime = kgupdate.fixedTime;
            if (kgupdate.InitialText != null)
                existing.InitialText = kgupdate.InitialText;
            if (kgupdate.hidden != null)
                existing.hidden = kgupdate.hidden;
            collection.Update(existing);
            return Task.FromResult(existing);
        }

        public Task<KnowledgeState> UpdateKnowledgeState(string userId, string ksId, KnowledgeStateUpdate state)
        {
            var ks = GetKnowledgeState(userId, ksId, state.knowledgeGraphName).Result;
            if(ks != null)
            {
                if (state.data != null)
                {
                    ks.data = state.data;
                }
            }
            var mc = db.GetCollection<KnowledgeState>(knowledgestateCollection);
            mc.Update(ks);
            return Task.FromResult(ks);
        }
    }
}
