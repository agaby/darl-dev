using DarlCommon;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public enum ModelType { ruleset, botmodel, mlmodel, simmodel };
    public class RuleSet
    {
        public ObjectId id { get; set; }

        public string Name { get; set; }
        public RuleForm Contents { get; set; } = new RuleForm();
        public string userId { get; set; }
        public ServiceConnectivity serviceConnectivity { get; set; } = new ServiceConnectivity();
        public List<UserUsage> UsageHistory { get; set; } = new List<UserUsage>();

    }
}
