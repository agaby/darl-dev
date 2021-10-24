using DarlCommon;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    /// <summary>
    /// Contains the view relevant to a remote bot framework
    /// </summary>
    public class BotRuntimeModel
    {
        public byte[] Model { get; set; }
        public List<Authorization> Authorizations { get; set; } = new List<Authorization>();
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
        public string password { get; set; }

        public string userId { get; set; }

        public string botModelName { get; set; }
    }
}
