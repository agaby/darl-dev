using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class OneFileConnectivity : IBlobConnectivity
    {
        public string implementation => nameof(OneFileConnectivity);
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string filepath;
        private string fileName { get; set; } = string.Empty;

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
            return new List<string> { fileName };
        }

        public async Task<byte[]> Read(string name)
        {
            return await File.ReadAllBytesAsync(filepath);
        }

        public async Task Write(string name, byte[] data)
        {
            fileName = name;
            await File.WriteAllBytesAsync(filepath, data);
        }

        public string CreateTimedAccessUrl(string name)
        {
            throw new NotImplementedException();
        }
    }
}
