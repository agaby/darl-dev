using Darl.GraphQL.Process.Web.Models.Noda;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaDocument: ILayoutable
    {
        public string name { get; set; } = "thinkbase.graph";
        public string format { get; set; } = "v2";
        public List<NodaNode> metaNodes { get; set; } = new List<NodaNode>();
        public List<NodaLink> metaLinks { get; set; } = new List<NodaLink>();
        public List<NodaNode> nodes { get; set; } = new List<NodaNode>();
        public List<NodaLink> links { get; set; } = new List<NodaLink>();

        private Dictionary<string, NodaNode>? nodeLookup { get; set; }

        private Dictionary<string, Dictionary<string, NodaLink>> linkLookup { get; set; } = new Dictionary<string, Dictionary<string, NodaLink>>();

        public void Init()
        {
            nodeLookup = nodes.ToDictionary(a => a.uuid);
            foreach (var l in links)
            {
                if (!linkLookup.ContainsKey(l.fromNode.Uuid))
                {
                    linkLookup.Add(l.fromNode.Uuid, new Dictionary<string, NodaLink>());
                }
                if (!linkLookup[l.fromNode.Uuid].ContainsKey(l.toNode.Uuid))
                {
                    linkLookup[l.fromNode.Uuid].Add(l.toNode.Uuid, l);
                }
            }
        }

        public ILayoutLink? GetEdge(string fromNode, string toNode)
        {
            if (linkLookup.ContainsKey(fromNode))
            {
                if (linkLookup[fromNode].ContainsKey(toNode))
                    return linkLookup[fromNode][toNode];
            }
            return null;
        }

        public ILayoutNode? GetNode(string uuid)
        {
            if (nodeLookup != null && nodeLookup.ContainsKey(uuid))
                return nodeLookup[uuid];
            return null;
        }

        internal void Clear()
        {
            throw new NotImplementedException();
        }

        public List<ILayoutNode> GetNodes()
        {
            return (List<ILayoutNode>)nodes.Select(a => a as ILayoutNode);
        }

        public List<ILayoutLink> GetLinks()
        {
            throw new NotImplementedException();
        }
    }
}
