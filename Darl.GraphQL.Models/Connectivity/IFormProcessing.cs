using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IFormProcessing
    {
        Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken);

        Task<QuestionSetProxy> BeginQuestionnaire(string userId, string ruleSetName, string language = "en", int questCount = 1);

        Task<QuestionSetProxy> ContinueQuestionnaire(QuestionSetInput responses);

        Task<object> BeginDynamicQuestionnaire(string userId, string selector, DQType dqType);

    }
}
