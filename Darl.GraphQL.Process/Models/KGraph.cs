using DarlCommon;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class KGraph
    {
        public ObjectId id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Authorization> Authorizations { get; set; } = new List<Authorization>();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
        public List<UserUsage> UsageHistory { get; set; } = new List<UserUsage>();

        public bool Shared { get; set; } = false;

        public string OwnerId { get; set; }

        public bool? ReadOnly { get; set; } = false;
    }
}
