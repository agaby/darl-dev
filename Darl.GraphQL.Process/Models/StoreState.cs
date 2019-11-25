using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class StoreState
    {
        public string name { get; set; }
        public Dictionary<string, string> states { get; set; } = new Dictionary<string, string>();
    }
}
