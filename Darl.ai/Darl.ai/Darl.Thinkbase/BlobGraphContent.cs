/// <summary>
/// </summary>

﻿using Darl.Common;
using Darl.Licensing;
using Darl.Lineage;
using DarlCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Darl.Thinkbase
{
    [ProtoContract]
    public class BlobGraphContent : IGraphModel
    {
        /// <summary>
        /// real objects indexed by id
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<string, GraphObject> vertices { get; set; } = new Dictionary<string, GraphObject>();

        /// <summary>
        /// real connections
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<string, GraphConnection> edges { get; set; } = new Dictionary<string, GraphConnection>();

        /// <summary>
        /// virtual vertices indexed by lineage
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<string, GraphObject> virtualVertices { get; set; } = new Dictionary<string, GraphObject>();

        /// <summary>
        /// virtual edges 
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<string, GraphConnection> virtualEdges { get; set; } = new Dictionary<string, GraphConnection>();
        [ProtoMember(10)]
        public string modelName { get; set; } = String.Empty;

        /// <summary>
        /// Recognition DAG networks  in this model, indexed by lineage
        /// The top level network has the index "default:"
        /// </summary>
        [ProtoMember(5)]
        public Dictionary<string, GraphObject> recognitionRoots { get; set; } = new Dictionary<string, GraphObject>();
        [ProtoMember(6)]
        public Dictionary<string, GraphObject> recognitionVertices { get; set; } = new Dictionary<string, GraphObject>();
        [ProtoMember(7)]
        public Dictionary<string, GraphConnection> recognitionEdges { get; set; } = new Dictionary<string, GraphConnection>();
        [ProtoMember(8)]
        public Dictionary<string, IDynamicConverter> dynamicSources { get; set; } = new Dictionary<string, IDynamicConverter>();

        /// <summary>
        /// key used for verification of source
        /// </summary>
        [ProtoMember(9)]
        public string key { get; set; } = String.Empty;
        [ProtoMember(11)]
        public string description { get; set; } = String.Empty;
        [ProtoMember(12)]
        public string initialText { get; set; } = String.Empty;

        [ProtoMember(13)]
        public string author { get; set; } = String.Empty;

        [ProtoMember(14)]
        public string copyright { get; set; } = String.Empty;

        [ProtoMember(15)]
        public string licenseUrl { get; set; } = String.Empty;
        public bool licensed { get; private set; } = true; //default for legacy
        [ProtoMember(16)]
        public IGraphModel.DateDisplay? dateDisplay { get; set; }
        [ProtoMember(17)]
        public IGraphModel.InferenceTime? inferenceTime { get; set; }
        [ProtoMember(18)]
        public DarlTime? fixedTime { get; set; }
        [ProtoMember(19)]
        public bool transient { get; set; } = false;
        [ProtoMember(20)]
        public string? defaultTarget { get; set; }

        public static BlobGraphContent? Load(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                try
                {
                    var res = Serializer.Deserialize<BlobGraphContent>(ms);
                    if (!string.IsNullOrEmpty(res.key))
                    {
                        DarlLicense.license = res.key;
                        if (!DarlLicense.licensed)
                        {
                            res.licensed = false;
                        }
                    }
                    res.SanityCheck();
                    return res;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return null;
            }
        }

        /// <summary>
        /// remove disconnected edges
        /// </summary>
        public void SanityCheck()
        {
            var orphanRealConnections = new List<GraphConnection>();
            foreach (var c in edges.Values)
            {
                if (!vertices.ContainsKey(c.startId) || !vertices.ContainsKey(c.endId))
                {
                    //remove from the end still connected, if any
                    if (vertices.ContainsKey(c.startId))
                        vertices[c.startId].Out.Remove(c);
                    else if (vertices.ContainsKey(c.endId))
                        vertices[c.endId].In.Remove(c);
                    orphanRealConnections.Add(c);
                }
            }
            foreach (var c in orphanRealConnections)
                edges.Remove(c.id);
            foreach (var v in vertices.Values)
            {
                var orphanOutEdges = new List<GraphConnection>();
                foreach (var e in v.Out)
                {
                    if (!edges.ContainsKey(e.id))
                    {
                        orphanOutEdges.Add(e);
                    }
                }
                foreach (var e in orphanOutEdges)
                    v.Out.Remove(e);
                var orphanInEdges = new List<GraphConnection>();
                foreach (var e in v.In)
                {
                    if (!edges.ContainsKey(e.id))
                    {
                        orphanInEdges.Add(e);
                    }
                }
                foreach (var e in orphanInEdges)
                    v.In.Remove(e);
            }

            var orphanVirtualConnections = new List<GraphConnection>();
            foreach (var c in virtualEdges.Values)
            {
                if (!virtualVertices.ContainsKey(c.startId) || !virtualVertices.ContainsKey(c.endId))
                {
                    //remove from the end still connected, if any
                    if (virtualVertices.ContainsKey(c.startId))
                        virtualVertices[c.startId].Out.Remove(c);
                    else if (virtualVertices.ContainsKey(c.endId))
                        virtualVertices[c.endId].In.Remove(c);
                    orphanRealConnections.Add(c);
                }
            }
            foreach (var c in orphanVirtualConnections)
                virtualEdges.Remove(c.id);
            var orphanRecognitionConnections = new List<GraphConnection>();
            foreach (var c in recognitionEdges.Values)
            {
                if (!recognitionVertices.ContainsKey(c.startId) || !recognitionVertices.ContainsKey(c.endId))
                {
                    //remove from the end still connected, if any
                    if (recognitionVertices.ContainsKey(c.startId))
                        recognitionVertices[c.startId].Out.Remove(c);
                    else if (recognitionVertices.ContainsKey(c.endId))
                        recognitionVertices[c.endId].In.Remove(c);
                    orphanRecognitionConnections.Add(c);
                }
            }
            foreach (var c in orphanRecognitionConnections)
                virtualEdges.Remove(c.id);
        }

        /// <summary>
        /// Finds a ruleset of a particular kind looking on the given node, it's virtual counterpart or up the hypernymy tree.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lineage"></param>
        /// <returns></returns>
        public (string?,string?) FindControlAttribute(string id)
        {
            if (!vertices.ContainsKey(id))
                return (null,null);
            var obj = vertices[id];
            if (obj.properties != null)
            {
                var att = obj.properties.Where(a => a.type == GraphAttribute.DataType.ruleset).FirstOrDefault(); //check name or lineage?
                if (att != null)
                {
                    return (att.value,id);
                }
            }
            //no local value, try virtual
            if (obj.lineage == null || !virtualVertices.ContainsKey(obj.lineage))
            {
                return (null,null);
            }
            var virtNode = virtualVertices[obj.lineage];
            var list1 = new List<GraphObject> { virtNode };
            FollowHypernymy(virtNode, list1);
            string ruleSource = string.Empty;
            string? found = null;
            foreach (var l in list1)
            {
                if (l.properties != null)
                {
                    foreach (var p in l.properties)
                    {
                        if (p.lineage != null /* && p.lineage.StartsWith(lineage)*/ && p.type == GraphAttribute.DataType.ruleset)
                        {
                            ruleSource = p.value;
                            found = l.id;
                            break;
                        }
                    }
                }
                if (found != null)
                    break;
            }
            if (ruleSource == string.Empty)
                return (null,null);
            return (ruleSource,found);
        }

        public DarlVar? FindDataAttribute(string id, string lineage, KnowledgeState ks)
        {
            var att = FindDataGraphAttribute(id, lineage, ks);
            if (att != null)
            {
                var obj = vertices.ContainsKey(id) ? vertices[id] : vertices.Values.FirstOrDefault(a => a.externalId == id);
                if (obj != null)
                {
                    var r = att.Convert();
                    r.name = obj.externalId;
                    return r;
                }
            }
            return null;
        }

        public GraphAttribute? FindDataGraphAttribute(string id, string lineage, KnowledgeState ks)
        {
            //id could be externalId, check
            GraphObject? obj = null;
            if (!vertices.ContainsKey(id))
            {
                obj = vertices.Values.FirstOrDefault(a => a.externalId == id);
                if (obj == null)
                    return null;
                id = obj.id ?? "";
            }
            else
            {
                obj = vertices[id];
            }
            //look in the ks
            if (ks != null && ks.ContainsRecord(id))
            {
                var att = ks.GetAttribute(id, lineage);
                if (att != null)
                {
                    return att;
                }
            }
            //then in the object
            if (obj.properties != null)
            {
                var oatt = obj.properties.Where(a => a.lineage != null && a.lineage.StartsWith(lineage)).FirstOrDefault();
                if (oatt != null)
                    return oatt;
            }
            //finally in the virtual realm - generic values
            if (obj.lineage == null)
                return null;
            var virtNode = virtualVertices[obj.lineage];
            var list1 = new List<GraphObject> { virtNode };
            FollowHypernymy(virtNode, list1);
            GraphAttribute? data = null;
            bool found = false;
            foreach (var l in list1)
            {
                if (l.properties != null)
                {
                    foreach (var p in l.properties)
                    {
                        if (p.lineage != null && p.lineage.StartsWith(lineage))
                        {
                            data = p;
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                    break;
            }
            return data;
        }

        public List<DarlTime?>? FindAttributeExistence(string id, string lineage, KnowledgeState ks)
        {
            if (ks != null && ks.ContainsRecord(id))
            {
                var att = ks.GetAttribute(id, lineage);
                if (att != null)
                    return att.existence;
            }
            if (!vertices.ContainsKey(id))
                return null;
            //then in the object
            var obj = vertices[id];
            var oatt = obj.properties != null ? obj.properties.Where(a => a.lineage != null && a.lineage.StartsWith(lineage)).FirstOrDefault() : null;
            if (oatt != null)
                return oatt.existence;
            return null;
        }

        public List<GraphObject> GetConnectedObjects(GraphObject node, string connectionLineage, string objectLineage)
        {
            var list = new List<GraphObject>();
            foreach (var i in node.Out)
            {
                if (!string.IsNullOrEmpty(i.lineage) && i.lineage.StartsWith(connectionLineage))
                {
                    var obj = vertices[i.endId];
                    if (obj.lineage != null && obj.lineage.StartsWith(objectLineage))
                        list.Add(obj);
                }
            }
            /*            foreach (var i in node.In)
                        {
                            if (!string.IsNullOrEmpty(i.lineage) && i.lineage.StartsWith(connectionLineage))
                            {
                                var obj = vertices[i.startId];
                                if (obj.lineage != null && obj.lineage.StartsWith(objectLineage))
                                    list.Add(obj);
                            }
                        }*/
            return list;
        }

        public void FollowHypernymy(GraphObject g, List<GraphObject> list)
        {
            foreach (var l in g.Out.Where(a => a.name == "kind_of"))
            {
                var parent = virtualVertices[l.endId];
                list.Add(parent);
                FollowHypernymy(parent, list);
            }
        }


        public List<LineageRecord> GetLineages(GraphElementType gtype)
        {
            var lineages = new List<string?>();
            switch (gtype)
            {
                case GraphElementType.node:
                    lineages = vertices.Values.Select(a => a.lineage).Distinct().ToList();
                    break;
                case GraphElementType.connection:
                    lineages = edges.Values.Select(a => a.lineage).Distinct().ToList();
                    break;
                case GraphElementType.attribute:
                    lineages = vertices.Values.Select(a => (a.properties ?? new List<GraphAttribute>()).Select(b => b.lineage).ToList()).SelectMany(i => i).Distinct().ToList();
                    break;
            }
            var records = new List<LineageRecord>();
            foreach (var l in lineages)
            {
                if (LineageLibrary.lineages.ContainsKey(l))
                    records.Add(LineageLibrary.lineages[l]);
                else
                {
                    var lins = SplitCompositeLineage(l);
                    if (lins.Item2 != null)
                    {
                        if (LineageLibrary.lineages.ContainsKey(lins.Item1) && LineageLibrary.lineages.ContainsKey(lins.Item2))
                        {
                            var main = LineageLibrary.lineages[lins.Item1];
                            var sub = LineageLibrary.lineages[lins.Item2];
                            records.Add(main);
                            records.Add(sub);
                            //add the composite version too.
                            records.Add(new LineageRecord { lineage = l, typeWord = $"{main.typeWord}_{sub.typeWord}", type = LineageType.composite });
                        }
                    }
                }
            }
            return records.Distinct().ToList();
        }

        public (string, string) SplitCompositeLineage(string comp)
        {
            var divider = comp.IndexOf('+');
            var lin = divider == -1 ? comp : comp.Substring(0, divider);
            if (divider != -1) //composite - we've added the primary part, now add the secondary
            {
                var sub = comp.Substring(divider + 1);
                return (lin, sub);
            }
            return (lin, null);
        }

        public void Clear()
        {
            vertices.Clear();
            edges.Clear();
            recognitionEdges.Clear();
            recognitionVertices.Clear();
            recognitionRoots.Clear();
            virtualEdges.Clear();
            virtualVertices.Clear();
            dynamicSources.Clear();
        }

        public void AddDefaultContent()
        {
            var defaultroot = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "default:", name = "root" };
            recognitionVertices.Add(defaultroot.id, defaultroot);
            recognitionRoots.Add("default:", defaultroot);
            var navigationRoot = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "navigation:", name = "root" };
            recognitionVertices.Add(navigationRoot.id, navigationRoot);
            recognitionRoots.Add("navigation:", navigationRoot);
            var obj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "default:", name = "default", properties = new List<GraphAttribute> { new GraphAttribute { id = Guid.NewGuid().ToString(), type = GraphAttribute.DataType.ruleset, value = "output textual response;\nif anything then response will be \"I don't know the answer to that. Please type 'help' to get instructions to use this knowledge graph.\";", lineage = "adjective:8953" } } };
            recognitionVertices.Add(obj.id, obj);
            var conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj.id, startId = defaultroot.id, weight = 1.0 };
            recognitionVertices[conn.startId].Out.Add(conn);
            recognitionVertices[conn.endId].In.Add(conn);
            recognitionEdges.Add(conn.id, conn);
            obj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "verb:397,2", name = "help", properties = new List<GraphAttribute> { new GraphAttribute { id = Guid.NewGuid().ToString(), type = GraphAttribute.DataType.ruleset, value = "output textual response;\nif anything then response will be \"Just select a response for each question. Type \"quit\" if you want to stop.\"; ", lineage = "adjective:8953" } } };
            recognitionVertices.Add(obj.id, obj);
            conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj.id, startId = navigationRoot.id, weight = 1.0 };
            recognitionVertices[conn.startId].Out.Add(conn);
            recognitionVertices[conn.endId].In.Add(conn);
            recognitionEdges.Add(conn.id, conn);
            obj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "verb:060", name = "quit", properties = new List<GraphAttribute> { new GraphAttribute { id = Guid.NewGuid().ToString(), type = GraphAttribute.DataType.ruleset, value = "output categorical terminate {\"true\",\"false\"};\nif anything then terminate will be true; ", lineage = "adjective:8953" } } };
            recognitionVertices.Add(obj.id, obj);
            conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj.id, startId = navigationRoot.id, weight = 1.0 };
            recognitionVertices[conn.startId].Out.Add(conn);
            recognitionVertices[conn.endId].In.Add(conn);
            recognitionEdges.Add(conn.id, conn);
        }
    }
}
