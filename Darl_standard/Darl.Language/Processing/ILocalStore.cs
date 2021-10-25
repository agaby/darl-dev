using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public interface ILocalStore
    {
        Task<DarlResult> ReadAsync(List<string> address);

        Task WriteAsync(List<string> address, DarlResult value);

    }
}
