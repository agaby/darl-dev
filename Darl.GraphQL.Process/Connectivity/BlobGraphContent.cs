using Darl.Thinkbase;
using System.Collections.Generic;
using ProtoBuf;

namespace Darl.GraphQL.Models.Connectivity
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
        public Dictionary<string,GraphConnection> virtualEdges { get; set; } = new Dictionary<string, GraphConnection>();
        public KnowledgeState state { get; set; }
        public string modelName { get ; set ; }

        public List<GraphObject> GetConnectedObjects(GraphObject node, string connectionLineage, string objectLineage)
        {
            var list = new List<GraphObject>();
            foreach(var i in node.Out)
            {
                if(i.lineage.StartsWith(connectionLineage))
                {
                    var obj = vertices[i.endId];
                    if (obj.lineage.StartsWith(objectLineage))
                        list.Add(obj);
                }
            }
            return list;
        }
    }
}
