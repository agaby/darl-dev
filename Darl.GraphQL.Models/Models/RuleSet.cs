using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class RuleSet
    {
        public string Name { get; set; }
        public RuleForm Contents { get; set; } = new RuleForm();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
    }
}
