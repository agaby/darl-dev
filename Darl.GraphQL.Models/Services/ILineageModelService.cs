using Darl.Lineage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface ILineageModelService
    {
        Task<LineageModel> GetLineageModelAsync(string name);
    }
}
