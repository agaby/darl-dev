using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCommon;
using Newtonsoft.Json;

namespace Darl.GraphQL.Models.Services
{
    public class BotFormatService : IBotFormatService
    {
        public async Task<BotFormat> GetConvertedBotFormat(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<BotFormat>(source);
            }
            catch
            {
                return null; 
            }
        }
    }
}
