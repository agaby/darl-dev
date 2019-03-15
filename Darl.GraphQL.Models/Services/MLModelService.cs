using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCommon;

namespace Darl.GraphQL.Models.Services
{
    public class MLModelService : IMLModelService
    {
        public async Task<MLModel> GetMlModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<MLModel>> GetMlModelsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
