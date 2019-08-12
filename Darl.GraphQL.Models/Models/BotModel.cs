using Darl.Lineage;
using DarlCommon;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotModel
    {
        public ObjectId id { get; set; }

        public string Name { get; set; }
        public byte[] Model { get; set; }
        /// <summary>
        /// This ought to be a list of references
        /// </summary>
        public List<ObjectId> botconnections { get; set; } = new List<ObjectId>();
        public List<Authorization> Authorizations { get; set; } = new List<Authorization>();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
        public List<UserUsage> UsageHistory { get; set; } = new List<UserUsage>();

    }
}
