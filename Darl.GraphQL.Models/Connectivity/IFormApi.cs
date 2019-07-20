using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using DarlCommon;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IFormApi
    {
        Task<QuestionSetProxy> Delete(string id);
        Task<QuestionSetProxy> Get(RuleSet ruleSet, string language = "en", int questCount = 1);
        Task<QuestionSetProxy> Post(QuestionSetProxy questionsetproxy);
        Task<bool> Trigger(string id);
        Task<QuestionSetProxy> CreateDynamicRuleSetEditor(RuleSet ruleset, RuleSet template);
    }
}