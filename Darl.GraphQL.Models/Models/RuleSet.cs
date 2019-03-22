using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class RuleSet
    {
        public RuleSet(DateTime lastModified, string name, int size, RuleForm contents = null)
        {
            LastModified = lastModified;
            Name = name;
            Size = size;
            Contents = contents;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public int Size { get; }
        public RuleForm Contents { get; }
    }
}
