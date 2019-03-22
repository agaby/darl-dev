using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotModel
    {
        public BotModel(DateTime lastModified, string name, int size, LineageModel model = null)
        {
            LastModified = lastModified;
            Name = name;
            Size = size;
            Model = model;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public int Size { get; }
        public LineageModel Model { get; }
    }
}
