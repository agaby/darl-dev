using Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class StringGraphConnectionPair
    {
        public string Name { get; set; }

        public GraphConnection Value { get; set; }
    }
}
