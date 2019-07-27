using Darl.Connectivity;
using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
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
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, appSettings.boaiuserid));
            TableContinuationToken continuationToken = null;
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            int count = 0;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                foreach (var c in defs)
                {
                    var existing = await cosmos.GetContactByEmail(c.Email);
                    Thread.Sleep(10);
                    if (existing == null)
                    {
                        await cosmos.CreateContactAsync(new Models.Models.Contact { Company = c.Company, Country = c.Country, Created = c.Created, Email = c.Email.ToLower(), FirstName = c.FirstName, Id = c.RowKey, IntroSent = c.InfoSent, LastName = c.LastName, Notes = c.Notes, Phone = c.Phone, Sector = c.Sector, Source = c.Source, Title = c.Title });
                        count++;
                        Thread.Sleep(10);
                    }
                }
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);
        }
        /*
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
                        }*/

        [TestMethod]
        [Ignore]
        public async Task CopyDarlBotModels()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var c = "cubebot.model";
            var mm = await dr.GetModel(userId, c);

            using (MemoryStream ms = new MemoryStream())
            {
                mm.Store(ms);
                ms.Position = 0;
                await cosmos.CreateBotModel(userId, c, ms.ToArray());
            }
        }
        /*
        [TestMethod]
        [Ignore]
        public async Task CopyDarlUsers()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var list = await dr.GetIndividualAccounts();

            foreach (var c in list)
            {
                await cosmos.CreateUserAsync(new Models.Models.DarlUserInput {  Created = c.Created, current_period_end = c.current_period_end, InvoiceEmail = c.InvoiceEmail, InvoiceName = c.InvoiceName, InvoiceOrganization = c.InvoiceOrganization, Issuer = c.Issuer, PaidUsageStarted = c.PaidUsageStarted, StripeCustomerId = c.StripeCustomerId, UsageStripeSubscriptionItem = c.UsageStripeSubscriptionItem, userId = c.PartitionKey });
            }
        }
        */
        [TestMethod]
        [Ignore]
        public async Task CopyCollateral()
        {
            /*           var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
                       var list = await dr.GetCollaterals(userId);
                       foreach (var c in list)
                       {
                           if(c.EndsWith("md"))
                           { 
                               var rf = await dr.GetCollateral(userId, c);
                               await cosmos.UpdateCollateral(userId, c, rf);
                           }
                       }*/
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            await cosmos.UpdateCollateral(userId, "suggestions.md", File.ReadAllText(@"C:\Users\Andrew\Downloads\suggestions.md"));
            await cosmos.UpdateCollateral(userId, "bot_help.md", File.ReadAllText(@"C:\Users\Andrew\Downloads\bot_help.md"));
        }

        /// <summary>
        /// Copy reference collateral to admin account
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore]
        public async Task CloneCollateral()
        {
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var c in await cosmos.GetCollaterals(userId))
            {
                await cosmos.UpdateCollateral("786e46c2-fa33-4124-af67-1bb14625c216", c.Name, c.Value);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task CreateBotConnections()
        {
            var adminuserId = "786e46c2-fa33-4124-af67-1bb14625c216";
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            //            await cosmos.FactoryReset(adminuserId);
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var creds = await dr.GetAllCredentialsAsync();
            foreach (var c in creds)
            {
                await cosmos.CreateBotConnection(adminuserId, c.model, c.PartitionKey, c.password);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task CreateDocuments()
        {
            var dr = new DarlRepository(new OptionsWrapper<Darl.Connectivity.AppSettings>(dAppSettings));
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            foreach (var doc in await dr.GetDocuments(userId))
            {
                using (var ms = new MemoryStream())
                {
                    var d = new Document { userId = userId, name = doc };
                    await dr.GetDocumentStream(userId, doc, ms);
                    d.content = ms.ToArray();
                    await cosmos.UpdateDocument(d);
                }
            }

        }

        [TestMethod]
        [Ignore]
        public async Task CopyDocuments()
        {
            var adminuserId = "786e46c2-fa33-4124-af67-1bb14625c216";
            var slawUserId = "8a14e17b-268a-4dc8-84fc-95d1a558e737";
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var d = await cosmos.GetDocument(userId, "modernslaverytest.docx");
            d.userId = adminuserId;
            await cosmos.UpdateDocument(d);
            d.userId = slawUserId;
            await cosmos.UpdateDocument(d);
        }

        [TestMethod]
        [Ignore]
        public async Task WriteOutRuleFile()
        {
            var slawUserId = "8a14e17b-268a-4dc8-84fc-95d1a558e737";
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            await cosmos.WriteRuleFormForTest(slawUserId, "military_service.rule", "military_service.rule");
        }
    }
}

