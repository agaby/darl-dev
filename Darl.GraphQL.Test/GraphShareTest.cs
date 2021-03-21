using Microsoft.VisualStudio.TestTools.UnitTesting;
using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Darl.GraphQL.Models.Models;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class GraphShareTest
    {
        private IGraphProcessing _graph;
        private IConfiguration _config;
        private IGraphPrimitives _primitives;
        private ILocalStore _graphStore;
        private IConnectivity _conn;
        private IFormApi _form;
        private IRuleFormInterface _rf;
        private ITrigger _trigger;
        private ILogger<BotProcessing> _bplogger;
        private IHttpContextAccessor _context;

        private static string graphName = "ai_triage.graph";

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();

            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("7ecb39be-fb44-4c13-92df-68ec152a4edb");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("2495b08b-93c3-4498-85b7-f4bdd36b6f01");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            //configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("33db770b-29e9-46ae-8a19-c1947bd775d8");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("a26560b3-7778-410b-a54b-b65da6a9649a");//andy@darl.ai account
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:GraphContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeAPIKey")]).Returns("sk_live_gKHiW6CmjAgGjhb9x3FY6n9H");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeCorporateLicensePlan")]).Returns("plan_DINGKECtG7jIs5");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeCorporateUsagePlan")]).Returns("plan_DINOybSsS0vMXf");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:boaiuserid")]).Returns("8c663676-a7dc-4561-af3d-89b38555837d");
            //           configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeTrialPeriodDays")]).Returns<int>(a >= 30);



            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobConnectivity>>();//
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobGraphConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            var clogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var clicense = new Mock<ILicensing>();
            var meta = new Mock<IMetaStructureHandler>();
            _conn = new CosmosDBConnectivity(_config, clogger.Object, clicense.Object, cache.Object);
            _primitives = new BlobGraphPrimitives(new List<IBlobConnectivity> { blob }, cache.Object, _conn, bgplogger.Object);
            _graph = new GraphProcessing(_primitives, glogger.Object,meta.Object);
            _graphStore = new GraphLocalStore(_config, logger.Object, context.Object, _graph);
            var form = new Mock<IFormApi>();
            _form = form.Object;
            var rf = new Mock<IRuleFormInterface>();
            _rf = rf.Object;
            var trigger = new Mock<ITrigger>();
            _trigger = trigger.Object;
            var bplogger = new Mock<ILogger<BotProcessing>>();
            _bplogger = bplogger.Object;
            _context = context.Object;
        }

        [TestMethod]
        [Ignore]
        public async Task ShareText()
        {
            var demoUser = _config["AppSettings:boaiuserid"];
//            await _conn.UpdateSubscriptionType(daveUser, DarlUser.SubscriptionType.corporate);
            await _conn.ShareKGraph(_config["userId"],graphName, demoUser, true);
        }

        /// <summary>
        /// Shares descriptions from the masters to shared KGs in the demo account
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore]
        public async Task ShareDescriptions()
        {
            var dest = await _conn.GetKGraphsAsync(_config["AppSettings:boaiuserid"]);
            foreach(var kg in dest)
            {
                if(kg.Shared)
                {
                    var shared = await _conn.GetKGModel(kg.OwnerId, kg.Name);
                    if(shared != null)
                    {
                        await _conn.UpdateKGraph(_config["AppSettings:boaiuserid"], kg.Name, new KGraphUpdate { Description = shared.Description });
                    }
                }
            }
        }

        [TestMethod]
        public async Task CorrectRecognitionLabels()
        {
            //read a KG
            //access the recognition objects,
            var compositeName = $"{_config["userId"]}_{graphName}";
            var rec = await _graph.GetRecognitionDisplayGraph(compositeName);
            foreach(var ob in rec.nodes)
            {
                var recObj = await _graph.GetRecognitionObjectById(compositeName, ob.id);
                if (string.IsNullOrEmpty(recObj.name) || recObj.name == recObj.lineage)
                {
                    if (recObj.lineage == "navigation:")
                    {
                        recObj.name = "navigation root";
                    }
                    else
                    {
                        //convert the name to be the typeword if present.
                        var typeword = await _conn.GetTypeWordForLineage(recObj.lineage);
                        if (!string.IsNullOrEmpty(typeword))
                            recObj.name = $"~{typeword}";
                    }
                }
            }
            //save kgraph.
            await _graph.Store(compositeName);
        }

        [TestMethod]
        public async Task AddDefaultRecognitionTree()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _graph.GetModel(_config["userId"], graphName) as BlobGraphContent;
            var p = _primitives as BlobGraphPrimitives;
            model.recognitionVertices.Clear();
            model.recognitionRoots.Clear();
            model.recognitionEdges.Clear();
            p.AddDefaultContent(model);
            await _graph.Store(compositeName);
        }
    }
}
