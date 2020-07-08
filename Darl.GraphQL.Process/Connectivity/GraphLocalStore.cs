using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GraphLocalStore : ILocalStore
    {
        private IConfiguration config;
        private ILogger<GraphLocalStore> logger;
        private IHttpContextAccessor context;

        public GraphLocalStore(IConfiguration config, ILogger<GraphLocalStore> logger, IHttpContextAccessor context)
        {
            this.config = config;
            this.logger = logger;
            this.context = context;
        }

        public Task<DarlResult> ReadAsync(List<string> address)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            throw new NotImplementedException();
        }
    }
}
