using Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class StringListGraphAttributePair
    {
        public string Name { get; set; }

        public List<GraphAttribute> Value { get; set; }
    }
}
