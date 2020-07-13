using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GraphLocalStore : ILocalStore
    {
        private IConfiguration config;
        private ILogger<GraphLocalStore> logger;
        private IHttpContextAccessor context;
        private IGraphProcessing graph;

        public GraphLocalStore(IConfiguration config, ILogger<GraphLocalStore> logger, IHttpContextAccessor context, IGraphProcessing graph)
        {
            this.config = config;
            this.logger = logger;
            this.context = context;
            this.graph = graph;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<DarlResult> ReadAsync(List<string> address)
        {
            var userId = context.HttpContext.User.Identity.Name;
            var emptyResult = new DarlResult("result", "", DarlResult.DataType.textual);
            emptyResult.SetWeight(0.0);
            try
            {
                if (address.Count > 1)
                {
                    switch (address[0].ToLower())
                    {
                        case "path":
                            if (address.Count != 4)
                            {
                                throw new ExecutionError("Path call to a graph store must have 4 parameters, 'path', graph name, start externalId and end externalId");
                            }
                            var path = await graph.ProcessPath(CreateCompositeName(userId, address[1].Trim()), address[2].Trim(), address[3].Trim());
                            if (path == null || path.Count == 0 || path.Count > 15)//reject long paths...Warning, arbitrary
                            {
                                return new DarlResult("result", "There is no path found", DarlResult.DataType.textual);
                            }
                            var res = String.Join(" -> ", path.Select(a => a.name));
                            return new DarlResult("result", res, DarlResult.DataType.textual);
                        case "attribute":
                            if (address.Count != 4)
                            {
                                throw new ExecutionError("Attribute call to a graph store must have 4 parameters, 'path', graph name, externalId and property name");
                            }
                            var att = await graph.ProcessAttribute(CreateCompositeName(userId, address[1].Trim()), address[2].Trim(), address[3].Trim());
                            if (string.IsNullOrEmpty(att))
                            {
                                return emptyResult;
                            }
                            return new DarlResult("result", att, DarlResult.DataType.textual);
                        case "categories":
                            if (address.Count != 5)
                            {
                                throw new Exception("Categories call to a graph store must have 4 parameters, 'categories', graph name, the root externalId, the children lineage and the attribute value name/lineage");
                            }
                            var results = await graph.ProcessCategories(CreateCompositeName(userId, address[1].Trim()), address[2].Trim(), address[3].Trim(), address[4].Trim());
                            var result = new DarlResult("result", DarlResult.DataType.categorical, 1.0);
                            foreach (var c in results)
                            {
                                var cat = $"{c.Name}%%{c.Value}%%";
                                if (!result.categories.ContainsKey(cat))
                                    result.categories.Add(cat, 1.0);
                            }
                            return result;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Error in ReadAsync, parameters {string.Join(',', address)}");
                return emptyResult;
            }
            return emptyResult;
        }

        public Task WriteAsync(List<string> address, DarlResult value)
        {
            throw new NotImplementedException();
        }

        public static string CreateCompositeName(string userId, string filename)
        {
            return $"{userId}_{filename}";
        }
    }
}
