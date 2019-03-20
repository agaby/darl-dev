using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IRuleFormService
    {
        Task<RuleForm> GetRuleFormAsync(string name);
        Task<List<RuleForm>> GetRuleFormsAsync(string name);
    }
}
