using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    //Definition returned from GetChildren search in tree editing
    public class LineageNodeDefinition
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool children { get; set; }
        public LineageNodeAttributes attributes { get; set; }

    }
}
