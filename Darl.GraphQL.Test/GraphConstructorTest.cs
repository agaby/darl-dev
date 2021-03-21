using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class GraphConstructorTest
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

        private static string graphName = "personality_test.graph";
        private string sourceName = "Personality test.rule";
        private string appraisal = "noun:01,0,0,11,0,06,1";
        private static string displayLineage = "noun:00,1,00,3,10,09,06";
        private static string textLineage = "noun:01,4,04,02,07,01";
        private static string consistsLineage = "verb:019,031";


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
            _graph = new GraphProcessing(_primitives, glogger.Object, meta.Object);
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
        public async Task ConvertRuleSet()
        {
            var rset = await _conn.GetRuleSet("8c663676-a7dc-4561-af3d-89b38555837d", sourceName);
//            var kg = await _graph.CreateNewGraph(_config["userId"], graphName);
            var darlRuntime = new DarlRunTime();
            var sourceTree = darlRuntime.CreateTree(rset.Contents.darl);
            var compositeName = $"{_config["userId"]}_{graphName}";
            foreach(var input in sourceTree.GetMapInputs())
            {
                var text = rset.Contents.language.LanguageList.Where(a => a.Name == input.Name).First().Text;
                //create a leaf node with a text attribute. For the personality test, questions can be negative or positive, so split text and display.
                await _graph.CreateGraphObject(compositeName, new GraphObjectInput {name = input.Name, externalId = input.Name, lineage = appraisal, properties = new List<GraphAttribute> { new GraphAttribute { type = GraphAttribute.DataType.textual, name = "text", value = text, lineage = textLineage } } }, OntologyAction.build);
            }
            await _graph.Store(compositeName);
        }
    }
}
