using Darl.Lineage;
using GraphQL;
using ProtoBuf;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    
    public class MatchGraph : QuickGraph.BidirectionalGraph<LineageNode, LineageEdge>
    {

        public List<LineageNode> tempObjects { get; set; } = new List<LineageNode>();
        public List<LineageEdge> tempEdges { get; set; } = new List<LineageEdge>();

        public LineageNode nounRoot { get; set; }

        public LineageNode verbRoot { get; set; }

        public MatchGraph()
        {
            nounRoot = new LineageNode { lineageElement = "noun:" };
            verbRoot = new LineageNode { lineageElement = "verb:" };
            AddVertex(nounRoot);
            AddVertex(verbRoot);
        }


        public Dictionary<string, List<string>> properNouns { get; set; } = new Dictionary<string, List<string>>();
        public static MatchGraph DeserializeGraph(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                var mgp = Serializer.Deserialize<MatchGraphProxy>(ms);
                var mg = new MatchGraph();
                foreach(var e in mgp.edges)
                {
                    mg.AddVerticesAndEdge(e);
                }
                foreach(var le in mg.Edges)
                {
                    le.Source.edges.Add(le.Target.lineageElement, le);
                }
                mg.properNouns = mgp.properNouns;
                mg.nounRoot = mg.Vertices.Where(a => a.lineageElement == "noun:").FirstOrDefault();
                mg.verbRoot = mg.Vertices.Where(a => a.lineageElement == "verb:").FirstOrDefault();
                return mg;
            }
        }

        public byte[] SerializeGraph()
        {
            var mgp = new MatchGraphProxy();
            mgp.nodes.AddRange(Vertices);
            mgp.edges.AddRange(Edges);
            mgp.properNouns = properNouns;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<MatchGraphProxy>(ms, mgp);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        //remove temp nodes
        public void Flush()
        {
            foreach (var e in tempEdges)
            {
                e.Source.edges.Remove(e.Target.lineageElement);
                this.RemoveEdge(e);
            }
            foreach (var v in tempObjects)
                this.RemoveVertex(v);
            tempObjects.Clear();
            tempEdges.Clear();
        }

        public void CreateTree(List<StringStringPair> data)
        {
            foreach (var d in data)
            {
                var words = LineageLibrary.SimpleTokenizer(d.Value.Replace('"', ' '));
                int index = 0;
                while (index < words.Count)
                {
                    var lineages = LineageLibrary.WordRecognizer(words, ref index);
                    foreach (var l in lineages)
                    {
                        LineageNode currentNode = null;
                        if (l.lineage.StartsWith("noun")) //only index nouns and verbs
                        {
                            currentNode = nounRoot;
                        }
                        else if (l.lineage.StartsWith("verb"))
                        {
                            currentNode = verbRoot;
                        }
                        if (currentNode != null)
                        {
                            currentNode = GetOrCreateNode(l.lineage, currentNode);
                            if (currentNode.indexes == null)
                                currentNode.indexes = new List<string>();
                            currentNode.indexes.Add(d.Name);
                        }
                        if (l.lineage.StartsWith("proper_noun"))
                        {
                            var word = words[index - 1];
                            if (!properNouns.ContainsKey(word))
                            {
                                properNouns.Add(word, new List<string>());
                            }
                            properNouns[word].Add(d.Name);
                        }
                    }
                }
            }

        }

        private LineageNode GetOrCreateNode(string lineage, LineageNode currentNode, bool temp = false)
        {
            var mantissa = lineage.Substring(5);
            var elements = mantissa.Split(',');
            foreach (var val in elements)
            {
                if (!currentNode.edges.ContainsKey(val))
                {
                    var t = new LineageNode { lineageElement = val, temp = temp };
                    var e = new LineageEdge { Source = currentNode, Target = t };
                    currentNode.edges.Add(val, e);
                    AddVertex(t);
                    AddEdge(e);
                    if (temp)
                    {
                        tempObjects.Add(t);
                        tempEdges.Add(e);
                    }
                }
                currentNode = currentNode.edges[val].Target;
            }
            return currentNode;
        }

        public MatchResult Find(string example)
        {
            var words = LineageLibrary.SimpleTokenizer(example);
            int index = 0;
            var list = new Dictionary<string, MatchResult>();
            int nounAndVerbCount = 0;
            while (index < words.Count)
            {
                var lineages = LineageLibrary.WordRecognizer(words, ref index);
                foreach (var l in lineages)
                {
                    //find start point for search
                    LineageNode startNode = null;
                    if (l.lineage.StartsWith("noun")) //only index nouns and verbs
                    {
                        startNode = nounRoot;
                        nounAndVerbCount++;
                    }
                    if (l.lineage.StartsWith("verb"))
                    {
                        startNode = verbRoot;
                        nounAndVerbCount++;
                    }
                    if (startNode != null)
                    {
                        var found = new List<LineageNode>();
                        startNode = GetOrCreateNode(l.lineage, startNode, true);
                        var algo = new BreadthFirstSearchAlgorithm<LineageNode, LineageEdge>(this);
                        algo.DiscoverVertex += delegate (LineageNode ln) { Algo_DiscoverVertex(ln, found, algo); };
                        algo.Compute(startNode);
                        if (found.Any())
                        {
                            var closest = found.First();
                            var weight = Weight(closest.indexes.Count);
                            foreach (var i in closest.indexes)
                            {
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                            }
                        }
                        /*                        var closest = FindNearest(new List<LineageNode> { startNode }, null, graph);
                                                if (closest != null)
                                                {
                                                    var weight = Weight(closest.indexes.Count);
                                                    foreach (var i in closest.indexes)
                                                    {
                                                        if (!list.ContainsKey(i))
                                                            list.Add(i, new MatchResult { sourceText = example, index = i });
                                                        list[i].matchedWords += weight;
                                                    }
                                                }*/
                    }
                    if (l.lineage.StartsWith("proper_noun"))
                    {
                        var word = words[index - 1];
                        if (properNouns.ContainsKey(word))
                        {
                            var weight = Weight(properNouns[word].Count);
                            foreach (var i in properNouns[word])
                            {
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                            }
                        }
                    }
                }
            }
            if (list.Count == 0)
                return null;
            //first pass return most matched words
            var sorted = list.Values.OrderByDescending(a => a.matchedWords).ToList();
            var r = sorted.First();
            int count = 0;
            foreach (var c in sorted)
            {
                if (c.matchedWords != r.matchedWords)
                    break;
                r.tieCount++;
                count++;
            }
            for (int n = 1; n < Math.Min(4, sorted.Count); n++)
            {
                var c = sorted[n];
                r.alternatives.Add(c.index, c.matchedWords);
            }
            r.confidence = (double)r.matchedWords / (double)nounAndVerbCount;
            return r;
        }

        private double Weight(int count)
        {
            //return 1.0 / Math.Log(count);
            return 1.0 / (double)count;
        }

        private void Algo_DiscoverVertex(LineageNode vertex, List<LineageNode> found, BreadthFirstSearchAlgorithm<LineageNode, LineageEdge> algo)
        {
            var graph = algo.VisitedGraph as MatchGraph;
            algo.VertexColors[graph.nounRoot] = QuickGraph.GraphColor.Black;
            algo.VertexColors[graph.verbRoot] = QuickGraph.GraphColor.Black;
            if (vertex.indexes != null)
            {
                found.Add(vertex);
            }
        }



    }
}
