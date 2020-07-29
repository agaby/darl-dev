using CsvHelper;
using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class ContactDownload
    {

        private IConfiguration _config;
        private IConnectivity _conv;

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinHostname")]).Returns("thinkbase.gremlin.cosmosdb.azure.com");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinPort")]).Returns("443");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinAuthKey")]).Returns("ffWKZWMJro4JHBaJAi4yG1o35ujaDvj0pIkrqsYEz4hCoHR9jvHr6YR3Pb2dxr8rw4obuO4ZvnJetejwJyrYQA==");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinDatabase")]).Returns("farleft");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("hypernymy");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("7d1a254f-d405-4385-acbc-308c8376f2e3");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("d70f1008-5758-41b5-9c44-bc90535aeabc");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("786e46c2-fa33-4124-af67-1bb14625c216");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("azure");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "botmodel")]).Returns("thousandquestions.model");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:boaiuserid")]).Returns("8c663676-a7dc-4561-af3d-89b38555837d");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");




            _config = configuration.Object;
            var logger = new Mock<ILogger<GraphLocalStore>>();
            var formLogger = new Mock<ILogger<FormApi>>();
            var botLogger = new Mock<ILogger<BotProcessing>>();
            var connLogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var blobLogger = new Mock<ILogger<BlobConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var licensing = new Mock<ILicensing>();
            _conv = new CosmosDBConnectivity(_config, connLogger.Object, licensing.Object);

        }
        [TestMethod]
        [Ignore]
        public async Task DownloadTest()
        {
            var contacts = await _conv.GetContacts();
            using (var writer = new StreamWriter("contacts.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(contacts);
            }
        }
    }
}
