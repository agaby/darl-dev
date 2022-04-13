using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public class NodaViewDocument : ILayoutable
    {
        public string name { get; set; }
        public string description { get; set; } = string.Empty;
        public string initialText { get; set; } = string.Empty;
        public List<NodaViewNodeProps> nodes { get; set; } = new List<NodaViewNodeProps>();
        public List<NodaViewLinkProps> links { get; set; } = new List<NodaViewLinkProps>();

        private Dictionary<string, NodaViewNodeProps>? nodeLookup { get; set; }

        private Dictionary<string, Dictionary<string, NodaViewLinkProps>> linkLookup { get; set; } = new Dictionary<string, Dictionary<string, NodaViewLinkProps>>();


        public ILayoutLink? GetEdge(string fromNode, string toNode)
        {
            if (linkLookup.ContainsKey(fromNode))
            {
                if (linkLookup[fromNode].ContainsKey(toNode))
                    return linkLookup[fromNode][toNode];
            }
            return null;
        }

        public List<ILayoutLink> GetLinks()
        {
            return (List<ILayoutLink>)links.Select(a => a as ILayoutLink);
        }

        public ILayoutNode? GetNode(string uuid)
        {
            if (nodeLookup != null && nodeLookup.ContainsKey(uuid))
                return nodeLookup[uuid];
            return null;
        }

        public List<ILayoutNode> GetNodes()
        {
            return (List<ILayoutNode>)nodes.Select(a => a as ILayoutNode);
        }

        public void Init()
        {
            nodeLookup = nodes.ToDictionary(a => a.uuid);
            foreach (var l in links)
            {
                if (!linkLookup.ContainsKey(l.fromUuid))
                {
                    linkLookup.Add(l.fromUuid, new Dictionary<string, NodaViewLinkProps>());
                }
                if (!linkLookup[l.fromUuid].ContainsKey(l.toUuid))
                {
                    linkLookup[l.fromUuid].Add(l.toUuid, l);
                }
            }        
        }
    }
}
