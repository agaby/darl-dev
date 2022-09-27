using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using GraphQL;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    [ProtoContract]
    public class BlobGraphPrimitives : IGraphPrimitives
    {

        private readonly IBlobConnectivity _blob;
        private readonly IDistributedCache _cache;
        private readonly IConnectivity _conn;
        private readonly ILogger _logger;
        private readonly ILicensing _license;
        private readonly IMemoryCache _localCache;
        private readonly IConfiguration _config;

        private readonly int modelLicenseDays = 1000;
        private int kgCacheMinutes = 30;




        private readonly Object lockObject = new object();

        private readonly HashSet<string> modified = new HashSet<string>();


        private bool modifiedToggle = false;

        public BlobGraphPrimitives(IBlobConnectivity blob, IDistributedCache cache, IConnectivity conn, ILogger<BlobGraphPrimitives> logger, ILicensing license, IMemoryCache localCache, IConfiguration config)
        {
            _blob = blob;
            _cache = cache;
            _conn = conn;
            _logger = logger;
            _license = license;
            _localCache = localCache;
            _config = config;
            //add BackOfficeKG as permanently loaded model
            LoadBackOfficeKG().Wait();
        }

        private async Task LoadBackOfficeKG()
        {
            var blobName = _config["AppSettings:boaiuserid"] + "_" + _config["AppSettings:BackOfficeKG"];
            var sharedState = await HandleSharedNames(blobName);
            if (!String.IsNullOrEmpty(sharedState.Item1))
            {
                var data = await _blob.Read(sharedState.Item1);
                var model = DeserializeGraph(data);
                model.SanityCheck();
                _localCache.Set(blobName, model);
            }
        }


        /// <summary>
        /// Updates a real object
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="go"></param>
        /// <returns>The modified object</returns>
        /// <remarks>Merges rather than overwriting properties</remarks>
 

        public async Task Store(string blobName, IGraphModel model)
        {
            if (model is BlobGraphContent cont)
            {
                cont.key = _license.CreateKey(DateTime.UtcNow + new TimeSpan(modelLicenseDays, 0, 0, 0), blobName, "");
                await _blob.Write(blobName, SerializeGraph(cont));
            }
        }

        public async Task<IGraphModel?> Load(string blobName)
        {
            if (_localCache.TryGetValue(blobName, out IGraphModel model))
            {
                return model;
            }
            if (await _blob.Exists(blobName))
            {
                try
                {
                    var data = await _blob.Read(blobName);
                    model = DeserializeGraph(data);
                    model.SanityCheck();
                    _localCache.Set(blobName, model, TimeSpan.FromMinutes(kgCacheMinutes));
                    return model;
                }
                catch (Exception ex)
                {
                    throw new ExecutionError($"Error loading {blobName}: {ex.Message}");
                }
            }
            else
            {
                //check if it's a shared blob
                var sharedState = await HandleSharedNames(blobName);
                if (!String.IsNullOrEmpty(sharedState.Item1))
                {
                    try
                    {
                        var data = await _blob.Read(sharedState.Item1);
                        model = DeserializeGraph(data);
                        model.SanityCheck();
                        _localCache.Set(blobName, model, TimeSpan.FromMinutes(kgCacheMinutes));
                        return model;
                    }
                    catch (Exception ex)
                    {
                        throw new ExecutionError($"Error loading shared graph {blobName}: {ex.Message}");
                    }
                }
/*                else
                {
                    var model = new BlobGraphContent();
                    buffer.Add(blobName, model);
                    return model;
                }*/
            }
            return null;
        }

        public static byte[] SerializeGraph(IGraphModel model)
        {
            if (model is BlobGraphContent cont)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize<BlobGraphContent>(ms, cont);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
            return new byte[0];
        }

        public BlobGraphContent DeserializeGraph(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                return Serializer.Deserialize<BlobGraphContent>(ms);
            }
        }

        public async Task<bool> Exists(string compositeName)
        {
            return await _blob.Exists(compositeName);
        }

        public async Task<bool> CreateModel(string compositeName)
        {
            var record = await GetRecord(compositeName);
            if (record != null)
                throw new ExecutionError($"{record.Name} already exists.");
            var divider = compositeName.IndexOf('_');
            var id = compositeName.Substring(0, divider);
            var name = compositeName[(divider + 1)..];
            await _conn.CreateKGraph(id, name);
            var newGraph = new BlobGraphContent();
            newGraph.AddDefaultContent();
            await _blob.Write(compositeName, SerializeGraph(newGraph));
            return true;
        }


        public async Task<bool> DeleteModel(string compositeName)
        {
            //if shared, delete the share but not the blob
            var record = await GetRecord(compositeName);
            if (record == null)
            {
                return false;
            }
            if (record.ReadOnly ?? false)
            {
                throw new ExecutionError("The owner of this KG has not permitted deletion.");
            }
            await _conn.DeleteKGraph(record.userId, record.Name);
            if (record.Shared)
            {
                return true;
            }
            _localCache.Remove(compositeName);
            return await _blob.Delete(compositeName);
        }

        public async Task<List<string>> ListModels(string userId)
        {
            return _blob.List(userId);
        }

        /// <summary>
        /// writes out any modified graph models to the blob storage
        /// </summary>
        /// <param name="stateInfo"></param>

        public async Task Store(string compositeName)
        {
            var sharedState = await HandleSharedNames(compositeName);
            if (sharedState.Item2)
                throw new ExecutionError("The owner of this KG has not permitted saving of edits.");
            if (!String.IsNullOrEmpty(sharedState.Item1))
            {
                modified.Remove(sharedState.Item1);
                if(_localCache.TryGetValue(compositeName, out IGraphModel model))
                {
                    byte[] data;
                    data = SerializeGraph(model);
                    await _blob.Write(sharedState.Item1, data);
                    _localCache.Set(compositeName, model, TimeSpan.FromMinutes(kgCacheMinutes));

                }
                else
                {
                    throw new ExecutionError("This KG was not found in the cache.");
                }
            }
        }

        private async Task<(string, bool)> HandleSharedNames(string compositeName)
        {
            var record = await GetRecord(compositeName);
            if (record != null)
            {
                if (record.Shared) //redirect
                {
                    return (record.OwnerId + '_' + record.Name, record.ReadOnly ?? false);
                }
                return (compositeName, record.ReadOnly ?? false);
            }
            return ("", true); //no matching model 
        }

        private async Task<KGraph> GetRecord(string compositeName)
        {
            var divider = compositeName.IndexOf('_');
            var id = compositeName.Substring(0, divider);
            var name = compositeName[(divider + 1)..];
            return await _conn.GetKGModel(id, name);
        }

        /// <summary>
        /// Recursively searches a network for dependencies
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dependencies"></param>
        /// <param name="currentParent"></param>
        /// <param name="currentNode"></param>
        /// <param name="linkLineage"></param>
        /// <param name="paths"></param>
        private void AddDependency(IGraphModel model, List<Dependency> dependencies, GraphObject currentParent, GraphObject currentNode, string linkLineage, List<string> paths)
        {
            var cont = model as BlobGraphContent;
            dependencies.Add(new Dependency { dependencyLineage = linkLineage, dependent = currentNode, parent = currentParent });
            foreach (var c in currentNode.Out)
            {
                if (paths.Contains(c.lineage))
                {
                    var childNode = cont.vertices[c.endId];
                    AddDependency(model, dependencies, currentNode, childNode, c.lineage, paths);
                }
            }
        }

        /// <summary>
        /// Creates a list of nodes connected by connections with lineages in path, with a depth value
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="node">The root node</param>
        /// <param name="paths">permitted lineages for connections</param>
        /// <returns>The found nodes</returns>
        /// <exception cref="MetaRuleException">Thrown if not a DAG.</exception>
        public List<KeyValuePair<GraphObject, int>> GetExecutionOrder(IGraphModel model, GraphObject node, List<string> paths)
        {
            var cont = model as BlobGraphContent;
            var dependencies = new List<Dependency>();
            foreach (var c in node.Out)
            {
                if (paths.Contains(c.lineage))
                {
                    var childNode = cont.vertices[c.endId];
                    AddDependency(model, dependencies, node, childNode, c.lineage, paths);
                }
            }
            //Now establish sequence
            int currentSequence = 1;
            var sequences = new Dictionary<GraphObject, int>();
            bool complete = false;
            while (!complete)
            {
                var deletions = new List<Dependency>();
                foreach (var dep in dependencies)
                {
                    if (!sequences.ContainsKey(dep.dependent))
                        sequences.Add(dep.dependent, currentSequence);
                    else
                        sequences[dep.dependent] = currentSequence;
                    //if dependent does not match any parents
                    //remove that link
                    bool match = false;
                    foreach (Dependency otherDep in dependencies)
                    {
                        if (otherDep.parent == dep.dependent)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        deletions.Add(dep);
                }
                if (deletions.Count == 0 && dependencies.Count > 0)
                {
                    throw new MetaRuleException("Loop found in nodes");
                }
                foreach (Dependency del in deletions)
                {
                    dependencies.Remove(del);
                }
                currentSequence++;
                complete = dependencies.Count == 0;
            }
            //sort dependency list
            return sequences.OrderByDescending(a => a.Value).ToList();
        }

        public async Task<bool> SaveKSChanges(string userId, string subjectId, KnowledgeState ks)
        {
            return (await _conn.UpdateKnowledgeState(userId, subjectId, new KnowledgeStateUpdate(ks))) != null;
        }

        public async Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId, string graphName, bool external)
        {
            return await GetKnowledgeStateByExternalId(userId, subjectId, graphName, external);
        }

        /// <summary>
        /// Get the KS 
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="extId">The subjectId</param>
        /// <param name="graphName">The KG</param>
        /// <param name="externalIds">if true replace objectIDs with externalIds to enhance readability</param>
        /// <returns></returns>
        public async Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, string graphName, bool externalIds)
        {
            var ks = await _conn.GetKnowledgeState(userId, extId, graphName);
            if (!externalIds)
            {
                return ks;
            }
            return await ConvertKSIDs(ks);
        }

        public async Task<KnowledgeState> ConvertKSIDs(KnowledgeState ks)
        {
            if (ks != null && !string.IsNullOrEmpty(ks.knowledgeGraphName))
            {
                try //several error modes - response is to return original
                {
                    var compositeName = CreateCompositeName(ks.userId, ks.knowledgeGraphName);
                    var cont = await Load(compositeName) as BlobGraphContent;
                    //replace ids with externalIds. default to overwriting duplicates.
                    var newData = new Dictionary<string, List<GraphAttribute>>();
                    foreach (var c in ks.data.Keys)
                    {
                        var newKey = cont.vertices[c].externalId;
                        if (!newData.ContainsKey(newKey))
                        {
                            newData.Add(newKey, ks.data[c]);
                        }
                        else
                        {
                            newData[newKey] = ks.data[c];
                        }
                    }
                    return new KnowledgeState { data = newData, knowledgeGraphName = ks.knowledgeGraphName, subjectId = ks.subjectId };
                }
                catch { }

            }
            return ks;
        }

        public async Task<string> CopyRenameKG(string userId, string name, string newName)
        {
            var sourceName = CreateCompositeName(userId, name);
            var destName = CreateCompositeName(userId, newName);
            if (!_localCache.TryGetValue(sourceName, out IGraphModel source) && await _blob.Exists(sourceName))
            {
                var data = await _blob.Read(sourceName);
                source = DeserializeGraph(data);
                source.SanityCheck();
            }
            if (source != null)
            {
                await Store(destName, source);
            }
            //now keep cosmoDB records up to date
            if ((await _conn.GetKGModel(userId, newName)) == null)
                await _conn.CreateKGraph(userId, newName);
            return newName;
        }

        //very brute force and slow. consider caching, or using virtual world


        /// <summary>
        /// Get data for VR display, including Attributes if required
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <param name="lineageFilter">if set show only vertices that descend from filter</param>
        /// <param name="subjectId">KS to use for attributes </param>
        /// <returns></returns>
        /// <exception cref="ExecutionError"></exception>
 

        public async Task<int> GetKGraphCountAsync(string userId)
        {
            return await _conn.GetKGraphCountAsync(userId);
        }

        public string CreateTimedAccessUrl(string userId, string name)
        {
            return _blob.CreateTimedAccessUrl(CreateCompositeName(userId, name));
        }


        internal static string CreateCompositeName(string userId, string name)
        {
            return userId + "_" + name.Replace(" ", "_");
        }

        private string CombineLineages(string lineage, string subLineage)
        {
            if (string.IsNullOrEmpty(subLineage))
                return lineage;
            return $"{lineage}+{subLineage}";
        }

        public async Task<KnowledgeState> GetKnowledgeStateByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            return await _conn.GetKnowledgeStateByTypeAndAttribute(userId, objectId, graphName, attLineage, attValue);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByType(string userId, string objectId, string graphName)
        {
            return await _conn.GetKnowledgeStatesByType(userId, objectId, graphName);
        }

        public async Task<KnowledgeState> CreateKnowledgeState(KnowledgeState state)
        {
            return await _conn.CreateKnowledgeState(state);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttribute(string userId, string objectId, string graphName, string attLineage, string attValue)
        {
            return await _conn.GetKnowledgeStatesByTypeAndAttribute(userId, objectId, graphName, attLineage, attValue);
        }

        public async Task<KnowledgeState> DeleteKnowledgeState(string userId, string subjectId, string graphName)
        {
            return await _conn.DeleteKnowledgeState(userId, subjectId, graphName);
        }

        public async Task<List<KnowledgeState>> GetKnowledgeStatesByTypeAndAttributeExistence(string userId, string objectId, string graphName, string attLineage)
        {
            return await _conn.GetKnowledgeStatesByTypeAndAttributeExistence(userId, objectId, graphName, attLineage);
        }

        public async Task<string> ShareKGraph(string userId, string name, string sharerId, bool readOnly, bool hidden)
        {
            return await _conn.ShareKGraph(userId, name, sharerId, readOnly, hidden);
        }

        public async Task<List<KnowledgeState>> GetSetOfKnowledgeStates(string userId, List<string> ksIds, string graphName)
        {
            return await _conn.GetSetOfKnowledgeStates(userId, ksIds, graphName);
        }

        public async Task<List<GraphAbstraction>> GetSetofConnectedObjects(string userId, List<string> ksIds, string graphName)
        {
            var notFound = new List<string>();
            var res = await _conn.GetSetofConnectedObjects(userId, ksIds, graphName, notFound);
            if (notFound.Any())
            {
                var compositeName = userId + "_" + graphName;
                if (await Load(compositeName) is not BlobGraphContent cont)
                    throw new ExecutionError($"Graph  '{compositeName}' does not exist.");
                foreach (var ids in notFound)
                {
                    if (cont.vertices.ContainsKey(ids))
                        res.Add(cont.vertices[ids]);
                }
            }
            return res;
        }

        public Task<bool> ExistsInCache(string userId, string graphName)
        {
            return Task.FromResult(_localCache.TryGetValue(userId + "_" + graphName, out IGraphModel model));
        }

        public async Task<byte[]> KGContents(string userId, string graphName)
        {
            var blobName = userId + "_" + graphName;
            var model = await Load(blobName);
            if (model != null)
            {
                if (model is BlobGraphContent cont)
                {
                    cont.key = _license.CreateKey(DateTime.UtcNow + new TimeSpan(modelLicenseDays, 0, 0, 0), blobName, "");
                    return SerializeGraph(cont);
                }
            }
            return new byte[0];
        }

        public Task<string> CreateTempKG(string userId, string graphName, byte[] bytes)
        {
            var compositeName = userId + "_" + graphName;
            try
            {
                var model = DeserializeGraph(bytes);
                if (model != null)
                    _localCache.Set(compositeName, model, TimeSpan.FromMinutes(kgCacheMinutes));
                else
                    throw new Exception("Couldn't deserialize model.");
            }
            catch(Exception ex)
            {
                throw new ExecutionError($"Invalid or empty graph data for {graphName}", ex);
            }
            return Task.FromResult(compositeName);
        }

        public Task<bool> IsDemo(string compositeName)
        {
            throw new NotImplementedException();
        }
    }

    public class Dependency
    {
        public GraphObject parent { get; set; }
        public GraphObject dependent { get; set; }

        public string dependencyLineage { get; set; }
    }
}
