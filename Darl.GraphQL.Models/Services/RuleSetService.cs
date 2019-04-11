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

        public async Task<RuleSet> CreateEmptyRuleSet(string name)
        {
            return await Connectivity.CreateEmptyRuleSet(name);
        }

        public async Task<RuleSet> DeleteRuleSet(string name)
        {
            var res = Connectivity.GetRuleSet(name);
            if (res != null)
                await Connectivity.DeleteRuleSet(name);
            return res;
        }

        public Task<RuleSet> GetRuleSet(string name)
        {
            return Task.FromResult(Connectivity.GetRuleSet(name)); ;
        }

        public async Task<List<RuleSet>> GetRuleSetsAsync()
        {
            return await Connectivity.GetRuleSetsAsync();
        }

    }
}
