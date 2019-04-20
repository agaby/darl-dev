using Darl.Lineage;
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
        public LineageModel Model { get; set; } = new LineageModel();
        public List<BotConnection> botconnections { get; set; } = new List<BotConnection>();
        public List<Authorization> Authorizations { get; set; } = new List<Authorization>();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
    }
}
