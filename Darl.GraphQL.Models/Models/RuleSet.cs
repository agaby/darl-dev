using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class RuleSet
    {
        public RuleSet(string name, RuleForm contents = null)
        {
            Name = name;
            Contents = contents;
        }

        public string Name { get; }
        public RuleForm Contents { get; }
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; }


    }
}
