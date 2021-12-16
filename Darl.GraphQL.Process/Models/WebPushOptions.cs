using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Models
{
    public class WebPushOptions
    {
        public List<WebPushAction> actions { get; set; } = new List<WebPushAction> { };
        public string? badge { get; set; }

        public string? body { get; set; }

        public object? data { get; set; } = null;

        public string? dir { get; set; }
        
        public string? icon { get; set; }
        public string? image { get; set; }

        public bool renotify { get; set; } = false;
        public bool requireInteraction { get; set; } = false;
        public bool silent { get; set; } = false;

        public string? tag { get; set; }
        
        public int? timeStamp { get; set; }

        public List<int>? vibrate { get; set; }

    }
}
