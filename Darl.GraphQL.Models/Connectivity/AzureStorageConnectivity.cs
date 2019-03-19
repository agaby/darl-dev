using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using DarlCommon;

namespace Darl.GraphQL.Models.Connectivity
{
    class AzureStorageConnectivity : IConnectivity
    {
        public AzureStorageConnectivity()
        {
                
        }

        public Task<BotModel> GetBotModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<BotModel>> GetMlBotModelsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<DarlCommon.MLModel> GetMlModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<DarlCommon.MLModel>> GetMlModelsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<RuleForm> GetRuleFormAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.MLModel>> GetRuleFormsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
