using GraphQL;
using ProtoBuf;
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
    }
}
