using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Darl.GraphQL.Test
{
    [TestClass]
    public class CursusHonorumTest
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

        private static string industryLineage = "noun:01,2,07,10,14,3,1";
        private static string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";
        private static string jobLineage = "noun:01,0,2,00,23,19";
        private static string areaLineage = "noun:01,1,00,10,09,5";
        private static string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static string courseLineage = "noun:01,0,2,00,23,29,02";
        private static string abilityLineage = "noun:01,0,0,04";
        private static string enableLineage = "verb:013,210";
        private static string ruleLineage = "noun:01,0,2,00,23,44,15";
        private static string personLineage = "noun:00,2,00";
        private static string universityLineage = "noun:01,2,07,10,13,7,4";
        private static string learningOutcomeLineage = "noun:01,0,0,15,16,2";
        private static string ownLineage = "verb:393";
        private static string consistsLineage = "verb:019,031";
        private static string teachLineage = "verb:034,30,01,09,01";
        private static string topicLineage = "noun:01,4,05,06";
        private static string skillLineage = "noun:01,0,0,04";
        private static string createLineage = "verb:023";
        private static string requireLineage = "verb:145";
        private static string descriptionLineage = "noun:01,4,05,21,05";
        private static string functionLineage = "noun:01,0,2,00,23,16,21,1";
        private static string careerLineage = "noun:01,0,2,00,00,15,20,01,1";
        private static string huntingLineage = "noun:01,0,2,00,23,35";
        private static string personalityLineage = "noun:01,1,09";
        private static string liveLineage = "adjective:7763";
        private static string studentLineage = "noun:00,2,00,175,0";
        private static string mathsLineage = "noun:01,0,0,15,21,0,08,02";
        private static string yearLineage = "noun:01,5,03,3,045";
        private static string followsLineage = "verb:534";
        private static string activityLineage = "noun:01,0,2,00,23";
        private static string testLineage = "noun:01,0,2,00,38,09";
        private static string completeLineage = "adjective:5500";
        private static string subactivityLineage = "noun:01,0,2,00,00";
        private static string questionLineage = "noun:01,0,2,00,39,08,08,1";
        private static string displayLineage = "noun:00,1,00,3,10,09,06";
        private static string textLineage = "noun:01,4,04,02,07,01";


        private static string graphName = "cursus_honorum.graph";
        private static string graphImage = "rf1.graphml";
         
        // Julius Caesar dates: Born 100BC, Military service 81BC, 71BC Military Tribune, Quaestor 69BC, 65BC Curule Aedile, 62BC Praetor, propraetor 61BC, Consul 59BC,  proconsul 58BC, Dictator 48BC https://en.wikipedia.org/wiki/Julius_Caesar

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

            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobGraphConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            var conn = new Mock<IConnectivity>();
            _conn = conn.Object;
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            conn.Setup(a => a.GetKnowledgeState(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<KnowledgeState>(new KnowledgeState()));
            conn.Setup(a => a.UpdateKnowledgeState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KnowledgeStateUpdate>()));
            _primitives = new BlobGraphPrimitives(new List<IBlobConnectivity> { blob }, cache.Object, conn.Object);
            _graph = new GraphProcessing(_primitives);
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
        public async Task CreateGraphTest()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            await _graph.ClearGraphContent(compositeName);
            var censor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Censor", lineage = "noun:00,2,00,127", externalId = "Censor" }, OntologyAction.build);
            var dictator = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Dictator", lineage = "noun:00,2,00,320,04", externalId = "Dictator" }, OntologyAction.build);
            var proconsul = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Proconsul", lineage = "noun:00,2,00,033,34,0,5", externalId = "Proconsul" }, OntologyAction.build);
            var consul = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Consul", lineage = "noun:00,2,00,050,43,35,14,02", externalId = "Consul" }, OntologyAction.build);
            var propraetor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Propraetor", lineage = "noun:00,2,00,080,0,07", externalId = "Propraetor" }, OntologyAction.build);
            var praetor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Praetor", lineage = "noun:00,2,00,080,0,07", externalId = "Praetor" }, OntologyAction.build);
            var curule_aedile = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Curule aedile", lineage = "noun:00,2,00,050,43,35,36", externalId = "Curule_aedile" }, OntologyAction.build);
            var plebian_aedile = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Plebian aedile", lineage = "noun:00,2,00,050,43,35,36", externalId = "Plebian_aedile" }, OntologyAction.build);
            var proquaestor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Proquaestor", lineage = "noun:00,2,00,050,43,35,36", externalId = "Proquaestor" }, OntologyAction.build);
            var quaestor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Quaestor", lineage = "noun:00,2,00,050,43,35,36", externalId = "Quaestor" }, OntologyAction.build);
            var tribune_of_the_plebs = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Tribune of the plebs", lineage = "noun:00,2,00,296,0,12", externalId = "Tribune_of_the_plebs" }, OntologyAction.build);
            var military_tribune = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Military Tribune", lineage = "noun:00,2,00,296,0,12", externalId = "Military_tribune" }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = censor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = proconsul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = dictator.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = propraetor.id, endId = consul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = praetor.id, endId = consul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = praetor.id, endId = propraetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = curule_aedile.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = quaestor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = plebian_aedile.id, endId = quaestor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = tribune_of_the_plebs.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = proquaestor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = curule_aedile.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = tribune_of_the_plebs.id, endId = plebian_aedile.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = tribune_of_the_plebs.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.Store(compositeName);

        }
    }


}
