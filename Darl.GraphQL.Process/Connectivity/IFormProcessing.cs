using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.GraphQL.Process.Models.Alexa;
using DarlCommon;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IFormProcessing
    {
        Task<QuestionSetProxy> BacktrackQuestionnaire(string ieToken);

        Task<QuestionSetProxy> BeginQuestionnaire(string userId, string ruleSetName, string language = "en", int questCount = 1);

        Task<QuestionSetProxy> ContinueQuestionnaire(QuestionSetInput responses);

        Task<QuestionSetProxy> BeginDynamicQuestionnaire(string userId, string selector, DQType dqType);
        Task<InteractionModel> GetAlexaInteractionModel(string userId, string name, string invocationName);
    }
}
