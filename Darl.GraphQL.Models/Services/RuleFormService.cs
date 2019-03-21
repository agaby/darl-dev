using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using DarlCommon;

namespace Darl.GraphQL.Models.Services
{
    public class RuleFormService : IRuleFormService
    {

        IConnectivity Connectivity;

        public RuleFormService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }

        public async Task<RuleForm> GetRuleFormAsync(string name)
        {
            return await Connectivity.GetRuleFormAsync(name);
        }

        public async Task<List<RuleForm>> GetRuleFormsAsync(string name)
        {
            return await Connectivity.GetRuleFormsAsync();
        }
    }
}
