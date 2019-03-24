using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Models.Services
{
    public class MLModelService : IMLModelService
    {

        IConnectivity Connectivity;


        public MLModelService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }


        public Task<MLModel> GetMLModel(string name)
        {
            return Task.FromResult(Connectivity.GetMlModel(name));
        }


        public async Task<List<MLModel>> GetMLModelsAsync()
        {
            return await Connectivity.GetMlModelsAsync();
        }
    }
}
