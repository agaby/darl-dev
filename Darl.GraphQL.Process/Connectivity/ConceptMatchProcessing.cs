using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using GraphQL;
using Gremlin.Net.Structure;
using Microsoft.Extensions.Logging;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class ConceptMatchProcessing : IConceptMapProcessing
    {
        private IBlobConnectivity _blob;
        private ILogger _logger;
        public ConceptMatchProcessing(IBlobConnectivity blob, ILogger<ConceptMatchProcessing> logger)
        {
            _blob = blob;
            _logger = logger;
        }

        public async Task<string> CreateConceptMatchTree(string userId, string treeName, List<StringStringPair> data, bool rebuild = false)
        {
            MatchGraph graph;
            LineageNode nounRoot;
            LineageNode verbRoot;
            var blobName = GenerateGraphName(userId, treeName);
            if (!rebuild && await _blob.Exists(blobName))
            {
                graph = MatchGraph.DeserializeGraph(await _blob.Read(blobName));
                nounRoot = graph.Vertices.Where(a => a.lineageElement == "noun:").FirstOrDefault();
                verbRoot = graph.Vertices.Where(a => a.lineageElement == "verb:").FirstOrDefault();
            }
            else
            {
                graph = new MatchGraph();
                nounRoot = new LineageNode { lineageElement = "noun:" };
                verbRoot = new LineageNode { lineageElement = "verb:" };
                graph.AddVertex(nounRoot);
                graph.AddVertex(verbRoot);
            }

            foreach (var d in data)
            {
                var words = LineageLibrary.SimpleTokenizer(d.Value.Replace('"',' '));
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
                            currentNode = GetOrCreateNode(l.lineage, currentNode, graph);
                            if (currentNode.indexes == null)
                                currentNode.indexes = new List<string>();
                            currentNode.indexes.Add(d.Name);
                        }
                        if(l.lineage.StartsWith("proper_noun"))
                        {
                            var word = words[index - 1];
                            if(!graph.properNouns.ContainsKey(word))
                            {
                                graph.properNouns.Add(word, new List<string>());
                            }
                            graph.properNouns[word].Add(d.Name);
                        }
                    }
                }
            }
            await _blob.Write(blobName, graph.SerializeGraph());
            return $"Match Model {treeName} created containing {data.Count()} texts.";
        }

        private string GenerateGraphName(string userId, string treeName)
        {
            return userId + "_" + treeName.Replace(" ", "_");
        }


        private LineageNode GetOrCreateNode(string lineage, LineageNode currentNode, MatchGraph graph, bool temp = false)
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
                    graph.AddVertex(t);
                    graph.AddEdge(e);
                    if (temp)
                    {
                        graph.tempObjects.Add(t);
                        graph.tempEdges.Add(e);
                    }
                }
                currentNode = currentNode.edges[val].Target;
            }
            return currentNode;
        }

        public async Task<List<MatchResult>> InferFromConceptMatchTree(string userId, string treeName, List<string> texts)
        {
            var blobName = GenerateGraphName(userId, treeName);

            if(!await _blob.Exists(blobName))
                throw new ExecutionError($"Concept match tree {treeName} not found in this account.");
            var graph = MatchGraph.DeserializeGraph(await _blob.Read(blobName));
            var responses = new List<MatchResult>(texts.Count);
            int index = 0;
            foreach (var text in texts)
            {
                responses.Add(Find(text, graph));
                graph.Flush();
                index++;
            }
            return responses;
        }

        public MatchResult Find(string example, MatchGraph graph)
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
                        startNode = graph.nounRoot;
                        nounAndVerbCount++;
                    }
                    if (l.lineage.StartsWith("verb"))
                    {
                        startNode = graph.verbRoot;
                        nounAndVerbCount++;
                    }
                    if (startNode != null)
                    {
                        var found = new List<LineageNode>();
                        startNode = GetOrCreateNode(l.lineage, startNode, graph, true);
                        var algo = new BreadthFirstSearchAlgorithm<LineageNode, LineageEdge>(graph);
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
                        if(graph.properNouns.ContainsKey(word))
                        {
                            var weight = Weight(graph.properNouns[word].Count);
                            foreach (var i in graph.properNouns[word])
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
            foreach(var c in sorted)
            {
                if (c.matchedWords != r.matchedWords)
                    break;
                r.tieCount++;
                count++;
            }
            for(int n = 1; n < Math.Min(4,sorted.Count); n++)
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

        private LineageNode FindNearest(List<LineageNode> downs, LineageNode up, MatchGraph graph, int depth = 0 )
        {
            if (depth == 0) //initial state
            {
                if (downs.Count != 1)
                    throw new ExecutionError("Find nearest starts with the root node");
                if (graph.TryGetInEdges(downs[0], out IEnumerable<LineageEdge> edges))
                {
                    up = edges.First().Source;
                }
            }
            else
            {
                if (up.lineageElement == "noun:" || up.lineageElement == "verb:")
                    up = null;
                if (up != null)
                {
                    if (graph.TryGetInEdges(up, out IEnumerable<LineageEdge> edges))
                    {
                        if(edges.Count() > 0)
                            up = edges.First().Source;
                    }
                }
                else
                {
                    if (downs.Count == 0)
                        return null; //cant find anything
                }
            }
            if(up != null)
            {
                if (up.indexes != null && up.indexes.Count > 0)
                    return up;
            }
            var newDowns = new List<LineageNode>();
            foreach (var d in downs)
            {
                foreach (var e in d.edges.Values)
                {
                    if (e.Target.indexes != null && e.Target.indexes.Count > 0)
                        return e.Target;
                    newDowns.Add(e.Target);
                }
            }
            return FindNearest(newDowns, up, graph, ++depth);
        }
    }
}
