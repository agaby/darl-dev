using System.Threading.Tasks;
using DarlCommon;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IFormApi
    {
        Task<QuestionSetProxy> Delete(string id);
        Task<QuestionSetProxy> Get(string id, int questCount = 1);
        Task<QuestionSetProxy> Post(QuestionSetProxy questionsetproxy);
        Task<bool> Trigger(string id);
    }
}