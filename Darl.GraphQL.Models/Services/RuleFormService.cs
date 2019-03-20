using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCommon;

namespace Darl.GraphQL.Models.Services
{
    public class RuleFormService : IRuleFormService
    {
        public Task<RuleForm> GetRuleFormAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<RuleForm>> GetRuleFormsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
