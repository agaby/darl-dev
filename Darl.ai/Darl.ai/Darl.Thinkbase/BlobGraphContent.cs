using Darl.Common;
using Darl.Licensing;
using Darl.Lineage;
using Darl.Thinkbase.Meta;
using DarlCommon;
using ProtoBuf;
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
        public string modelName { get; set; }

        private static readonly DarlMetaRunTime runtime = new DarlMetaRunTime(new MetaStructureHandler());


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
        public string key { get; set; }
        [ProtoMember(11)]
        public string description { get; set; }
        [ProtoMember(12)]
        public string initialText { get; set; }

        [ProtoMember(13)]
        public string author { get; set; }

        [ProtoMember(14)]
        public string copyright { get; set; }

        [ProtoMember(15)]
        public string licenseUrl { get; set; }
        public bool licensed { get; private set; } = true; //default for legacy
        [ProtoMember(16)]
        public IGraphModel.DateDisplay? dateDisplay { get; set; }
        [ProtoMember(17)]
        public IGraphModel.InferenceTime? inferenceTime { get; set; }
        [ProtoMember(18)]
        public DarlTime? fixedTime { get; set; }

        public static BlobGraphContent Load(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                var res = Serializer.Deserialize<BlobGraphContent>(ms);
                if (!string.IsNullOrEmpty(res.key))
                {
                    DarlLicense.license = res.key;
                    if (!DarlLicense.licensed)
                    {
                        res.licensed = false;
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Finds a ruleset of a particular kind looking on the given node, it's virtual counterpart or up the hypernymy tree.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lineage"></param>
        /// <returns></returns>
        public string FindControlAttribute(string id, string lineage)
        {
            if (!vertices.ContainsKey(id))
                return null;
            var obj = vertices[id];
            if (obj.properties != null)
            {
                var att = obj.properties.Where(a => a.lineage.StartsWith(lineage)).FirstOrDefault(); //check name or lineage?
                if (att != null)
                {
                    try
                    {
                        runtime.CreateTree(att.value, obj, this);
                        return att.value;
                    }
                    catch
                    {

                    }
                }
            }
            //no local value, try virtual
            if (!virtualVertices.ContainsKey(obj.lineage))
            {
                return null;
            }
            var virtNode = virtualVertices[obj.lineage];
            var list1 = new List<GraphObject> { virtNode };
            FollowHypernymy(virtNode, list1);
            string ruleSource = string.Empty;
            bool found = false;
            foreach (var l in list1)
            {
                if (l.properties != null)
                {
                    foreach (var p in l.properties)
                    {
                        if (p.lineage.StartsWith(lineage))
                        {
                            ruleSource = p.value;
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                    break;
            }
            if (ruleSource == string.Empty)
                return null;
            try
            {
                runtime.CreateTree(ruleSource, obj, this);
                return ruleSource;
            }
            catch
            {
                return null;
            }
        }

        public DarlVar FindDataAttribute(string id, string lineage, KnowledgeState ks)
        {
            var att = FindDataGraphAttribute(id, lineage, ks);
            return att != null ? att.Convert() : null;
        }

        public GraphAttribute FindDataGraphAttribute(string id, string lineage, KnowledgeState ks)
        {
            //look in the ks
            if (ks != null && ks.ContainsRecord(id))
            {
                var att = ks.GetAttribute(id, lineage);
                if (att != null)
                    return att;
            }
            if (!vertices.ContainsKey(id))
                return null;
            //then in the object
            var obj = vertices[id];
            if (obj.properties != null)
            {
                var oatt = obj.properties.Where(a => a.lineage.StartsWith(lineage)).FirstOrDefault();
                if (oatt != null)
                    return oatt;
            }
            //finally in the virtual realm - generic values
            var virtNode = virtualVertices[obj.lineage];
            var list1 = new List<GraphObject> { virtNode };
            FollowHypernymy(virtNode, list1);
            GraphAttribute data = null;
            bool found = false;
            foreach (var l in list1)
            {
                if (l.properties != null)
                {
                    foreach (var p in l.properties)
                    {
                        if (p.lineage.StartsWith(lineage))
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

        public List<DarlTime?> FindAttributeExistence(string id, string lineage, KnowledgeState ks)
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
            var oatt = obj.properties.Where(a => a.lineage.StartsWith(lineage)).FirstOrDefault();
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
                    if (obj.lineage.StartsWith(objectLineage))
                        list.Add(obj);
                }
            }
            return list;
        }

        private void FollowHypernymy(GraphObject g, List<GraphObject> list)
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
            List<string> lineages = new List<string>();
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
    }
}
