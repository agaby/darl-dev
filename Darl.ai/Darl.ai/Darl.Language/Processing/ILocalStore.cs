using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public interface ILocalStore
    {
        Task<DarlResult> ReadAsync(List<string> address);

        Task WriteAsync(List<string> address, DarlResult value);

    }
}
