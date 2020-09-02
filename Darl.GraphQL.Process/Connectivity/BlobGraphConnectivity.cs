using Darl.GraphQL.Models.Connectivity;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BlobGraphConnectivity : IBlobConnectivity
    {
        private IConfiguration _config;
        private ILogger _logger;
        private CloudBlobClient _blob;
        private CloudBlobContainer _container;

        public string implementation => nameof(BlobGraphConnectivity);

        public BlobGraphConnectivity(IConfiguration config, ILogger<BlobConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            var csa = CloudStorageAccount.Parse(_config["AppSettings:StorageConnectionString"]);
            _blob = csa.CreateCloudBlobClient();
            _container = _blob.GetContainerReference(_config["AppSettings:GraphContainer"]);
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
            try
            {
                var b = _container.GetBlockBlobReference(name);
                await b.UploadFromByteArrayAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure to write blob '{name}', length {data.Length}");
            }
        }

        public async Task<bool> Exists(string name)
        {
            var b = _container.GetBlockBlobReference(name);
            return await b.ExistsAsync();
        }

        public async Task<bool> Delete(string name)
        {
            var b = _container.GetBlockBlobReference(name);
            return await b.DeleteIfExistsAsync();
        }

        public List<string> List(string prefix)
        {
            var list = _container.ListBlobs(prefix).Select(a => a.Uri.ToString()).ToList();
            //response is full url, remove all but the model name.
            var abbreviatedList = new List<string>();
            foreach (var l in list)
            {
                int loc = l.LastIndexOf(prefix);
                abbreviatedList.Add(l.Substring(loc + prefix.Length + 1));
            }
            return abbreviatedList;
        }
    }
}
