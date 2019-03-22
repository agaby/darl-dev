using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using DarlCommon;

namespace Darl.GraphQL.Models.Services
{
    public class MLSpecTypeService : IMLSpecTypeService
    {
        IConnectivity Connectivity;

        public MLSpecTypeService(IConnectivity connectivity)
        {
            Connectivity = connectivity;
        }

        public async Task<DarlCommon.MLModel> GetMlModelAsync(string name)
        {
            return await Connectivity.GetMlInternalModelAsync(name);
        }


    }
}
