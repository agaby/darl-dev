using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BlobConnectivity : IBlobConnectivity
    {
        private IConfiguration _config;
        private ILogger _logger;
        private CloudBlobClient _blob;
        private CloudBlobContainer _container;

        public BlobConnectivity(IConfiguration config, ILogger<BlobConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            var csa = CloudStorageAccount.Parse(_config["AppSettings:StorageConnectionString"]);
            _blob = csa.CreateCloudBlobClient();
            _container = _blob.GetContainerReference(_config["AppSettings:BlobContainer"]);
        }

        public async Task<byte[]> Read(string name)
        {
            var b = _container.GetBlockBlobReference(name);
            await b.FetchAttributesAsync();
            byte[] target = new byte[b.Properties.Length];
            await b.DownloadToByteArrayAsync(target, 0);
            return target;
        }

        public async Task Write(string name, byte[] data)
        {
            var b = _container.GetBlockBlobReference(name);
            await b.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        public async Task<bool> Exists(string name)
        {
            var b = _container.GetBlockBlobReference(name);
            return await b.ExistsAsync();
        }
    }
}
