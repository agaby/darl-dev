using Darl.Thinkbase.Meta;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Thinkbase
{
    /// <summary>
    /// A knowledgeState constrained to contain data for only one parent Object
    /// </summary>
    public class KnowledgeRecord : KnowledgeState
    {
        /// <summary>
        /// Find the parent node and connections
        /// </summary>
        /// <param name="model"></param>
        /// <param name="lineages"></param>
        /// <returns></returns>
        public (GraphObject?, List<GraphConnection>) DeReference(IGraphModel model, List<string>? lineages)
        {
            GraphObject? currentNode = null;
            var connections = new List<GraphConnection>();
            if (data != null && data.Any())
            {
                foreach (var key in data.Keys)
                {
                    if (model.vertices.ContainsKey(key))
                    {
                        if (currentNode != null)
                        {
                            throw new MetaRuleException($"Multiple nodes found.");
                        }
                        currentNode = model.vertices[key];
                    }
                    else if (model.edges.ContainsKey(key))
                    {
                        var c = model.edges[key];
                        if (CheckPermittedLineages(c.lineage, lineages))
                        {
                            connections.Add(c);
                        }
                    }
                    else
                    {
                        throw new MetaRuleException($"linked item not found: {key}");
                    }
                }
                //now add any non-infered parent connections
                if(currentNode != null)
                {
                    foreach(var c in currentNode.Out)
                    {
                        if(!c.inferred)
                        {
                            if (CheckPermittedLineages(c.lineage, lineages))
                            {
                                connections.Add(c);
                            }
                        }
                    }
                }
            }
            return (currentNode, connections);
        }

        private bool CheckPermittedLineages(string? lineage, List<string>? lineages)
        {
            if (lineages == null || !lineages.Any() || lineage == null)
                return true;
            foreach (var l in lineages)
            {
                if (lineage.StartsWith(l))
                    return true;
            }
            return false;
        }

    }
}
