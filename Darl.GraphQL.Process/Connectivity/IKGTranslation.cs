using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IKGTranslation
    {
        Task<DateTime> GetLastUpdate(string from, string to);
        Task<DateTime> SetLastUpdate(string from, string to);
        Task<List<Update>> Updates();
    }
}
