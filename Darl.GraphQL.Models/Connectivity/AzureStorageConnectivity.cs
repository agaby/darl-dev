using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Darl.Connectivity;
using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Darl.GraphQL.Models.Connectivity
{
    public class AzureStorageConnectivity : IConnectivity
    {

        IOptions<AppSettings> _opt;

        private CloudBlobContainer botBlob;

        private CloudBlobContainer ruleBlob;

        private CloudBlobContainer mlmodelsBlob;

        private CloudBlobContainer collateralBlob;

        private CloudBlobContainer mailcontentBlob;

        private CloudBlobContainer newscontentBlob;

        private CloudBlobContainer docsBlob;

        private CloudTable connections { get; set; }

        private CloudTable credentials { get; set; }

        private CloudTable botusages { get; set; }

        private CloudTable contacts { get; set; }

        private CloudTable defaults { get; set; }

        private CloudTable authorizations { get; set; }



        public string userId { get; set; }


        public AzureStorageConnectivity(IOptions<AppSettings> optionsAccessor)
        {
            _opt = optionsAccessor;
            var storageAccount = CloudStorageAccount.Parse(_opt.Value.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            botBlob = blobClient.GetContainerReference(_opt.Value.modelcontainer);
            ruleBlob = blobClient.GetContainerReference(_opt.Value.rulecontainer);
            mlmodelsBlob = blobClient.GetContainerReference(_opt.Value.mlmodelcontainer);
            docsBlob = blobClient.GetContainerReference(_opt.Value.docscontainer);
            newscontentBlob = blobClient.GetContainerReference(_opt.Value.newscontentcontainer);
            mailcontentBlob = blobClient.GetContainerReference(_opt.Value.mailcontentcontainer);
            collateralBlob = blobClient.GetContainerReference(_opt.Value.collateralcontainer);
            var client = storageAccount.CreateCloudTableClient();
            connections = client.GetTableReference(nameof(connections));
            credentials = client.GetTableReference(nameof(credentials));
            botusages = client.GetTableReference(nameof(botusages));
            contacts = client.GetTableReference(nameof(contacts));
            defaults = client.GetTableReference(nameof(defaults));
            authorizations = client.GetTableReference(nameof(authorizations));

            userId = _opt.Value.boaiuserid;
        }

        public BotModel GetBotModel(string name)
        {
            try
            {
                var b = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}.model"));
                if (b is CloudBlockBlob)
                {
                    var c = b as CloudBlockBlob;
                    return new BotModel((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length);
                }
            }
            catch
            {
                return null;
            }
            return null;
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
                    list.Add(new BotModel((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length));
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
                    var bbref = botBlob.GetBlockBlobReference($"{userId}/{name}.model");
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
                    var re = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}.mlmodel"));
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
                    var re = ruleBlob.GetBlockBlobReference(WebUtility.HtmlEncode(string.IsNullOrEmpty(userId) ? name : $"{userId}/{name}.rule"));
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


        public RuleSet GetRuleSet(string name)
        {
            try
            {
                var b = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}.model"));
                if (b is CloudBlockBlob)
                {
                    var c = b as CloudBlockBlob;
                    return new RuleSet((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length);
                }
            }
            catch
            {
                return null;
            }
            return null;
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
                    list.Add(new RuleSet((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length));
                }
            }
            return list;
        }

        public Models.MLModel GetMlModel(string name)
        {
            try
            {
                var b = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}.model"));
                if (b is CloudBlockBlob)
                {
                    var c = b as CloudBlockBlob;
                    return new Models.MLModel((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length);
                }                
            }
            catch
            {
                return null;
            }
            return null;
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
                    list.Add(new Models.MLModel((c.Properties.Created ?? DateTime.MinValue).DateTime, RemovePath(c.Name), (int)c.Properties.Length));
                }
            }
            return list;
        }

        private static string RemovePath(string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        public async Task<ServiceConnectivity> GetServiceConnectivity()
        {
            var ev = new ServiceConnectivity();
            var r = await connections.Get<TableSellerCenterCred>(userId, TableSellerCenterCred.signum);
            if (r != null)
                ev.sellercred = new SellerCenterCredentials { LiveMode = r.LiveMode, MerchantId = r.MerchantId, StripeApiKey = r.StripeApiKey };
            var s = await connections.Get<TableSendGridCred>(userId, TableSendGridCred.signum);
            if (s != null)
                ev.sendgridcred = new SendGridCredentials { SendGridAPIKey = s.SendGridAPIKey };
            var t = await connections.Get<TableZendeskCred>(userId, TableZendeskCred.signum);
            if (t != null)
                ev.zendeskcred = new ZendeskCredentials { ZendeskApiKey = t.ZendeskApiKey, ZendeskURL = t.ZendeskURL, ZendeskUser = t.ZendeskUser };
            var tw = await connections.Get<TableTwilioCred>(userId, TableTwilioCred.signum);
            if (tw != null)
                ev.twiliocred = new TwilioCredentials { SMSAccountFrom = tw.SMSAccountFrom, SMSAccountIdentification = tw.SMSAccountIdentification, SMSAccountPassword = tw.SMSAccountPassword };
            var az = await connections.Get<TableAzureCred>(userId, TableAzureCred.signum);
            if (az != null)
                ev.azurecred = new AzureCredentials { AzureAPIKey = az.AzureAPIKey };
            return ev;
        }

        public async Task<List<ConnectivityView>> GetBotConnectivity(string name)
        {
            var list = new List<ConnectivityView>();
            TableQuery<TableCredential> credQuery = new TableQuery<TableCredential>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("user", QueryComparisons.Equal, userId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("model", QueryComparisons.Equal, name)));
            TableContinuationToken continuationToken = null;
            do
            {
                var creds = await credentials.ExecuteQuerySegmentedAsync(credQuery, continuationToken);
                foreach (var c in creds)
                    list.Add(new ConnectivityView { AppId = c.PartitionKey, Password = c.password });
                continuationToken = creds.ContinuationToken;
            } while (continuationToken != null);
            return list;
        }

        public async Task<List<BotUsage>> GetBotUsage(string appId)
        {
            var list = new List<BotUsage>();
            TableQuery<TableBotUsage> defQuery = new TableQuery<TableBotUsage>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, appId));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await botusages.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach(var d in defs)
                    list.Add(new BotUsage(DateTime.Parse(d.RowKey), d.Count));
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            list.Sort();
            return list;
        }

        public async Task<List<Contact>> GetContacts()
        {
            var list = new List<Contact>();
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach(var d in defs)
                list.Add(new Contact { Company = d.Company, Country = d.Country,Created = d.Created.ToString(), Email = d.Email, FirstName = d.FirstName, IntroSent = d.InfoSent, LastName = d.LastName, Notes = d.Notes, Phone = d.Phone, Sector = d.Sector, Source = d.Source  });
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            return list;
        }

        public async Task<List<Contact>> GetContactsByLastName(string lastName)
        {
            var list = new List<Contact>();
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("LastName", QueryComparisons.Equal, lastName)));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach (var d in defs)
                    list.Add(new Contact { Company = d.Company, Country = d.Country, Created = d.Created.ToString(), Email = d.Email, FirstName = d.FirstName, IntroSent = d.InfoSent, LastName = d.LastName, Notes = d.Notes, Phone = d.Phone, Sector = d.Sector, Source = d.Source });
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            return list;
        }

        public async Task<List<Contact>> GetContactsByEmail(string email)
        {
            var list = new List<Contact>();
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email)));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach (var d in defs)
                    list.Add(new Contact { Company = d.Company, Country = d.Country, Created = d.Created.ToString(), Email = d.Email, FirstName = d.FirstName, IntroSent = d.InfoSent, LastName = d.LastName, Notes = d.Notes, Phone = d.Phone, Sector = d.Sector, Source = d.Source });
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            return list;
        }

        public async Task<Contact> GetContactById(string Id)
        {
            var tc = await contacts.Get<TableContacts>(userId, Id);
            return new Contact { RowKey = tc.RowKey, Company = tc.Company, Country = tc.Country, Created = tc.Created.ToString(), Email = tc.Email, FirstName = tc.FirstName, IntroSent = tc.InfoSent, LastName = tc.LastName, Notes = tc.Notes, Phone = tc.Phone, Sector = tc.Sector, Source = tc.Source, Title = tc.Title };
        }

        public async Task<List<TableAuthorizations>> GetAuthorizations(string name)
        {
            var list = new List<TableAuthorizations>();
            TableQuery<TableAuthorizations> defQuery = new TableQuery<TableAuthorizations>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await authorizations.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                list.AddRange(defs);
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            return list.Where(a => a.modelName == name).ToList();
        }

        public async Task<List<Default>> GetDefaults()
        {
            var list = new List<Default>();
            TableQuery<TableDefaults> defQuery = new TableQuery<TableDefaults>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await defaults.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach (var d in defs)
                    list.Add(new Default { RowKey = d.RowKey, Value = d.Value });
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            return list;
        }

        public async Task<string> GetDefaultValue(string name)
        {
            var r =  await defaults.Get<TableDefaults>(userId, name);
            return r == null ? "" : r.Value;
        }

        public async Task DeleteBotModel(string name)
        {
            await botBlob.GetBlockBlobReference($"{userId}/{name}").DeleteAsync();
        }

        public async Task SaveModel(string userId, string modelName, LineageModel model)
        {
            using (var ms = new MemoryStream())
            {
                model.Store(ms);
                ms.Position = 0;
                await botBlob.GetBlockBlobReference($"{userId}/{modelName}").UploadFromStreamAsync(ms);
            }
        }

        public async Task<BotModel> CreateEmptyModel(string name)
        {
            var lm = new LineageModel() { tree = new LineageMatchTree() };
            await SaveModel(userId, name, lm);
            return GetBotModel(name);
        }

        public async Task<Models.MLModel> CreateEmptyMLModel(string name)
        {
            var ml = new DarlCommon.MLModel() { name = name, sets = 3, percentTest = 0, version = "0.0" };
            await SaveMLModel(userId, name, ml);
            return GetMlModel(name);
        }

        private async Task SaveMLModel(string userId, string name, DarlCommon.MLModel mlm)
        {
            var json = JsonConvert.SerializeObject(mlm, Formatting.Indented, new StringEnumConverter());
            using (var stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                var blockBlob = mlmodelsBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{name}"));
                await blockBlob.UploadFromStreamAsync(stream);
            }
        }

        public async Task DeleteMLModel(string name)
        {
            await mlmodelsBlob.GetBlockBlobReference($"{userId}/{name}").DeleteAsync();
        }

        public async Task<RuleSet> CreateEmptyRuleSet(string name)
        {
            var rf = new RuleForm { name = name, darl = "ruleset new_ruleset/n{/n}/n" };
            await SaveRuleSet(userId, name, rf);
            return GetRuleSet(name);
        }

        public async Task DeleteRuleSet(string name)
        {
            await ruleBlob.GetBlockBlobReference($"{userId}/{name}").DeleteAsync();
        }

        public async Task SaveRuleSet(string userId, string rulesetName, RuleForm rf)
        {
            try
            {
                var json = JsonConvert.SerializeObject(rf, Formatting.Indented, new StringEnumConverter());
                using (var stream = new MemoryStream())
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(json);
                    writer.Flush();
                    stream.Position = 0;
                    var blockBlob = ruleBlob.GetBlockBlobReference(WebUtility.HtmlEncode($"{userId}/{rulesetName}"));
                    await blockBlob.UploadFromStreamAsync(stream);
                }
            }
            catch
            {

            }
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            var tc = new TableContacts { PartitionKey = userId, RowKey = contact.RowKey, Company = contact.Company, FirstName = contact.FirstName, LastName = contact.LastName, Email = contact.Email, InfoSent = contact.IntroSent, Country = contact.Country, Notes = contact.Notes, Phone = contact.Phone, Sector = contact.Sector, Source = contact.Source, Title = contact.Title };
            var insertOperation = TableOperation.InsertOrReplace(tc);
            await contacts.ExecuteAsync(insertOperation);
            return await GetContactById(contact.RowKey);
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            var tc = new TableContacts { PartitionKey = userId, RowKey = contact.RowKey, Company = contact.Company, FirstName = contact.FirstName, LastName = contact.LastName, Email = contact.Email, InfoSent = contact.IntroSent, Country = contact.Country, Notes = contact.Notes, Phone = contact.Phone, Sector = contact.Sector, Source = contact.Source, Title = contact.Title };
            var insertOperation = TableOperation.Merge(tc);
            await contacts.ExecuteAsync(insertOperation);
            return await GetContactById(contact.RowKey);
        }

        public async Task DeleteContactAsync(string id)
        {
            var contact = await contacts.Get<TableContacts>(userId, id);
            if (contact != null)
            {
                var deleteOperation = TableOperation.Delete(contact);
                await contacts.ExecuteAsync(deleteOperation);
            }
        }

        public Task<RuleForm> CreateRuleFormFromDarl(string name, string darl)
        {
            throw new NotImplementedException();
        }

        public Task<InputFormat> UpdateRuleFormInputFormat(string name, string inputName, InputFormatUpdate inputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<OutputFormat> UpdateRuleFormOutputFormat(string ruleSetName, string outputName, OutputFormatUpdate outputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<LanguageText> UpdateRuleFormLanguageText(string ruleSetName, string languageName, string languageText)
        {
            throw new NotImplementedException();
        }

        public Task<VariantText> UpdateRuleFormVariantText(string ruleSetName, string languageName, string isoLanguageName, string variantText)
        {
            throw new NotImplementedException();
        }

        public Task<Default> CreateUpdateDefault(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<Default> DeleteDefault(string name)
        {
            throw new NotImplementedException();
        }

        public Task<TableAuthorizations> CreateAuthorization(string name, string name1)
        {
            throw new NotImplementedException();
        }

        public Task<TableAuthorizations> DeleteAuthorization(string name, string name1)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectivityView> CreateBotConnection(string botModelName, string appId, string password)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectivityView> DeleteBotConnection(string botModelName, string appId)
        {
            throw new NotImplementedException();
        }

        public Task<BotInputFormat> UpdateBotModelInputFormat(string botModelName, string inputName, InputFormatUpdate inputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<BotOutputFormat> UpdateBotModelOutputFormat(string botModelName, string outputName, BotOutputFormatUpdate outputUpdate)
        {
            throw new NotImplementedException();
        }

        public Task<StringDoublePair> CreateUpdateConstant(string botModelName, string name, double value)
        {
            throw new NotImplementedException();
        }

        public Task<StringDoublePair> DeleteConstant(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateUpdateStore(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteStore(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<StringStringPair> CreateUpdateString(string botModelName, string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task<StringStringPair> DeleteString(string botModelName, string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetChildrenLineageNodes(string botModelName, string path, bool isRoot)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageRecord>> GetLineagesForWord(string isoLanguage, string word)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetAttribute(string botModelName, string phrase)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineageNodeDefinition>> GetAttributeFromPath(string botModelName, string path)
        {
            throw new NotImplementedException();
        }

        public Task<LineageNodeDefinitionUpdate> UpdateAttribute(string botModelName, LineageNodeDefinitionUpdate attribute)
        {
            throw new NotImplementedException();
        }
    }
}
