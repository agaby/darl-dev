using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IMLModelService
    {
        Task<DarlCommon.MLModel> GetMlModelAsync(string name);
        Task<List<DarlCommon.MLModel>> GetMlModelsAsync();
    }
}
