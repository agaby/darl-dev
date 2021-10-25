using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    public class WordStore : ILocalStore
    {
        
        public Task<DarlResult> ReadAsync(List<string> address)
        {
            int index = 0;
            var definitions = LineageLibrary.WordRecognizer(LineageLibrary.SimpleTokenizer(address[0]), ref index);
            if (definitions != null && !(definitions.Count == 1 && string.IsNullOrEmpty(definitions[0].description))) //if valid response
            {
                var sb = new StringBuilder();
                foreach (var d in definitions)
                {
                    sb.AppendLine(d.description);
                }
                return Task.FromResult<DarlResult>(new DarlResult("", sb.ToString(), DarlResult.DataType.textual));
            }
            return Task.FromResult<DarlResult>(new DarlResult(0.0, true));
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            return Task.CompletedTask;
        }
    }
}
