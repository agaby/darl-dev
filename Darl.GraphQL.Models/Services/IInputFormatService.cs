using DarlCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface IInputFormatService
    {
        Task<List<InputFormat>> GetInputFormatsAsync();
    }
}
