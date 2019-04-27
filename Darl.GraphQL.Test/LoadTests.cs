using Darl.Connectivity;
using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static Darl.GraphQL.Models.Models.DarlUser;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class LoadTests
    {

        public AppSettings appSettings = null;
        public Darl.Connectivity.AppSettings dAppSettings = null;
        public CloudStorageAccount storageAccount = null;
        public CloudTableClient client = null;
        public string userId = "";
        public CloudBlobClient blobClient = null;

        [TestInitialize()]
        public void Initialize()
        {

            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.AppSettings.json"));
            var source = reader.ReadToEnd();
            appSettings = JsonConvert.DeserializeObject<AppSettings>(source);
            dAppSettings = JsonConvert.DeserializeObject<Darl.Connectivity.AppSettings>(source);
            storageAccount = CloudStorageAccount.Parse(appSettings.StorageConnectionString);
            client = storageAccount.CreateCloudTableClient();
            blobClient = storageAccount.CreateCloudBlobClient();
            userId = appSettings.boaiuserid;
        }

        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        [Ignore]
        public async Task CopyDarlContacts()
        {

            var contacts = client.GetTableReference("contacts");
            var list = new List<TableContacts>();
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, appSettings.boaiuserid));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                list.AddRange(defs);
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var c in list)
            {
                await cosmos.CreateContactAsync(new Models.Models.Contact { Company = c.Company, Country = c.Country, Created = c.Created, Email = c.Email.ToLower(), FirstName = c.FirstName, Id = c.RowKey, IntroSent = c.InfoSent, LastName = c.LastName, Notes = c.Notes, Phone = c.Phone, Sector = c.Sector, Source = c.Source, Title = c.Title });
            }
        }

        [TestMethod]
        [Ignore]
        public async Task CopyDarlDefaults()
        {
            var defaults = client.GetTableReference("defaults");
            var list = new List<TableDefaults>();
            TableQuery<TableDefaults> defQuery = new TableQuery<TableDefaults>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, appSettings.boaiuserid));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await defaults.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                list.AddRange(defs);
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var c in list)
            {
                await cosmos.CreateDefault(c.RowKey,  c.Value );
            }
        }

        [TestMethod]
        [Ignore]
        public async Task CopyDarlRuleSets()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var list = await dr.GetRuleSets(userId);
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var c in list)
            {
                var rf = await dr.GetRuleset(userId, c);
                await cosmos.CreateRuleSet(c,rf,new Models.Models.ServiceConnectivity());
            }
        }

        [TestMethod]
        [Ignore]

        public async Task CopyDarlMLModels()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var list = await dr.GetMLModels(userId);
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var c in list)
            {
                var mm = await dr.GetMLModel(userId, c);
                await cosmos.CreateMLModel(c, mm);
            }
        }

        [TestMethod]
        //[Ignore]

        public async Task CopyDarlBotModels()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var c = "cubebot.model";
            var mm = await dr.GetModel(userId, c);
            await cosmos.CreateBotModel(c, mm, new Models.Models.ServiceConnectivity(), new List<Models.Models.Authorization>(), new List<Models.Models.BotConnection>());
            
        }

        [TestMethod]
        //[Ignore]

        public async Task CopyDarlUsers()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var list = await dr.GetIndividualAccounts();

            foreach (var c in list)
            {
                await cosmos.CreateUserAsync(new Models.Models.DarlUser { accountState = (AccountState)c.accountState, Created = c.Created, current_period_end = c.current_period_end, InvoiceEmail = c.InvoiceEmail, InvoiceName = c.InvoiceName, InvoiceOrganization = c.InvoiceOrganization, Issuer = c.Issuer, PaidUsageStarted = c.PaidUsageStarted, StripeCustomerId = c.StripeCustomerId, UsageStripeSubscriptionItem = c.UsageStripeSubscriptionItem, userId = c.PartitionKey });
            }
        }
    }
}
