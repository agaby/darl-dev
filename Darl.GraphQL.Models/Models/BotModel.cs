using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotModel
    {
        public BotModel(DateTime lastModified, string name, int size, LineageModel model)
        {
            LastModified = lastModified;
            Name = name;
            Model = model;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public LineageModel Model { get; }
    }
}
