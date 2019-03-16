using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Services
{
    public interface ILanguageTextService
    {
        Task<List<ILanguageTextService>> GetLanguageTextsAsync();
    }
}
