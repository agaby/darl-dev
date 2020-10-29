using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.Azure.Storage.Shared.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using ProtoBuf;
using QuickGraph.Algorithms.MaximumFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    [ProtoContract]
    public class BlobGraphPrimitives : IGraphPrimitives
    {

        private IBlobConnectivity _blob;
        private IDistributedCache _cache;
        private IConnectivity _conn;

        private Dictionary<string, BlobGraphContent> buffer = new Dictionary<string, BlobGraphContent>();

        private Object lockObject = new object();

        private HashSet<string> modified = new HashSet<string>();

        private Timer flushTimer;

        private bool modifiedToggle = false;

        public static int maxDepth = 0;

        public BlobGraphPrimitives(IEnumerable<IBlobConnectivity> blobs, IDistributedCache cache, IConnectivity conn)
        {
            _blob = blobs.FirstOrDefault(h => h.implementation == nameof(BlobGraphConnectivity));
            _cache = cache;
            _conn = conn;
            flushTimer = new Timer(FlushTimerTimeOut, null, 500, 500);
        }

        public async Task<GraphConnection> CreateConnection(string compositeName, GraphConnectionInput conn)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var gc = new GraphConnection { id = Guid.NewGuid().ToString(), endId = conn.endId, existence = conn.existence, inferred = false, lineage = conn.lineage, name = conn.name, properties = conn.properties, startId = conn.startId, weight = conn.weight ?? 1.0, _virtual = false };
            if (cont.vertices.ContainsKey(conn.startId))
                cont.vertices[conn.startId].Out.Add(gc);
            else
                throw new ExecutionError($"Real vertex id {conn.startId} does not exist");
            if (cont.vertices.ContainsKey(conn.endId))
                cont.vertices[conn.endId].In.Add(gc);
            else
                throw new ExecutionError($"Real vertex id {conn.endId} does not exist"); 
            cont.edges.Add(gc.id,gc);
            FlagChanges(compositeName);
            return gc;
        }

        public async Task<GraphObject> CreateObject(string compositeName, GraphObjectInput graphObject)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var go = new GraphObject { existence = graphObject.existence, externalId = graphObject.externalId, id = Guid.NewGuid().ToString(), inferred = false, lineage = graphObject.lineage, name = graphObject.name, _virtual = false, properties = graphObject.properties };
            cont.vertices.Add(go.id, go);
            FlagChanges(compositeName);
            return go;
        }

        public async Task CreateVirtualConnection(IGraphModel model, string child, string lineage, string connectionLabel)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            if (!cont.virtualEdges.ContainsKey(($"{child}->{lineage}")))
            {
                var gc = new GraphConnection { id = Guid.NewGuid().ToString(), endId = lineage, inferred = false, name = connectionLabel, startId = child, weight = 1.0, _virtual = true };
                if (cont.virtualVertices.ContainsKey(child))
                    cont.virtualVertices[child].Out.Add(gc);
                else
                    throw new ExecutionError($"Virtual vertex id {child} does not exist");
                if (cont.virtualVertices.ContainsKey(lineage))
                    cont.virtualVertices[lineage].In.Add(gc);
                else
                    throw new ExecutionError($"Virtual vertex id {lineage} does not exist");
                cont.virtualEdges.Add($"{child}->{lineage}", gc);
            }
       }

        public async Task CreateVirtualObject(IGraphModel model, string lineage, string typeword, string description)
        {
            var go = new GraphObject { id = Guid.NewGuid().ToString(), inferred = false, lineage = lineage, name = typeword, _virtual = true, properties = new List<GraphAttribute> { new GraphAttribute { name = "description", value = description, lineage= "noun:01,4,05,21,05"} } };
            ((BlobGraphContent)model).virtualVertices.Add(go.lineage, go);
        }

        public async Task<GraphConnection> DeleteConnection(string compositeName, string id)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if(!cont.edges.ContainsKey(id))
            { 
                return null;
            }
            var conn = cont.edges[id];
            if(cont.vertices.ContainsKey(conn.startId))
            {
                cont.vertices[conn.startId].Out.Remove(conn);
            }
            if (cont.vertices.ContainsKey(conn.endId))
            {
                cont.vertices[conn.endId].In.Remove(conn);
            }
            cont.edges.Remove(id);
            FlagChanges(compositeName);
            return conn;
        }

        public async Task<GraphObject> DeleteObject(string compositeName, string id)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.vertices.ContainsKey(id))
            {
                return null;
            }
            var node = cont.vertices[id];
            //delete associated connections
            foreach(var c in node.Out)
            {
                if (cont.vertices.ContainsKey(c.endId))
                {
                    var end = cont.vertices[c.endId];
                    var success = end.In.Remove(end.In.Where(a => a.id == c.id).FirstOrDefault());
                }
                cont.edges.Remove(c.id);
            }
            foreach (var c in node.In)
            {
                if (cont.vertices.ContainsKey(c.startId))
                {
                    var start = cont.vertices[c.startId];
                    var success = start.Out.Remove(start.Out.Where(a => a.id == c.id).FirstOrDefault());
                }
                cont.edges.Remove(c.id);
            }
            cont.vertices.Remove(id);
            FlagChanges(compositeName);
            return node;
        }

        public async Task<GraphConnection> GetConnectionByIds(string compositeName, string startId, string endId, string lineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var start = cont.vertices[startId];
            return start.Out.Where(a => a.lineage == lineage && a.endId == endId).FirstOrDefault();
        }

        public async Task<GraphObject> GetGraphObjectByExternalId(string compositeName, string externalId)
        {//will need to see if it is worth adding another index.
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.vertices.Values.Where(a => a.externalId == externalId).FirstOrDefault();
        }

        public async Task<GraphObject> GetGraphObjectById(string compositeName, string id)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (cont.vertices.ContainsKey(id))
                return cont.vertices[id];
            return null;
        }

        public async Task<string> GetGraphObjectProperty(string compositeName, string id, string property)
        {
            var o = await GetGraphObjectById(compositeName, id);
            if (o == null)
                return string.Empty;
            return GetAttibuteGivenObject(o, property);
        }

        public async Task<List<GraphObject>> GetGraphObjects(string compositeName, string name, string lineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.vertices.Values.Where(a => a.name == name && a.lineage.StartsWith(lineage)).ToList();
        }

        public async Task<bool> LineageExists(IGraphModel model,string lineage)
        {
            return ((BlobGraphContent)model).virtualVertices.ContainsKey(lineage);
        }

        public async Task<GraphConnection> UpdateConnection(string compositeName, GraphConnectionUpdate gc)
        {
            var conn = await GetConnectionByIds(compositeName, gc.startId, gc.endId, gc.lineage);
            if (conn == null)
                return null;
            //update nun-null elements in gc
            bool changed = false;
            if(gc.existence != null)
            {
                conn.existence = gc.existence;
                changed = true;
            }
            if (gc.lineage != null) //allow change of lineage?
            {
                if (conn.lineage != gc.lineage)
                {
                    conn.lineage = gc.lineage;
                    changed = true;
                }
            }
            if (gc.name != null)
            {
                if (conn.name != gc.name)
                {
                    conn.name = gc.name;
                    changed = true;
                }
            }
            if (gc.weight != null)
            {
                if (conn.weight != (gc.weight ?? 1.0))
                {
                    conn.weight = gc.weight ?? 1.0;
                    changed = true;
                }
            }
            if (gc.properties != null && gc.properties.Any())
            {
                conn.properties = gc.properties;
                changed = true;

            }
            if(changed)
            {
                FlagChanges(compositeName);
            }
            return conn;
        }

        /// <summary>
        /// Updates a real object
        /// </summary>
        /// <param name="compositeName"></param>
        /// <param name="go"></param>
        /// <returns>The modified object</returns>
        /// <remarks>Merges rather than overwriting properties</remarks>
        public async Task<GraphObject> UpdateObject(string compositeName, GraphObjectUpdate go)
        {
            var node = await GetGraphObjectById(compositeName, go.id);
            if (node == null)
                return null;
            //update nun-null elements in go
            bool changed = false;
            if (go.existence != null)
            {
                node.existence = go.existence;
                changed = true;
            }
            if (go.lineage != null) //allow change of lineage?
            {
                if (node.lineage != go.lineage)
                {
                    node.lineage = go.lineage;
                    changed = true;
                }
            }
            if (go.name != null)
            {
                if (node.name != go.name)
                {
                    node.name = go.name;
                    changed = true;
                }
            }
            if (go.properties != null && go.properties.Any())
            {
                foreach (var a in go.properties)
                {
                    var found = node.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                    if (found != null)
                    {
                        node.properties.Remove(found);
                    }
                    node.properties.Add(a);
                }
            }
            if (changed)
            {
                FlagChanges(compositeName);
            }
            return node;
        }

        public async Task<bool> VirtualAssociationExists(IGraphModel model, string lineage1, string lineage2)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            if (!cont.virtualVertices.ContainsKey(lineage1) || !cont.virtualVertices.ContainsKey(lineage2))
                return false;
            var obj1 = cont.virtualVertices[lineage1];
            var obj2 = cont.virtualVertices[lineage2];
            //get the set of direct ancestors for each object
            var list1 = new List<GraphObject>();
            FollowHypernymy(model, obj1, list1);
            var list2 = new List<GraphObject>();
            var otherIds = list2.Select(a => a.id).ToList();
            FollowHypernymy(model, obj2, list2);
            //now search for connections
            foreach(var o in list1)
            {
                if (o.Out.Where(a => otherIds.Contains(a.id)).Any())
                    return true;
            }
            return false;
        }

        private void FollowHypernymy(IGraphModel model, GraphObject g, List<GraphObject> list)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            foreach (var l in g.Out.Where(a => a.name == "kind_of"))
            {
                var parent = cont.virtualVertices[l.endId];
                list.Add(parent);
                FollowHypernymy(model, parent, list);
            }
        }

        public async Task Store(string blobName, IGraphModel model)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            await _blob.Write(blobName, SerializeGraph(cont));
        }

        public async Task<IGraphModel> Load(string blobName)
        {
            if (buffer.ContainsKey(blobName))
                return buffer[blobName];
 
            if (await _blob.Exists(blobName))
            {
                var data = await _blob.Read(blobName);
                var model = DeserializeGraph(data);
                buffer.Add(blobName, model);
                return model;
            }
            else
            {
                var model =  new BlobGraphContent();
                buffer.Add(blobName, model);
                return model;
            }

        }

        public byte[] SerializeGraph(IGraphModel model)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<BlobGraphContent>(ms, cont);
                ms.Position = 0;
                return ms.ToArray();
            }
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
            await _blob.Write(compositeName, SerializeGraph(new BlobGraphContent()));
            return true;
        }

        public async Task<bool> DeleteModel(string compositeName)
        {
            if (buffer.ContainsKey(compositeName))
                buffer.Remove(compositeName);
            return await _blob.Delete(compositeName);
        }

        public async Task<List<string>> ListModels(string userId)
        {
            return _blob.List(userId);
        }

 
        public async Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID)
        {
            try
            {
                var cont = await Load(compositeName);
                var start = await GetGraphObjectByExternalId(compositeName, startExternalID);
                var target = await GetGraphObjectByExternalId(compositeName, endExternalID);
                return ShortestPath(cont, start, target);
            }
            catch(Exception ex)
            {

            }
            return null;
        }

        public async Task<List<StringStringPair>> GetLinkedCategories(string compositeName, string rootExternalID, string childLineage, string childValueAttribute)
        {
            var list = new List<StringStringPair>();
            if (string.IsNullOrEmpty(rootExternalID))
            {
                var objects = await GetGraphObjectsByLineage(compositeName, childLineage);
                foreach(var o in objects)
                {
                    var externalId = GetAttibuteGivenObject(o, nameof(GraphObject.externalId));
                    var value = GetAttibuteGivenObject(o, childValueAttribute);
                    list.Add(new StringStringPair(externalId, value));
                }
            }
            else
            {
                var obj = await GetGraphObjectByExternalId(compositeName, rootExternalID);
                if (obj != null)
                {
                    foreach (var c in obj.Out)
                    {
                        var other = await GetGraphObjectById(compositeName, c.endId);
                        if (other.lineage.StartsWith(childLineage))
                        {
                            var externalId = GetAttibuteGivenObject(other, nameof(GraphObject.externalId));
                            var value = GetAttibuteGivenObject(other, childValueAttribute);
                            list.Add(new StringStringPair(externalId, value));
                        }
                    }
                    foreach (var c in obj.In)
                    {
                        var other = await GetGraphObjectById(compositeName, c.startId);
                        if (other.lineage.StartsWith(childLineage))
                        {
                            var externalId = GetAttibuteGivenObject(other, nameof(GraphObject.externalId));
                            var value = GetAttibuteGivenObject(other, childValueAttribute);
                            list.Add(new StringStringPair(externalId, value));
                        }
                    }
                }
            }
            return list;
        }

        public async Task<List<StringStringPair>> GetCategoriesByLineage(string compositeName, string childLineage, string childValueAttribute)
        {
            var list = new List<StringStringPair>();
            var cont = await Load(compositeName) as BlobGraphContent;
            foreach (var v in cont.vertices.Values.Where(a => a.lineage.StartsWith(childLineage)))
            {
                var externalId = GetAttibuteGivenObject(v, nameof(GraphObject.externalId));
                var value = GetAttibuteGivenObject(v, childValueAttribute);
                list.Add(new StringStringPair(externalId, value));
            }
            return list;
        }

        public async Task<string> GetAttribute(string compositeName, string externalID, string propertyName)
        {
            var obj = await GetGraphObjectByExternalId(compositeName, externalID);
            return GetAttibuteGivenObject(obj, propertyName);
        }

        private string GetAttibuteGivenObject(GraphObject obj, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(GraphObject.name):
                    return obj.name;
                case nameof(GraphObject.existence):
                    return string.Join(",", obj.existence);
                case nameof(GraphObject.lineage):
                    return obj.lineage;
                case nameof(GraphObject.externalId):
                    return obj.externalId;
                case nameof(GraphObject.id):
                    return obj.id;
                case nameof(GraphObject.inferred):
                    return obj.inferred.ToString();
                default:
                    if (obj.properties != null && obj.properties.Where(a => a.name == propertyName).Any())
                    {
                        return obj.properties.Where(a => a.name == propertyName).First().value;
                    }
                    return string.Empty;
            }
        }

        /// <summary>
        /// flag that one of the GraphModels has changed
        /// </summary>
        /// <param name="compositeName"></param>
        private void FlagChanges(string compositeName)
        {
            lock(lockObject)
            {
                modified.Add(compositeName);
                modifiedToggle = true;
            }
        }

        /// <summary>
        /// Dijkstra's shortest path algorithm
        /// </summary>
        /// <param name="model"></param>
        /// <param name="start"></param>
        /// <param name="Target"></param>
        /// <returns>the vertex,edge,vertex... sequence</returns>
        /// <remarks></remarks>
        public List<GraphElement> ShortestPath(IGraphModel model, GraphObject start, GraphObject target)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            var list = new List<GraphElement>{ start};
            var coverage = new Dictionary<GraphObject, (double, bool, int)> { {start,(0,true, 0) } };
            try
            {
                var path = ShortestPathRecursion(model, start, target, list, coverage, 0);
            }
            catch(Exception ex)
            {

            }
            var shortestPath = new List<GraphElement> { target };
            var next = target;
            while(next != start)
            {
                if(!coverage.ContainsKey(next))
                {
                    return null;
                }
                var potential = coverage[next].Item1;
                bool found = false;
                foreach (var i in next.In)
                {
                    var begin = cont.vertices[i.startId];
                    if (!coverage.ContainsKey(begin))
                        continue;
                    var otherPotential = coverage[begin].Item1;
                    if(potential - otherPotential == i.weight)
                    {
                        shortestPath.Add(i);
                        shortestPath.Add(begin);
                        next = begin;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return null;
            }
            shortestPath.Reverse();
            return shortestPath;
        }

        private List<GraphElement> ShortestPathRecursion(IGraphModel model, GraphObject start, GraphObject target, List<GraphElement> path, Dictionary<GraphObject, (double, bool, int)> coverage, int depth)
        {
            if (depth > maxDepth)
                maxDepth = depth;
            BlobGraphContent cont = model as BlobGraphContent;
            var current = new List<(GraphObject, double, List<GraphElement>)>();
            foreach (var c in start.Out)
            {
                var newPath = new List<GraphElement>(path);
                var next = cont.vertices[c.endId];
                newPath.Add(c);
                newPath.Add(next);
                var distance = coverage[start].Item1 + c.weight;
                if (coverage.ContainsKey(next))
                {
                    var oldDistance = coverage[next].Item1;
                    var visitCount = coverage[next].Item3 + 1;

                    if (distance < oldDistance)
                    {
                        coverage[next] = (distance, visitCount >= next.In.Count, visitCount);
                    }
                    else
                    {
                        coverage[next] = (oldDistance, visitCount >= next.In.Count, visitCount);
                    }
                }
                else
                { 
                    coverage.Add(next, (distance, next.In.Count == 1, 1));
                }
                current.Add((next, distance, newPath));
                if (coverage[next].Item2)
                {
                    if (next == target)
                        return newPath;
                }

            }
 /*           foreach (var c in start.In)
            {
                var newPath = new List<GraphElement>(path);
                var next = cont.vertices[c.startId];
                if (coverage.ContainsKey(next))
                {
                    continue;
                }
                newPath.Add(c);
                newPath.Add(next);
                var distance = coverage[start].Item1 + c.weight;
                coverage.Add(next, (distance, true));
                current.Add((next, distance, newPath));
                if (next == target)
                    return newPath;
            }*/
            //sort current by distance descending
            current = current.OrderBy(a => a.Item2).ToList();
            foreach(var v in current)
            {
                if (depth < 10)
                {
                    var found = ShortestPathRecursion(model, v.Item1, target, new List<GraphElement>(v.Item3), coverage, ++depth);
                    if (coverage.ContainsKey(target) && coverage[target].Item2)
                        return found;
                }
            }
            return null;
        }


        public async Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.vertices.Values.Where(a => a.lineage.StartsWith(lineage)).ToList();
        }

        /// <summary>
        /// writes out any modified graph models to the blob storage
        /// </summary>
        /// <param name="stateInfo"></param>
        public void FlushTimerTimeOut(Object stateInfo)
        {
            //ModifiedToggle is used to only write out the GraphModels if one timeout has elapsed since the last change.
            //Intended to prevent disruption to bursts of updates.
            //Has side effect that one very busy model will stop another quiet model being written out.
            lock(lockObject)
            {
                if(modifiedToggle)
                {
                    modifiedToggle = false;
                    return;
                }
            }
            List<string> changedModels = new List<string>();
            //copy the modified list and clear it
            lock(lockObject)
            {
                changedModels = modified.ToList();
                modified.Clear();
                modifiedToggle = false;
            }
            foreach(var s in changedModels)
            {
                //get the graphmodel
                var model = buffer[s];
                byte[] data;
                //lock it
                lock(lockObject)
                {
                    //serialize it and release it
//                    data = SerializeGraph(model);
                }
                //write out the serialized model
 //               _blob.Write(s, data);

            }  
        }

        public async Task Store(string compositeName)
        {
            modified.Remove(compositeName);
            var model = buffer[compositeName];
            byte[] data;
            data = SerializeGraph(model);
            await _blob.Write(compositeName, data);
        }

        public async Task<List<GraphObject>> GetAllRealObjects(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.vertices.Values.ToList();
        }

        public async Task<IEnumerable<GraphObject>> GetAllVirtualObjects(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.virtualVertices.Values.ToList();
        }

        public async Task<IEnumerable<GraphConnection>> GetAllRealConnections(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.edges.Values.ToList();
        }

        public async Task<IEnumerable<GraphConnection>> GetAllVirtualConnections(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.virtualEdges.Values.ToList();
        }

        public async Task CreateRawObject(IGraphModel model, GraphObject graphObject)
        {
            var cont = model as BlobGraphContent;
            var go = new GraphObject { existence = graphObject.existence, externalId = graphObject.externalId, id = graphObject.id, inferred = false, lineage = graphObject.lineage, name = graphObject.name, _virtual = graphObject._virtual, properties = graphObject.properties };
            cont.vertices.Add(go.id, go);
        }

        public async Task CreateRawConnection(IGraphModel model, GraphConnection conn)
        {
            var cont = model as BlobGraphContent;
            var gc = new GraphConnection { id = Guid.NewGuid().ToString(), endId = conn.endId, existence = conn.existence, inferred = false, lineage = conn.lineage, name = conn.name, properties = conn.properties, startId = conn.startId, weight = conn.weight, _virtual = conn._virtual };
            if (cont.vertices.ContainsKey(conn.startId))
                cont.vertices[conn.startId].Out.Add(gc);
            else
                throw new ExecutionError($"Real vertex id {conn.startId} does not exist");
            if (cont.vertices.ContainsKey(conn.endId))
                cont.vertices[conn.endId].In.Add(gc);
            else
                throw new ExecutionError($"Real vertex id {conn.endId} does not exist");
            cont.edges.Add(gc.id, gc);
        }




        public async Task CreateVirtualAttribute(string compositeName, string lineage, GraphAttributeInput att)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if(cont.virtualVertices.ContainsKey(lineage))
            {
                var node = cont.virtualVertices[lineage];
                if (node.properties.Where(a => a.lineage == att.lineage).Any())
                {
                    node.properties.Remove(node.properties.Where(a => a.lineage == att.lineage).First());
                }
                node.properties.Add(new GraphAttribute {id = Guid.NewGuid().ToString(), existence = att.existence, confidence = att.confidence, inferred = false, lineage = att.lineage, name = att.name, type = att.type, value = att.value, _virtual = true });
            }
        }

        public async Task<List<GraphObject>> FindNext(IGraphModel model, List<KeyValuePair<GraphObject, int>> ordered, KnowledgeState ks, GraphObject node, List<string> paths, string completedLineage)
        {
            var list = new List<GraphObject>();
            var cont = model as BlobGraphContent;
            //first build dependency list of nodes reachable from the start node
            var saliences = new Dictionary<GraphObject, double>();
            //in descending order calculate salience
            saliences.Add(node, 1.0);
            foreach(var o in ordered)
            {
                double salience = 0.0;
                foreach(var c in o.Key.In)
                {
                    if (paths.Contains(c.lineage))
                    {
                        var parentNode = cont.vertices[c.startId];
                        salience += saliences[parentNode];
                    }
                }
                saliences.Add(o.Key, salience);
            }
            var orderedBySalience = saliences.OrderByDescending(a => a.Value).ToList();
            var currentSalience = 0.0;
            foreach(var o in orderedBySalience)
            {
                var obj = o.Key;
                currentSalience = Math.Max(currentSalience, o.Value);
                if (currentSalience != o.Value && list.Count > 0)
                    break;
                if (obj.Out.Count > 0) // not leaf
                    continue;
                if(ks.ContainsAttribute(obj.id, completedLineage))
                    continue;
                list.Add(obj);
            }

            //list is leaf nodes with highest salience
            return list;
        }

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

        public List<KeyValuePair<GraphObject,int>> GetExecutionOrder(IGraphModel model, GraphObject node, List<string> paths)
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

        public async Task<GraphObject> GetRecognitionRoot(IGraphModel model, string rootLineage)
        {
            if (model.recognitionRoots.ContainsKey(rootLineage))
                return model.recognitionRoots[rootLineage];
            throw new ExecutionError($"Recognition root '{rootLineage}' does not exist");
        }

        public async Task<GraphObject> CreateRecognitionRoot(string compositeName, string rootLineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var rootObject = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = rootLineage, name = "root" };
            if (cont.recognitionRoots.ContainsKey(rootLineage))
            {
                throw new ExecutionError($"Recognition root '{rootLineage}' is already specified");
            }
            cont.recognitionVertices.Add(rootObject.id, rootObject);
            cont.recognitionRoots.Add(rootLineage, rootObject);
            return rootObject;
        }

        public async Task<GraphConnection> CreateRecognitionConnection(string compositeName, GraphConnectionInput graphConnection)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = graphConnection.endId, lineage = graphConnection.lineage, startId = graphConnection.startId, weight = graphConnection.weight ?? 0.0 };
            if(!cont.recognitionVertices.ContainsKey(conn.startId))
                throw new ExecutionError($"GraphConnection startId '{conn.startId}' does not exist.");
            if (!cont.recognitionVertices.ContainsKey(conn.endId))
                throw new ExecutionError($"GraphConnection endId '{conn.endId}' does not exist.");
            cont.recognitionVertices[conn.startId].Out.Add(conn);
            cont.recognitionVertices[conn.endId].In.Add(conn);
            cont.recognitionEdges.Add(conn.id, conn);
            return conn;
        }

        public async Task<GraphObject> CreateRecognitionObject(string compositeName, GraphObjectInput graphObject)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var obj= new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = graphObject.lineage, name = graphObject.name, properties = graphObject.properties };
            cont.recognitionVertices.Add(obj.id, obj);
            return obj;
        }

        public async Task<GraphObject> DeleteRecognitionObject(string compositeName, string id)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.recognitionVertices.ContainsKey(id))
                throw new ExecutionError($"GraphConnection id '{id}' does not exist.");
            var obj = cont.recognitionVertices[id];
            foreach(var c in obj.In)
            {
                cont.recognitionVertices[c.startId].Out.Remove(c);
                cont.recognitionEdges.Remove(c.id);
            }
            foreach (var c in obj.Out)
            {
                cont.recognitionVertices[c.endId].In.Remove(c);
                cont.recognitionEdges.Remove(c.id);
            }
            cont.recognitionVertices.Remove(id);
            DeleteRecognitionOrphans(cont);
            return obj;
        }


        public async Task<GraphObject> DeleteRecognitionRoot(string compositeName, string rootLineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if(cont.recognitionRoots.ContainsKey(rootLineage))
                throw new ExecutionError($"Recognition root '{rootLineage}' does not exist");
            var obj = cont.recognitionRoots[rootLineage];
            cont.recognitionRoots.Remove(rootLineage);
            DeleteRecognitionOrphans(cont);
            return obj;
        }

        public async Task<GraphObject> UpdateRecognitionObject(string compositeName, GraphObjectUpdate go)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.recognitionVertices.ContainsKey(go.id))
                throw new ExecutionError($"GraphConnection id '{go.id}' does not exist.");
            var obj = cont.recognitionVertices[go.id]; 
            //update nun-null elements in go
            if (go.existence != null)
            {
                obj.existence = go.existence;
            }
            if (go.lineage != null) //allow change of lineage?
            {
                if (obj.lineage != go.lineage)
                {
                    obj.lineage = go.lineage;
                }
            }
            if (go.name != null)
            {
                if (obj.name != go.name)
                {
                    obj.name = go.name;
                }
            }
            if (go.properties != null && go.properties.Any())
            {
                //merge properties
                foreach (var a in go.properties)
                {
                    var found = obj.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                    if (found != null)
                    {
                        obj.properties.Remove(found);
                    }
                    obj.properties.Add(a);
                }
            }
            return obj;
        }

        public async Task<List<GraphObject>> NavigateRecognition(string compositeName, string root, string path)
        {
            var list = new List<GraphObject>();
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.recognitionRoots.ContainsKey(root))
                return list;
            var tokens = path.Split('/').ToList();
            return cont.recognitionRoots[root].Navigate(cont, tokens);
        }

        public async Task<GraphObject> UpdateVirtualObject(string compositeName, GraphObjectUpdate go, bool merge = false)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.virtualVertices.ContainsKey(go.lineage))
                return null;
            var node = cont.virtualVertices[go.lineage];
            //update nun-null elements in go
            if (go.name != null)
            {
                if (node.name != go.name)
                {
                    node.name = go.name;
                }
            }
            if (go.properties != null && go.properties.Any())
            {
                if (merge)
                {
                    //merge properties
                    foreach (var a in go.properties)
                    {
                        var found = node.properties.Where(b => b.lineage == a.lineage).FirstOrDefault();
                        if (found != null)
                        {
                            node.properties.Remove(found);
                        }
                        node.properties.Add(a);
                    }
                }
                else
                {
                    node.properties = go.properties;
                }
            }
            return node;
        }

        public async Task<GraphObject> FindRecognition(string compositeName, string root, string path)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (!cont.recognitionRoots.ContainsKey(root))
                return null;
            var tokens = path.Split('/').ToList();
            return cont.recognitionRoots[root].Find(cont, tokens);
        }

        public async Task<GraphObject> GetVirtualObjectByLineage(string compositeName, string lineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (cont.virtualVertices.ContainsKey(lineage))
                return cont.virtualVertices[lineage];
            return null;
        }

        public async Task<GraphObject> GetRecognitionObjectById(string compositeName, string id)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            if (cont.recognitionVertices.ContainsKey(id))
                return cont.recognitionVertices[id];
            return null;
            throw new NotImplementedException();
        }

        public async Task<DisplayModel> GetRealDisplayGraph(string compositeName, string lineageFilter)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            if(string.IsNullOrEmpty(lineageFilter)) //return everything
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Select(i => new DisplayObject {id = i.id, name = i.name, lineage = i.lineage, externalId = i.externalId }));
                dmodel.edges.AddRange(cont.edges.Values.Select(i => new DisplayConnection {id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            else
            {
                dmodel.nodes.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).Select((i => new DisplayObject { id = i.id, name = i.name, lineage = i.lineage, externalId = i.externalId })));
                dmodel.edges.AddRange(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.In).Intersect(cont.vertices.Values.Where(a => a.lineage.StartsWith(lineageFilter)).SelectMany(a => a.Out)).Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId, target = i.endId }));
            }
            return dmodel;
        }

        public async Task<DisplayModel> GetVirtualDisplayGraph(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            dmodel.nodes.AddRange(cont.virtualVertices.Values.Select(i => new DisplayObject { id = i.lineage, name = i.name, lineage = i.lineage}));
            dmodel.edges.AddRange(cont.virtualEdges.Values.Select(i => new DisplayConnection { id = i.id, name = i.name, source = i.startId, target = i.endId }));           
            return dmodel;
        }

        public async Task<DisplayModel> GetRecognitionDisplayGraph(string compositeName)
        {
            var dmodel = new DisplayModel { nodes = new List<DisplayObject>(), edges = new List<DisplayConnection>() };
            var cont = await Load(compositeName) as BlobGraphContent;
            foreach(var robj in cont.recognitionRoots.Values)
            {
                RecursivelyAddElements(robj, dmodel, cont);
            }
            return dmodel;
        }

        public async Task CorrectBrokenLinks(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var deleteList = new List<GraphConnection>();
            foreach (var n in cont.vertices.Values)
            {
                foreach(var c in n.Out)
                {
                    if (!cont.vertices.ContainsKey(c.endId))
                        deleteList.Add(c);
                }
                foreach(var c in deleteList)
                {
                    n.Out.Remove(c);
                    cont.edges.Remove(c.id);
                }
            }
        }

        public async Task SaveKSChanges(string userId, string subjectId, KnowledgeState ks)
        {
            await _conn.UpdateKnowledgeState(userId, subjectId, new KnowledgeStateUpdate (ks));
        }

        public async Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId)
        {
            return await _conn.GetKnowledgeState(userId, subjectId);
        }

        public async Task<KnowledgeState> GetKnowledgeStateByExternalId(string userId, string extId, bool externalIds)
        {
            var ks = await _conn.GetKnowledgeState(userId, extId);
            if (!externalIds)
            {
                return ks;
            }
            if(ks != null && !string.IsNullOrEmpty(ks.knowledgeGraphName))
            {
                try //several error modes - response is to return original
                {
                    var compositeName = CreateCompositeName(userId, ks.knowledgeGraphName);
                    var cont = await Load(compositeName) as BlobGraphContent;
                    //replace ids with externalIds. default to overwriting duplicates.
                    var newData = new Dictionary<string, List<GraphAttribute>>();
                    foreach (var c in ks.data.Keys)
                    {
                        var newKey = cont.vertices[c].externalId;
                        if(!newData.ContainsKey(newKey))
                        {
                            newData.Add(newKey, ks.data[c]);
                        }
                        else
                        {
                            newData[newKey] = ks.data[c];
                        }
                    }
                    return new KnowledgeState(userId, new KnowledgeStateInput { data = newData, knowledgeGraphName = ks.knowledgeGraphName, subjectId = ks.Id });
                }
                catch { }

            }
            return ks;
        }

        public async Task ClearGraphContent(string compositeName)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            cont.edges.Clear();
            cont.recognitionEdges.Clear();
            cont.recognitionRoots.Clear();
            cont.recognitionVertices.Clear();
            cont.vertices.Clear();
            cont.virtualEdges.Clear();
            cont.virtualVertices.Clear();
        }

        public async Task<string> CopyRenameKG(string userId, string name, string newName)
        {
            var sourceName = CreateCompositeName(userId, name);
            var destName = CreateCompositeName(userId, newName);
            BlobGraphContent source = null;
            if (buffer.ContainsKey(sourceName))
            {
                source = buffer[sourceName];
            }
            else if (await _blob.Exists(sourceName))
            {
                var data = await _blob.Read(sourceName);
                source = DeserializeGraph(data);
            }
            if (source != null)
            {
                await Store(destName, source);
            }
            //now keep cosmoDB records up to date
            if ((await _conn.GetKGModel(userId, newName)) != null) 
                await _conn.CreateKGraph(userId, newName);
            return newName;
        }

        internal static string CreateCompositeName(string userId, string name)
        {
            return userId + "_" + name.Replace(" ", "_");
        }

        private void RecursivelyAddElements(GraphObject robj, DisplayModel dmodel, IGraphModel cont)
        {
            dmodel.nodes.Add(new DisplayObject { id = robj.id, name = robj.name, lineage = robj.lineage });
            foreach (var c in robj.Out)
            {
                dmodel.edges.Add(new DisplayConnection { id = c.id, name = c.name, source = c.startId, target = c.endId});
                RecursivelyAddElements(cont.recognitionVertices[c.endId], dmodel, cont);
            }
        }


        private void DeleteRecognitionOrphans(IGraphModel model)
        {
            var complete = false;
            while (!complete)
            {
                var deletions = new List<GraphObject>();
                foreach (var o in model.recognitionVertices.Values)
                {
                    if (!o.In.Any() && !model.recognitionRoots.Values.Contains(o))
                    {
                        deletions.Add(o);
                    }
                }
                foreach (var o in deletions)
                {
                    foreach (var c in o.Out)
                    {
                        model.recognitionVertices[c.endId].In.Remove(c);
                        model.recognitionEdges.Remove(c.id);
                    }
                    model.recognitionVertices.Remove(o.id);
                }
                if (!deletions.Any())
                    complete = true;
            }
        }

 
    }

    public class Dependency
    {
        public GraphObject parent { get; set; }
        public GraphObject dependent { get; set; }

        public string dependencyLineage { get; set; }
    }
}
