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


        public async Task<MLModel> GetMLModelAsync(string name)
        {
            return await Connectivity.GetMlModelAsync(name);
        }


        public async Task<List<MLModel>> GetMLModelsAsync()
        {
            return await Connectivity.GetMlModelsAsync();
        }
    }
}
