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

        public async Task<BotModel> GetBotModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BotModel>> GetBotModelsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DarlCommon.MLModel> GetMlModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DarlCommon.MLModel>> GetMlModelsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<RuleForm> GetRuleFormAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Models.MLModel>> GetRuleFormsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
