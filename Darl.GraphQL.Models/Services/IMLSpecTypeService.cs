using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IMLSpecTypeService
    {
        Task<DarlCommon.MLModel> GetMlModelAsync(string name);
    }
}
