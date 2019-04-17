using Darl.Connectivity.Models;
using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotModel
    {
        public BotModel(string name, LineageModel model = null)
        {
            Name = name;
            Model = model;
        }

        public string Name { get; }
        public LineageModel Model { get; }
        public List<ConnectivityView> botconnections { get; }
        public List<string> Authorizations { get; set; }
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; }
    }
}
