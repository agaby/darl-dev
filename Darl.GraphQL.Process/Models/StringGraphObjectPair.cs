using Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class StringGraphObjectPair
    {
        public string Name { get; set; }

        public GraphObject Value { get; set; }
    }
}
