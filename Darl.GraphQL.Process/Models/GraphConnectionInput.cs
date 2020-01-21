using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphConnectionInput  : GraphElementInput
    {
        public double weight { get; set; } = 1.0;
        public string startId { get; set; }
        public string endId { get; set; }
    }
}
