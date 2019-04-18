using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotModel
    {

        public string Name { get; set; }
        public LineageModel Model { get; set; } = new LineageModel();
        public List<BotConnection> botconnections { get; set; } = new List<BotConnection>();
        public List<string> Authorizations { get; set; } = new List<string>();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
    }
}
