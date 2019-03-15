using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Darl.Lineage;

namespace Darl.GraphQL.Models.Services
{
    class LineageModelService : ILineageModelService
    {
        public Task<LineageModel> GetLineageModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageModel>> GetLineageModelsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
