using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MLModel
    {
        public ObjectId id { get; set; }
        public string Name { get; set; }
        public DarlCommon.MLModel model { get; set; }
        public string userId { get; set; }
        public List<MLResult> results { get; set; } = new List<MLResult>();

    }
}
