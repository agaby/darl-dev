using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Models
{
    public  class PushSub
    {
        public string ipAddress { get; set; } = string.Empty;
        public string pushAuth { get; set; } = string.Empty;
        public string pushEndPoint { get; set; } = string.Empty;
        public string pushKey { get; set; } = string.Empty;
    }
}
