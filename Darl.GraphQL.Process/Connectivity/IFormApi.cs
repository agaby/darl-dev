/// </summary>

﻿using Darl.GraphQL.Models.Models;
using DarlCommon;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IFormApi
    {
        Task<QuestionSetProxy> Delete(string id);
        Task<QuestionSetProxy> Get(RuleSet ruleSet, string language = "en", int questCount = 1);
        Task<QuestionSetProxy> Post(QuestionSetProxy questionsetproxy);
        Task<QuestionSetProxy> CreateDynamicRuleSetEditor(RuleSet ruleset, RuleSet template);
    }
}