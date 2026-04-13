/// <summary>
/// KGClientAccessTests.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.Licensing;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class KGClientAccessTests
    {
        GraphQLHttpClient client = null;
        private IConfiguration _config;
        private BlobGraphConnectivity blob;
        private CosmosDBConnectivity conn;

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();

            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("7ecb39be-fb44-4c13-92df-68ec152a4edb");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("2495b08b-93c3-4498-85b7-f4bdd36b6f01");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            //configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("33db770b-29e9-46ae-8a19-c1947bd775d8");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("a26560b3-7778-410b-a54b-b65da6a9649a");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:GraphContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");


            var blogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var clogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var clicense = new Mock<ILicensing>();
            var cache = new Mock<IDistributedCache>();
            _config = configuration.Object;
            blob = new BlobGraphConnectivity(_config, blogger.Object);
            client = new GraphQLHttpClient("https://darl.dev/graphql/", new NewtonsoftJsonSerializer());
            var authcode = "8952d1af-9d34-4866-a4bc-412bf51743d6";
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authcode);
            conn = new CosmosDBConnectivity(_config, clogger.Object);
        }

        [TestMethod]
        public async Task SetUpCosmosRecordsForKG()
        {
            var container = _config["AppSettings:GraphContainer"];
            //for each blob across accounts in darldevgraphs
            foreach (var url in blob.List(string.Empty))
            {
                //add a KGraph entry
                var l = url.IndexOf(container) + container.Length + 1;
                var composite = url.Substring(l);
                var separator = composite.IndexOf('_');
                var userId = composite.Substring(0, separator);
                var graphname = composite.Substring(separator + 1);
                var res = await conn.CreateKGraph(userId, graphname);
            }
        }
    }
}
