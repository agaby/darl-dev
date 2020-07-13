using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using Darl_standard.Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Darl.GraphQL.Models.Connectivity
{
    [ProtoContract]
    public class BlobGraphContent : GraphModel
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

    }
}
