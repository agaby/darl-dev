using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class RuleSet
    {
        public RuleSet(DateTime lastModified, string name, RuleForm contents)
        {
            LastModified = lastModified;
            Name = name;
            Contents = contents;
        }

        public DateTime LastModified { get; }
        public string Name { get; }
        public RuleForm Contents { get; }
    }
}
