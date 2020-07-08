using Darl.GraphQL.Models.Connectivity;
using Darl.Thinkbase;
using Darl_standard.Darl.Thinkbase;
using GraphQL;
using Microsoft.Extensions.Caching.Distributed;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    [ProtoContract]
    public class BlobGraphPrimitives : IGraphPrimitives
    {

        private IBlobConnectivity _blob;
        private IDistributedCache _cache;

        private static TimeSpan cacheExpiration = new TimeSpan(0, 30, 0);

        private Dictionary<string, BlobGraphContent> buffer = new Dictionary<string, BlobGraphContent>();

        public BlobGraphPrimitives(IBlobConnectivity blob, IDistributedCache cache)
        {
            _blob = blob;
            _cache = cache;
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
            return gc;
        }

        public async Task<GraphObject> CreateObject(string compositeName, GraphObjectInput graphObject)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            var go = new GraphObject { existence = graphObject.existence, externalId = graphObject.externalId, id = Guid.NewGuid().ToString(), inferred = false, lineage = graphObject.lineage, name = graphObject.name, _virtual = false, properties = graphObject.properties };
            cont.vertices.Add(go.id, go);
            return go;
        }

        public async Task CreateVirtualConnection(GraphModel model, string child, string lineage, string connectionLabel)
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

        public async Task CreateVirtualObject(GraphModel model, string lineage, string typeword, string description)
        {
            var go = new GraphObject { id = Guid.NewGuid().ToString(), inferred = false, lineage = lineage, name = typeword, _virtual = true, properties = new List<StringStringPair> { new StringStringPair("description",description) } };
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
                    cont.vertices[c.endId].In.Remove(c);
                }
                cont.edges.Remove(c.id);
            }
            foreach (var c in node.In)
            {
                if (cont.vertices.ContainsKey(c.startId))
                {
                    cont.vertices[c.startId].In.Remove(c);
                }
                cont.edges.Remove(c.id);
            }
            cont.vertices.Remove(id);
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

        public async Task<bool> LineageExists(GraphModel model,string lineage)
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
                node.properties = go.properties;
                changed = true;
            }
            if (changed)
            {
                FlagChanges(compositeName);
            }
            return node;
        }

        public async Task<bool> VirtualAssociationExists(GraphModel model, string lineage1, string lineage2)
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

        private void FollowHypernymy(GraphModel model, GraphObject g, List<GraphObject> list)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            foreach (var l in g.Out.Where(a => a.name == "kind_of"))
            {
                var parent = cont.virtualVertices[l.endId];
                list.Add(parent);
                FollowHypernymy(model, parent, list);
            }
        }

        public async Task Store(string blobName, GraphModel model)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            await _blob.Write(blobName, SerializeGraph(cont));
        }

        public async Task<GraphModel> Load(string blobName)
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

        public byte[] SerializeGraph(GraphModel model)
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
            return await _blob.Delete(compositeName);
        }

        public async Task<List<string>> ListModels(string userId)
        {
            return _blob.List(userId);
        }

 
        public async Task<List<GraphElement>> ProcessPath(string compositeName, string startExternalID, string endExternalID)
        {
            var cont = await Load(compositeName);
            var start = await GetGraphObjectByExternalId(compositeName, startExternalID);
            var target = await GetGraphObjectByExternalId(compositeName, endExternalID);
            return ShortestPath(cont, start, target);
        }

        public async Task<List<StringStringPair>> GetLinkedCategories(string compositeName, string rootExternalID, string childLineage, string childValueAttribute)
        {
            var list = new List<StringStringPair>();
            var obj = await GetGraphObjectByExternalId(compositeName, rootExternalID);
            foreach(var c in obj.Out)
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
                    if (obj.properties != null && obj.properties.Where(a => a.Name == propertyName).Any())
                    {
                        return obj.properties.Where(a => a.Name == propertyName).First().Value;
                    }
                    return string.Empty;
            }
        }

        private void FlagChanges(string compositeName)
        {

        }

        /// <summary>
        /// Dijkstra's shortest path algorithm
        /// </summary>
        /// <param name="model"></param>
        /// <param name="start"></param>
        /// <param name="Target"></param>
        /// <returns>the vertex,edge,vertex... sequence</returns>
        /// <remarks></remarks>
        public List<GraphElement> ShortestPath(GraphModel model, GraphObject start, GraphObject target)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            var list = new List<GraphElement>{ start};
            var coverage = new Dictionary<GraphObject, (double, bool)> { {start,(0,true) } };
            return ShortestPathRecursion(model, start, target, list, coverage);
        }

        private List<GraphElement> ShortestPathRecursion(GraphModel model, GraphObject start, GraphObject target, List<GraphElement> list, Dictionary<GraphObject, (double, bool)> coverage)
        {
            BlobGraphContent cont = model as BlobGraphContent;
            List<GraphElement> bestFound = null;
            foreach (var c in start.Out)
            {
                var next = cont.vertices[c.endId];
                if(coverage.ContainsKey(next))
                {
                    return null;
                }
                list.Add(c);
                list.Add(next);
                coverage.Add(next, (coverage[start].Item1 + c.weight, true));
                if (next == target)
                    return list;
                var found = ShortestPathRecursion(model, next, target, new List<GraphElement>(list), coverage);
                if (found != null && found.Count < (bestFound == null ? double.MaxValue : bestFound.Count))
                    bestFound = found;
            }
            return bestFound;
        }

        public async Task<List<GraphObject>> GetGraphObjectsByLineage(string compositeName, string lineage)
        {
            var cont = await Load(compositeName) as BlobGraphContent;
            return cont.vertices.Values.Where(a => a.lineage == lineage).ToList();
        }
    }
}
