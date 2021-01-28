using DarlCommon;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class KGraphUpdate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string userId { get; set; }
        public bool? ReadOnly { get; set; } = false;
    }
}
