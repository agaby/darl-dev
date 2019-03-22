using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Darl.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Darl.GraphQL.Models.Connectivity
{
    class AzureStorageConnectivity : IConnectivity
    {

        IOptions<AppSettings> _opt;

        private CloudBlobContainer botBlob;

        private CloudBlobContainer ruleBlob;

        private CloudBlobContainer mlmodelsBlob;

        private string userId;


        public AzureStorageConnectivity(IOptions<AppSettings> optionsAccessor)
        {
            _opt = optionsAccessor;
            var storageAccount = CloudStorageAccount.Parse(_opt.Value.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            botBlob = blobClient.GetContainerReference(_opt.Value.modelcontainer);
            ruleBlob = blobClient.GetContainerReference(_opt.Value.rulecontainer);
            mlmodelsBlob = blobClient.GetContainerReference(_opt.Value.mlmodelcontainer);
            userId = _opt.Value.boaiuserid;
        }

        public async Task<BotModel> GetBotModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BotModel>> GetBotModelsAsync()
        {
            var list = new List<BotModel>();
            var folder = botBlob.GetDirectoryReference(userId);
            foreach (var b in await folder.ListBlobsAsync())
            {
                if (b is CloudBlockBlob)
                {
                    var c = b as CloudBlockBlob;
                    var name = b.Uri.Segments.Last().Replace("%20", " ");
                    list.Add(new BotModel((c.Properties.Created ?? DateTime.MinValue).DateTime, c.Name, (int)c.Properties.Length));
                }
            }
            return list;
        }

        public async Task<LineageModel> GetLineageModelAsync(string name)
        {
            try
            {
                LineageModel lib = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    var bbref = botBlob.GetBlockBlobReference($"{userId}/{ name}");
                    await bbref.DownloadToStreamAsync(ms);
                    ms.Position = 0;
                    lib = LineageModel.Load(ms);
                    lib.AddDescriptions();
                }
                return lib;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DarlCommon.MLModel> GetMlInternalModelAsync(string name)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var re = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}"));
                    await re.DownloadToStreamAsync(ms);
                    ms.Position = 0;
                    StreamReader r = new StreamReader(ms);
                    var mlm = JsonConvert.DeserializeObject<DarlCommon.MLModel>(await r.ReadToEndAsync());
                    return mlm;
                }
            }
            catch
            {
                return null;
            }
        }


        public async Task<RuleForm> GetRuleFormAsync(string name)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var re = ruleBlob.GetBlockBlobReference(WebUtility.HtmlEncode(string.IsNullOrEmpty(userId) ? name : $"{userId}/{name}"));
                    await re.DownloadToStreamAsync(ms);
                    ms.Position = 0;
                    StreamReader r = new StreamReader(ms);
                    var rf = JsonConvert.DeserializeObject<RuleForm>(await r.ReadToEndAsync());
                    return rf;
                }
            }
            catch
            {
                return null;
            }
        }


        public async Task<RuleSet> GetRuleSetAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<RuleSet>> GetRuleSetsAsync()
        {
            var list = new List<Models.RuleSet>();
            var folder = ruleBlob.GetDirectoryReference(userId);
            foreach (var b in await folder.ListBlobsAsync())
            {
                if (b is CloudBlockBlob)
                {
                    var c = b as CloudBlockBlob;
                    var name = b.Uri.Segments.Last().Replace("%20", " ");
                    list.Add(new RuleSet((c.Properties.Created ?? DateTime.MinValue).DateTime, c.Name, (int)c.Properties.Length));
                }
            }
            return list;
        }

        public async Task<Models.MLModel> GetMlModelAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Models.MLModel>> GetMlModelsAsync()
        {
            var list = new List<Models.MLModel>();
            var folder = mlmodelsBlob.GetDirectoryReference(userId);
            foreach (var b in await folder.ListBlobsAsync())
            {
                if(b is CloudBlockBlob)
                { 
                    var c =  b as CloudBlockBlob;
                    var name = b.Uri.Segments.Last().Replace("%20", " ");
                    list.Add(new Models.MLModel((c.Properties.Created ?? DateTime.MinValue).DateTime, c.Name, (int)c.Properties.Length));
                }
            }
            return list;
        }
    }
}
