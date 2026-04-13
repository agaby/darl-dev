/// <summary>
/// </summary>

﻿using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BlobGraphConnectivity : IBlobConnectivity
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly BlobContainerClient _container;

        public string implementation => nameof(BlobGraphConnectivity);

        public BlobGraphConnectivity(IConfiguration config, ILogger<BlobGraphConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            _container = new BlobContainerClient(_config["AppSettings:StorageConnectionString"], _config["AppSettings:GraphContainer"]);
            _container.CreateIfNotExists();
        }

        public async Task<byte[]> Read(string name)
        {
            var b = _container.GetBlobClient(name);
            using (var m = new MemoryStream())
            {
                await b.DownloadToAsync(m);
                return m.ToArray();
            }
        }

        public async Task Write(string name, byte[] data)
        {
            try
            {
                var b = _container.GetBlobClient(name);
                using (var m = new MemoryStream(data))
                {
                    await b.UploadAsync(m, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure to write blob '{name}', length {data.Length}");
            }
        }

        public async Task<bool> Exists(string name)
        {
            try
            {
                var b = _container.GetBlobClient(name);
                return await b.ExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string name)
        {
            var b = _container.GetBlobClient(name);
            return await b.DeleteIfExistsAsync();
        }

        public List<string> List(string prefix)
        {
            var listseg = _container.GetBlobs(prefix: prefix);
            return listseg.Select(a => a.Name.ToString()).ToList();
        }

        public string CreateTimedAccessUrl(string name)
        {
            var blob = _container.GetBlobClient(name);
            return blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.UtcNow.AddHours(24)).ToString();
        }
    }
}
