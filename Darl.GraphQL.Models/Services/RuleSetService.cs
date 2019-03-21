using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    public class RuleSetService : IRuleSetService
    {
        IConnectivity Connectivity;

        public RuleSetService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }
        public Task<RuleSet> GetRuleSetAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<RuleSet>> GetRuleSetsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
