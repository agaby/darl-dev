using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private IMetaStructureHandler meta;
        private IGraphHandler _graphHandler;
        private IDarlMetaRunTime _runtime;

        private static readonly string industryLineage = "noun:01,2,07,10,14,3,1";
        private static readonly string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";
        private static readonly string jobLineage = "noun:01,0,2,00,23,19";
        private static readonly string areaLineage = "noun:01,1,00,10,09,5";
        private static readonly string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static readonly string courseLineage = "noun:01,0,2,00,23,29,02";
        private static readonly string abilityLineage = "noun:01,0,0,04";
        private static readonly string enableLineage = "verb:013,210";
        private static readonly string ruleLineage = "noun:01,0,2,00,23,44,15";
        private static readonly string personLineage = "noun:00,2,00";
        private static readonly string universityLineage = "noun:01,2,07,10,13,7,4";
        private static readonly string learningOutcomeLineage = "noun:01,0,0,15,16,2";
        private static readonly string ownLineage = "verb:393";
        private static readonly string consistsLineage = "verb:019,031";
        private static readonly string teachLineage = "verb:034,30,01,09,01";
        private static readonly string topicLineage = "noun:01,4,05,06";
        private static readonly string skillLineage = "noun:01,0,0,04";
        private static readonly string createLineage = "verb:023";
        private static readonly string requireLineage = "verb:145";
        private static readonly string descriptionLineage = "noun:01,4,05,21,05";
        private static readonly string functionLineage = "noun:01,0,2,00,23,16,21,1";
        private static readonly string careerLineage = "noun:01,0,2,00,00,15,20,01,1";
        private static readonly string huntingLineage = "noun:01,0,2,00,23,35";
        private static readonly string personalityLineage = "noun:01,1,09";
        private static readonly string liveLineage = "adjective:7763";
        private static readonly string studentLineage = "noun:00,2,00,175,0";
        private static readonly string mathsLineage = "noun:01,0,0,15,21,0,08,02";
        private static readonly string yearLineage = "noun:01,5,03,3,045";
        private static readonly string followsLineage = "verb:534";
        private static readonly string activityLineage = "noun:01,0,2,00,23";
        private static readonly string testLineage = "noun:01,0,2,00,38,09";
        private static readonly string completeLineage = "adjective:5500";
        private static readonly string subactivityLineage = "noun:01,0,2,00,00";
        private static readonly string questionLineage = "noun:01,0,2,00,39,08,08,1";
        private static readonly string displayLineage = "noun:00,1,00,3,10,09,06";
        private static readonly string textLineage = "noun:01,4,04,02,07,01";
        private static readonly string senatorLineage = "noun:00,2,00,033,34,1,10";
        private static readonly string lictorLineage = "noun:00,2,00,296,0,01";
        private static readonly string socialClassLineage = "noun:01,2,06,34";


        private static readonly string graphName = "cursus_honorum.graph";
        private static readonly string graphImage = "rf1.graphml";

        // Julius Caesar dates: Born 100BC, Military service 81BC, 71BC Military Tribune, Quaestor 69BC, 65BC Curule Aedile, 62BC Praetor, propraetor 61BC, Consul 59BC,  proconsul 58BC, Dictator 48BC https://en.wikipedia.org/wiki/Julius_Caesar

        [TestInitialize()]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                .AddUserSecrets<CursusHonorumTest>()
                .Build();
            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var ghlogger = new Mock<ILogger<GraphHandler>>();
            var lclogger = new Mock<ILogger<LocalConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
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
            _graphHandler = new GraphHandler(_config, _graph, ghlogger.Object, meta);

        }

        [TestMethod]
        public async Task CreateGraphTest()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            await _graph.ClearGraphContent(compositeName);
            var senator = new GraphAttributeInput
            {
                name = "senator",
                value = "true",
                lineage = senatorLineage,
                type = GraphAttribute.DataType.categorical,
                properties = new List<GraphAttributeInput> {
                    new GraphAttributeInput {name = "category", lineage = meta.CommonLineages["category"], type = GraphAttribute.DataType.textual, value = "true"},
                    new GraphAttributeInput {name = "category", lineage = meta.CommonLineages["category"], type = GraphAttribute.DataType.textual, value = "false"}
                }
            };
            var socialClass = new GraphAttributeInput
            {
                name = "class",
                lineage = socialClassLineage,
                type = GraphAttribute.DataType.categorical,
                properties = new List<GraphAttributeInput> {
                    new GraphAttributeInput {name = "category", lineage = meta.CommonLineages["category"], type = GraphAttribute.DataType.textual, value = "plebian"},
                    new GraphAttributeInput {name = "category", lineage = meta.CommonLineages["category"], type = GraphAttribute.DataType.textual, value = "equites"},
                    new GraphAttributeInput {name = "category", lineage = meta.CommonLineages["category"], type = GraphAttribute.DataType.textual, value = "patrician"}
                }
            };
            var censor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Censor", lineage = "noun:00,2,00,127", externalId = "Censor", properties = new List<GraphAttributeInput> { senator } }, OntologyAction.build);
            var dictator = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Dictator", lineage = "noun:00,2,00,320,04", externalId = "Dictator", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "24", lineage = lictorLineage } } }, OntologyAction.build);
            var proconsul = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Proconsul", lineage = "noun:00,2,00,033,34,0,5", externalId = "Proconsul", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "12", lineage = lictorLineage } } }, OntologyAction.build);
            var consul = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Consul", lineage = "noun:00,2,00,050,43,35,14,02", externalId = "Consul", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "12", lineage = lictorLineage } } }, OntologyAction.build);
            var propraetor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Propraetor", lineage = "noun:00,2,00,080,0,07", externalId = "Propraetor", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "6", lineage = lictorLineage } } }, OntologyAction.build);
            var praetor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Praetor", lineage = "noun:00,2,00,080,0,07", externalId = "Praetor", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "6", lineage = lictorLineage } } }, OntologyAction.build);
            var curule_aedile = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Curule aedile", lineage = "noun:00,2,00,050,43,35,36", externalId = "Curule_aedile", properties = new List<GraphAttributeInput> { senator, new GraphAttributeInput { name = "lictors", value = "2", lineage = lictorLineage } } }, OntologyAction.build);
            var plebian_aedile = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Plebian aedile", lineage = "noun:00,2,00,050,43,35,36", externalId = "Plebian_aedile", properties = new List<GraphAttributeInput> { senator } }, OntologyAction.build);
            var proquaestor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Proquaestor", lineage = "noun:00,2,00,050,43,35,36", externalId = "Proquaestor", properties = new List<GraphAttributeInput> { senator } }, OntologyAction.build);
            var quaestor = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Quaestor", lineage = "noun:00,2,00,050,43,35,36", externalId = "Quaestor", properties = new List<GraphAttributeInput> { senator } }, OntologyAction.build);
            var tribune_of_the_plebs = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Tribune of the plebs", lineage = "noun:00,2,00,296,0,12", externalId = "Tribune_of_the_plebs", properties = new List<GraphAttributeInput> { senator } }, OntologyAction.build);
            var military_tribune = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Military Tribune", lineage = "noun:00,2,00,296,0,12", externalId = "Military_tribune" }, OntologyAction.build);
            var candidate = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "Candidate", lineage = meta.CommonLineages["person"], externalId = "candidate", properties = new List<GraphAttributeInput> { socialClass } }, OntologyAction.build);

            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = censor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = proconsul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = consul.id, endId = dictator.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = propraetor.id, endId = consul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = praetor.id, endId = consul.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = praetor.id, endId = propraetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = curule_aedile.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = quaestor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = plebian_aedile.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = tribune_of_the_plebs.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = proquaestor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = quaestor.id, endId = curule_aedile.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = tribune_of_the_plebs.id, endId = plebian_aedile.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = praetor.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);
            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = military_tribune.id, endId = tribune_of_the_plebs.id, lineage = followsLineage, name = "can be followed by", weight = 1.0 }, OntologyAction.build);

            var tpconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = tribune_of_the_plebs.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var coconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = consul.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var ppconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = propraetor.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var prconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = praetor.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var caconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = curule_aedile.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var quconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = quaestor.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var mtconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = military_tribune.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var paconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = plebian_aedile.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var diconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = dictator.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var pqconn = await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = candidate.id, endId = proquaestor.id, lineage = meta.CommonLineages["have"], name = "can hold", weight = 1.0 }, OntologyAction.build);
            var subjectId = "poopies";
            var kr = new KnowledgeRecordInput { subjectId = subjectId, knowledgeGraphName = graphName };
            kr.AddReference(candidate, "class", "plebian");
            await _graph.CreateKnowledgeState(_config["userId"], kr);
            var sb = new System.Text.StringBuilder();
            var res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, null);
            var log = sb.ToString();
            sb.Clear();
            Assert.AreEqual(13, res.data.Count);
            //Now Add existences to the objects and check for overlap
            kr.AddExistence(candidate, new List<Common.DarlTime> { new Common.DarlTime(-100, 1), new Common.DarlTime(-44, 0) });//life dates of Julius Caesar
            await _graph.CreateKnowledgeState(_config["userId"], kr);
            consul.existence = new List<Common.DarlTime> { new Common.DarlTime(-589, 1), new Common.DarlTime(887, 0) };
            censor.existence = new List<Common.DarlTime> { new Common.DarlTime(-443, 1), new Common.DarlTime(-22, 0) };
            proconsul.existence = new List<Common.DarlTime> { new Common.DarlTime(-327, 1), new Common.DarlTime(420, 0) };
            praetor.existence = new List<Common.DarlTime> { new Common.DarlTime(-366, 1), new Common.DarlTime(395, 0) };
            propraetor.existence = new List<Common.DarlTime> { new Common.DarlTime(-290, 1), new Common.DarlTime(395, 0) };
            curule_aedile.existence = new List<Common.DarlTime> { new Common.DarlTime(-367, 1), new Common.DarlTime(200, 0) };
            plebian_aedile.existence = new List<Common.DarlTime> { new Common.DarlTime(-494, 1), new Common.DarlTime(200, 0) };
            quaestor.existence = new List<Common.DarlTime> { new Common.DarlTime(-420, 1), new Common.DarlTime(500, 0) };
            proquaestor.existence = new List<Common.DarlTime> { new Common.DarlTime(-420, 1), new Common.DarlTime(500, 0) };
            tribune_of_the_plebs.existence = new List<Common.DarlTime> { new Common.DarlTime(-493, 1), new Common.DarlTime(-23, 0) };
            military_tribune.existence = new List<Common.DarlTime> { new Common.DarlTime(-311, 1), new Common.DarlTime(-27, 0) };
            dictator.existence = new List<Common.DarlTime> { new Common.DarlTime(-501, 1), new Common.DarlTime(-47, 0) };
            //add checks for mutual existence.
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, null);
            log = sb.ToString();
            sb.Clear();
            Assert.AreEqual(13, res.data.Count);
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(Common.DarlTime.UtcNow));
            log = sb.ToString();
            sb.Clear();
            Assert.AreEqual(1, res.data.Count);
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-50, 0)));
            log = sb.ToString();
            sb.Clear();
            Assert.AreEqual(13, res.data.Count);
            //add state of candidate as military tribune and the period.
            tpconn.inferred = true;
            coconn.inferred = true;
            ppconn.inferred = true;
            prconn.inferred = true;
            caconn.inferred = true;
            quconn.inferred = true;
            mtconn.inferred = true;
            paconn.inferred = true;
            diconn.inferred = true;
            pqconn.inferred = true;
            kr.AddConnection(mtconn, military_tribune.id, new List<Common.DarlTime> { new Common.DarlTime(-81, 1), new Common.DarlTime(-69, 0) });
            await _graph.CreateKnowledgeState(_config["userId"], kr);
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-50, 0)));
            log = sb.ToString();
            sb.Clear();
            //Add conditions to kg based on birth
            //patrician only
            var patricianRule = $"lineage social_class \"{socialClassLineage}\";\noutput categorical completed {{true, false}} complete;\noutput categorical class {{plebian,equites,patrician}} social_class;\nif anything then class will be single(person,have,social_class);\nif class is patrician then completed will be true; ";
            quaestor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = patricianRule });
            proquaestor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = patricianRule });
            curule_aedile.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = patricianRule });
            //plebian or equites only
            var plebianRule = $"lineage social_class \"{socialClassLineage}\";\noutput categorical completed {{true, false}} complete;\noutput categorical class {{plebian, equites,patrician}} social_class;\nif anything then class will be single(person,have,social_class);\nif class is equites or class is plebian then completed will be true; ";
            tribune_of_the_plebs.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = plebianRule });
            plebian_aedile.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = plebianRule });
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-50, 0)));
            Assert.AreEqual(11, res.data.Count);
            log = sb.ToString();
            sb.Clear();
            kr.AddReference(candidate, "class", "patrician");
            await _graph.CreateKnowledgeState(_config["userId"], kr);
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-50, 0)));
            Assert.AreEqual(12, res.data.Count);
            log = sb.ToString();
            sb.Clear();
            //add conditions based on age
            var military_tribune_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 18Y;\nduration plebian_age 20Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            military_tribune.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = military_tribune_rule });
            var tribune_of_the_plebs_rule = "duration plebian_age 27Y; \noutput categorical completed { true,false};\nif age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            tribune_of_the_plebs.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = tribune_of_the_plebs_rule });
            var quaestor_rule = "duration patrician_age 30Y;\noutput categorical completed { true,false};\nif age(existence(\"candidate\")) is > patrician_age then completed will be true;\n";
            quaestor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = quaestor_rule });
            var proquaestor_rule = "duration patrician_age 31Y;\noutput categorical completed { true,false};\nif age(existence(\"candidate\")) is > patrician_age then completed will be true;\n";
            proquaestor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = proquaestor_rule });
            var curule_aedile_rule = "duration patrician_age 36Y;\noutput categorical completed { true,false};\nif age(existence(\"candidate\")) is > patrician_age then completed will be true;\n";
            curule_aedile.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = curule_aedile_rule });
            var plebian_aedile_rule = "duration plebian_age 36Y;\noutput categorical completed { true,false};\nif age(existence(\"candidate\")) is > plebian_age then completed will be true;\n";
            plebian_aedile.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = plebian_aedile_rule });
            var praetor_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 37Y;\nduration plebian_age 39;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            praetor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = praetor_rule });
            var propraetor_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 38Y;\nduration plebian_age 40Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            propraetor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = propraetor_rule });
            var consul_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 40Y;\nduration plebian_age 42Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            consul.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = consul_rule });
            var proconsul_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 41Y; \nduration plebian_age 43Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            proconsul.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = proconsul_rule });
            var dictator_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 41Y; \nduration plebian_age 43Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            dictator.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = dictator_rule });
            var censor_rule = "lineage social_class \"noun:01,2,06,34\";\nduration patrician_age 41Y; \nduration plebian_age 43Y;\noutput categorical completed { true,false};\noutput categorical class {plebian,equites,patrician} social_class;\nif anything then class will be single(person, have, social_class);\nif class is patrician and age(existence(\"candidate\")) is > patrician_age then completed will be true;\nif (class is plebian or class is equites) and age(existence(\"candidate\")) is > plebian_age then completed will be true;";
            censor.properties.Add(new GraphAttribute { name = "completed", lineage = meta.CommonLineages["complete"], type = GraphAttribute.DataType.ruleset, value = censor_rule });
            //set the current time
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-68, 0)));
            log = sb.ToString();            //Add conditions based on length of service
            Assert.AreEqual(7, res.data.Count);
            sb.Clear();
            //_runtime.SetEvaluationTime(new List<Common.DarlTime> { new Common.DarlTime(-68, 1) });
            res = await _graphHandler.Discover(_config["userId"], graphName, subjectId, new List<string> { meta.CommonLineages["have"], followsLineage }, sb, new Common.FuzzyTime(new Common.DarlTime(-60, 0)));
            log = sb.ToString();            //Add conditions based on length of service

        }
        [TestMethod]
        public async Task TestRuleFormConvert()
        {
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var blogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            string answerLineage = "noun:01,4,05,21,19";
            string appraisalLineage = "noun:01,0,2,00,26,4,0";
            string textLineage = "noun:01,4,04,02,07,01";
            var blob = new BlobGraphConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            var lic = new Mock<ILicensing>();
            var _metastruct = new MetaStructureHandler();
            var connLogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var conv = new CosmosDBConnectivity(_config, connLogger.Object);
            var lcache = new Mock<IMemoryCache>();
            var _prim = new BlobGraphPrimitives(blob, cache.Object, conv, bgplogger.Object, lic.Object, lcache.Object);
            var dataLoader = new DataLoader(_metastruct);
            var graph = new GraphProcessing(_prim, glogger.Object, _metastruct, dataLoader);
            var graphName = "Cocomo-II.graph";
            var model = await graph.CreateNewGraph(_config["userId"], graphName);
            var composite_name = _config["userId"] + "_" + graphName;
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.Cocomo II.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            var objs = new Dictionary<string, GraphObject>();
            var inferredObjs = new Dictionary<string, GraphObject>();
            foreach (var c in rf.format.InputFormatList)
            {
                var obj = await graph.CreateGraphObject(composite_name, new GraphObjectInput { name = c.Name, lineage = appraisalLineage }, OntologyAction.build);
                objs.Add(c.Name, obj);
                obj.properties = new List<GraphAttribute>();
                obj.properties.Add(new GraphAttribute { type = ConvertFormType(c.InType), confidence = 1.0, name = "answer", lineage = answerLineage });
                //add categories and numeric range stuff
            }
            foreach (var t in rf.language.LanguageList)
            {
                if (objs.ContainsKey(t.Name))
                {
                    var obj = objs[t.Name];
                    obj.properties.Add(new GraphAttribute { type = GraphAttribute.DataType.textual, confidence = 1.0, name = "text", lineage = textLineage, value = t.Text });
                }
            }
            foreach (var o in rf.format.OutputFormatList)
            {
                var obj = await graph.CreateGraphObject(composite_name, new GraphObjectInput { name = o.Name, lineage = appraisalLineage }, OntologyAction.build);
                obj.properties = new List<GraphAttribute>();
                inferredObjs.Add(o.Name, obj);
                obj.properties.Add(new GraphAttribute { type = GraphAttribute.DataType.ruleset, confidence = 1.0, name = "completed", lineage = completeLineage });
            }

        }

        private GraphAttribute.DataType ConvertFormType(InputFormat.InputType intype)
        {
            switch (intype)
            {
                case InputFormat.InputType.numeric:
                    return GraphAttribute.DataType.numeric;
                case InputFormat.InputType.textual:
                    return GraphAttribute.DataType.textual;
                case InputFormat.InputType.categorical:
                    return GraphAttribute.DataType.categorical;
                case InputFormat.InputType.temporal:
                    return GraphAttribute.DataType.temporal;
            }
            return GraphAttribute.DataType.numeric;
        }
    }


}
