using Darl.GraphQL.Models.Connectivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class OneFileConnectivity : IBlobConnectivity
    {
        public string implementation => nameof(OneFileConnectivity);
        private IConfiguration _config;
        private ILogger _logger;
        private string filepath;

        public OneFileConnectivity(IConfiguration config, ILogger<OneFileConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            filepath = _config["BLOBFILEPATH"];
            var backgroundUserId = _config["SINGLEUSERID"];
        }

        public Task<bool> Delete(string name)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Exists(string name)
        {
            return Task.FromResult(File.Exists(filepath));
        }

        public List<string> List(string prefix)
        {
            return new List<string> {filepath};
        }

        public async Task<byte[]> Read(string name)
        {
            return await File.ReadAllBytesAsync(filepath);
        }

        public async Task Write(string name, byte[] data)
        {
            await File.WriteAllBytesAsync(filepath, data);
        }

        public string CreateTimedAccessUrl(string name)
        {
            throw new NotImplementedException();
        }
    }
}
