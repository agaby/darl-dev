using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class Document
    {
        public Document(DateTime lastModified, string name, int size)
        {
            LastModified = lastModified;
            Name = name;
            Size = size;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public int Size { get; }
    }
}
