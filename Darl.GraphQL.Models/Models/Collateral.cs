using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class Collateral
    {
        public Collateral(DateTime lastModified, string name, int size, string content)
        {
            LastModified = lastModified;
            Name = name;
            Size = size;
            Content = content;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public int Size { get; }
        public string Content { get; }
        public string userId { get; set; }
    }
}
