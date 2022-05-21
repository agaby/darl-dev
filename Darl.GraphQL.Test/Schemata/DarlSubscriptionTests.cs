using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class DarlSubscriptionTests
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
        private IMetaStructureHandler meta;
        private IGraphHandler _graphHandler;
        private IDarlMetaRunTime _runtime;


        private static readonly string graphName = "backoffice_test.graph";

        [TestInitialize]
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
            configuration.Setup(a => a[It.Is<string>(s => s == "LOCALDATABASEPATH")]).Returns("localdb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:GraphContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");
            configuration.Setup(a => a[It.Is<string>(s => s == "licensing:darlMetaLicense")]).Returns("RwEAAB+LCAAAAAAAAApVkEtPwzAQhO+V+h984xCEyauUyrVoHqKOkjQlUVRxc4mhLnk6sUr49UQWCDh+s7Mzq0Uhf2F1z/B8BgDKxpbhdKB1QUWBoEI18D9aLujAmxpnJ3kNdBMEsgbGrWEB3VhZ9sq4B49RhuAfp9p0ZT80FROKJo5pxbAnwKYuxqsekASEw1Sl5G+LX1Fe4l62bSOGh+mU8obyKVnJKhT+S0Upf6vpIAXDkb93iR/LT+891+y83bsmLM6tvnBOZ/J6l5Ry8WQcD2PwrBVenmsb7ljEHHfJznMO22WXdHpXenlIuN4E8SKNDMs+biNikiVrLus1gr9d8xmCP9/7AhubQj1HAQAA");

            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var ghlogger = new Mock<ILogger<GraphHandler>>();
            var lclogger = new Mock<ILogger<LocalConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobGraphConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            meta = new MetaStructureHandler();
            _conn = new LocalConnectivity(_config, lclogger.Object);
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            var trans = new Mock<IKGTranslation>();
            var lic = new Mock<ILicensing>();
            var lcache = new Mock<IMemoryCache>();
            _primitives = new BlobGraphPrimitives(blob, cache.Object, _conn, bgplogger.Object, lic.Object, lcache.Object);
            var dataLoader = new DataLoader(meta);
            _graph = new GraphProcessing(_primitives, glogger.Object, meta, dataLoader);
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
            _runtime = new DarlMetaRunTime(_config, meta);
            _graphHandler = new GraphHandler(_config, _graph, ghlogger.Object,meta);
        }

        [TestMethod]
        public async Task DarlSubscriptionTest()
        {
            KnowledgeState returnedKnowledgeState;
            var disp = _graph.ObservableKStates().Subscribe(x => returnedKnowledgeState = x);
            await _graph.CreateKnowledgeState(_config["userId"], new KnowledgeStateInput { knowledgeGraphName = graphName, subjectId = "fred" });
        }
    }
}