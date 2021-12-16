using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Models
{
    public  class WebPushPayload
    {
        public string title { get; set; } = string.Empty;

        public WebPushOptions? options { get; set; }
    }
}
