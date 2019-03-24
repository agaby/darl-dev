using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IRuleSetService
    {
        Task<RuleSet> GetRuleSet(string name);
        Task<List<RuleSet>> GetRuleSetsAsync();
    }
}
