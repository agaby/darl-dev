/// </summary>

﻿using DarlLanguage.Processing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot.Stores
{
    public class RestStore : ILocalStore
    {
        /// Look up data on the internet with json responses
        /// </summary>
        /// <param name="address">A sequence of parameters.</param>
        /// <returns>The data</returns>
        /// <remarks>1st parameter is url. 2nd is jsonpath to response. Further params are inserted in the url in order</remarks>
        /// <example>""</example>
        public async Task<DarlResult> ReadAsync(List<string> address)
        {
            if (address.Count >= 2)
            {
                try
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(string.Format(address[0], address.Skip(2)));
                    if (response.Content != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            var o = JObject.Parse(responseString);
                            var r = o.SelectToken(address[1]);
                            if (r != null)
                            {
                                return new DarlResult("", r.ToString(), DarlResult.DataType.textual);
                            }
                            return new DarlResult("", "I tried to look that up but the response was wrong.", DarlResult.DataType.textual);
                        }
                    }
                }
                catch
                {
                    return new DarlResult("", "I tried to look that up but the remote site didn't work.", DarlResult.DataType.textual);
                }
            }
            return new DarlResult("", "Someone sent the wrong number of parameters in the request. Contact help.", DarlResult.DataType.textual);
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            throw new NotImplementedException();
        }
    }
}