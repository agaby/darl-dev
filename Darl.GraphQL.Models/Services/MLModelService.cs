using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using DarlCommon;

namespace Darl.GraphQL.Models.Services
{
    public class MLModelService : IMLModelService
    {
        IConnectivity Connectivity;

        public MLModelService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }
        public async Task<MLModel> GetMlModelAsync(string name)
        {
            return await Connectivity.GetMlModelAsync(name);
        }

        public async Task<List<MLModel>> GetMlModelsAsync()
        {
            return await Connectivity.GetMlModelsAsync(); 
        }
    }
}
