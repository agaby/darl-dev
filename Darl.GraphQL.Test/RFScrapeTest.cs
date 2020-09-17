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
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class RFScrapeTest
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

        private static string graphName = "rf1.graph";
        private static string graphImage = "rf1.graphml";

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
            conn.Setup(a => a.GetKnowledgeState(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<KnowledgeState>(new KnowledgeState { data = new Dictionary<string, List<GraphAttribute>>() }));
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
        [Ignore]
        public async Task TestScrapeMathsSite()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var sb = new StringBuilder();
            var topics = new HashSet<string>();
            //emit top level node "maths"
            var toplevel = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "maths", lineage = mathsLineage, externalId = "MATH1" }, OntologyAction.build);
            GraphObject currentTopic = null;
            GraphObject currentYear = null;
            GraphObject currentActivity = null;
            int activityCount = 1;
            int subactivityCount = 1;
            int testCount = 1;
            int subTestCount = 1;
            GraphObject currentTest = null;
            var doc = new HtmlDocument();
            doc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.HTMLPage1.html"));
            var nodes = doc.DocumentNode.Descendants("a").ToList();
            foreach (var a in nodes)
            {
                var label = a.ParentNode.InnerText;
                bool matches = false;
                switch (label)
                {
                    case "Activities":
                        sb.AppendLine($"Label: {label}");
                        if (a.Attributes["href"] != null)
                        {
                            var url = a.Attributes["href"].Value;
                            if (url.StartsWith("/resources/"))
                            {//its one we want
                                sb.AppendLine($"URL: {url}");
                                var web = new HtmlWeb();
                                var subDoc = web.Load("https://www.ncetm.org.uk" + url);
                                //look for <h2> Activity ...
                                var hnodes = subDoc.DocumentNode.SelectNodes("//h2");
                                var startAct = hnodes.Where(a => a.InnerText != null && a.InnerText.Trim().StartsWith("Activity") || a.InnerText.Trim().StartsWith("Activities")).FirstOrDefault();
                                if (startAct == null)
                                    continue;
                                var wrapper = startAct.ParentNode;
                                //consists of an h2 header with activity name and one or more <p> sections containing the activity text.
                                bool activityFound = false;
                                foreach (var child in wrapper.ChildNodes)
                                {
                                    if (child.Name == "h2")
                                    {
                                        if (child.InnerText.Trim().StartsWith("Activity"))
                                        {
                                            var activityText = HttpUtility.HtmlDecode(child.InnerText.Trim());
                                            sb.AppendLine($"Activity: {activityText}");
                                            //add an activity object
                                            var oldActivity = currentActivity;
                                            currentActivity = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = activityText, lineage = activityLineage, externalId = $"ACTIVITY{activityCount++}" }, OntologyAction.build);
                                            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentYear.id, endId = currentActivity.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                            if (oldActivity != null)
                                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentActivity.id, endId = oldActivity.id, lineage = followsLineage, name = "follows", weight = 1.0 }, OntologyAction.build);
                                            activityFound = true;
                                        }
                                        else
                                            break;
                                    }
                                    else if (child.Name == "p" && activityFound)
                                    {
                                        if (child.ChildNodes != null)
                                        {
                                            foreach (var c in child.ChildNodes)
                                            {
                                                if (c.Name == "img")
                                                {
                                                    var subactivityText = $"![{c.Attributes["alt"].Value}]({c.Attributes["src"].Value})";
                                                    sb.AppendLine($"Activity image: {subactivityText}");
                                                    //add a sub-activity object
                                                    var currentSubActivity = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = subactivityText, lineage = activityLineage, externalId = $"SUBACTIVITY{subactivityCount++}" }, OntologyAction.build);
                                                    await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentActivity.id, endId = currentSubActivity.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                                        {
                                            var subactivityText = HttpUtility.HtmlDecode(child.InnerText.Trim());
                                            sb.AppendLine($"Activity text: {subactivityText}");
                                            var currentSubActivity = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = subactivityText, lineage = activityLineage, externalId = $"SUBACTIVITY{subactivityCount++}" }, OntologyAction.build);
                                            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentActivity.id, endId = currentSubActivity.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "Exemplification":
                        sb.AppendLine($"Label: {label}");
                        if (a.Attributes["href"] != null)
                        {
                            var url = a.Attributes["href"].Value;
                            if (url.StartsWith("/resources/"))
                            {//its one we want
                                sb.AppendLine($"URL: {url}");
                                var web = new HtmlWeb();
                                var subDoc = web.Load("https://www.ncetm.org.uk" + url);
                                //look for span with text "Exemplification"
                                var hnodes = subDoc.DocumentNode.SelectNodes("//div");
                                var headers = hnodes.Where(a => a.Attributes.Contains("class") && a.Attributes["class"].Value == "ExemplificationStatements").ToList();
                                for (int n = 0; n < headers.Count; n++)
                                {
                                    var testText = HttpUtility.HtmlDecode(headers[n].InnerText.Trim());
                                    sb.AppendLine($"Exemplification: {testText}");
                                    //add a test object
                                    //add an activity object
                                    var oldTest = currentTest;
                                    currentTest = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = testText, lineage = testLineage, externalId = $"TEST{testCount++}" }, OntologyAction.build);
                                    await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentYear.id, endId = currentTest.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                    if (oldTest != null)
                                        await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentTest.id, endId = oldTest.id, lineage = followsLineage, name = "follows", weight = 1.0 }, OntologyAction.build);

                                    var x = headers[n].NextSibling;
                                    while (x != null && n + 1 < headers.Count && x != headers[n + 1])
                                    {
                                        if (x.Name == "ul")
                                        {
                                            foreach (var c in x.ChildNodes)
                                            {
                                                if (c.Name == "li")
                                                {
                                                    if (!string.IsNullOrEmpty(c.InnerText.Trim()))
                                                        sb.AppendLine($"Exemplification text: {HttpUtility.HtmlDecode(c.InnerText.Trim())}");
                                                }
                                            }
                                        }
                                        else if (x.Name == "p")
                                        {
                                            if (x.ChildNodes != null)
                                            {
                                                foreach (var c in x.ChildNodes)
                                                {
                                                    if (c.Name == "img")
                                                    {
                                                        var subTestText = $"![{c.Attributes["alt"].Value}]({c.Attributes["src"].Value})";
                                                        sb.AppendLine($"Test image: {subTestText}");
                                                        //add a sub-Test object
                                                        var currentSubTest = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = subTestText, lineage = testLineage, externalId = $"SUBTEST{subTestCount++}" }, OntologyAction.build);
                                                        await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentTest.id, endId = currentSubTest.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(x.InnerText.Trim()))
                                            {
                                                var subTestText = HttpUtility.HtmlDecode(x.InnerText.Trim());
                                                sb.AppendLine($"Exemplification text: {subTestText}");
                                                var currentSubTest = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = subTestText, lineage = testLineage, externalId = $"SUBTEST{subTestCount++}" }, OntologyAction.build);
                                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentTest.id, endId = currentSubTest.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                            }
                                        }
                                        x = x.NextSibling;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if (label.StartsWith("Y"))//it's a topic heading
                        {
                            var splits = label.Split(':');
                            var year = splits[0];
                            var topic = splits[1].Trim();
                            if (!topics.Contains(topic))
                            {
                                currentTopic = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = topic, lineage = mathsLineage, externalId = $"TOPIC{topics.Count + 1}" }, OntologyAction.build);
                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = toplevel.id, endId = currentTopic.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                                topics.Add(topic);
                                currentYear = null;
                            }
                            var oldYear = currentYear;
                            currentYear = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = year, lineage = yearLineage, externalId = year }, OntologyAction.build);
                            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentTopic.id, endId = currentYear.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                            if (oldYear != null)//set precedence
                            {
                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentYear.id, endId = oldYear.id, lineage = followsLineage, name = "follows", weight = 1.0 }, OntologyAction.build);
                            }
                            currentActivity = null;
                            currentTest = null;
                            sb.AppendLine($"Topic: {topic}");
                            sb.AppendLine($"Year: {year}");
                        }
                        break;
                }
                if (a.Attributes["href"] != null)
                {
                    var url = a.Attributes["href"].Value;
                    if (url.StartsWith("/resources/") && matches)
                    {//its one we want
                        sb.AppendLine($"URL: {url}");
                        var web = new HtmlWeb();
                        var subDoc = web.Load("https://www.ncetm.org.uk" + url);
                    }
                }
            }
            File.WriteAllText("scrape_1st_pass.txt", sb.ToString());
            var topicCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{consistsLineage}\",\"{yearLineage}\") and all(\"{followsLineage}\",\"{yearLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, mathsLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = topicCode, lineage = completeLineage });
            var yearCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{consistsLineage}\",\"{activityLineage}\") and all(\"{followsLineage}\",\"{activityLineage}\") and all(\"{consistsLineage}\",\"{testLineage}\") and all(\"{followsLineage}\",\"{testLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, yearLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = yearCode, lineage = completeLineage });
            var activityCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{consistsLineage}\",\"{activityLineage}\") and all(\"{followsLineage}\",\"{activityLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, activityLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = activityCode, lineage = completeLineage });
            var testCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{consistsLineage}\",\"{testLineage}\") and all(\"{followsLineage}\",\"{testLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, testLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = testCode, lineage = completeLineage });

            await _graph.Store(compositeName);
            var stream = await _graph.StoreGraphML(compositeName);
            using (var fileStream = File.Create(graphImage))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task Dedupe()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _primitives.Load(compositeName) as BlobGraphContent;
            var roots = model.vertices.Values.Where(a => a.externalId == "MATH1").ToList();
            var topics = model.vertices.Values.Where(a => a.externalId.StartsWith("TOPIC")).ToList();
            await _graph.DeleteGraphObject(compositeName, roots[1].id);
            await _graph.DeleteGraphObject(compositeName, roots[2].id);
            bool completed = false;
            while (!completed)
            {
                var removeList = new List<GraphObject>();
                foreach (var v in model.vertices.Values)
                {
                    if (v.In.Count == 0 && v.externalId != "MATH1")
                    {
                        removeList.Add(v);
                    }
                }
                if (!removeList.Any())
                {
                    completed = true;
                }
                else
                {
                    foreach (var v in removeList)
                    {
                        await _graph.DeleteGraphObject(compositeName, v.id);
                    }
                }
            }
            await _graph.Store(compositeName);
            var stream = await _graph.StoreGraphML(compositeName);
            using (var fileStream = File.Create(graphImage))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateRules()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var topicCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{yearLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{yearLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, mathsLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = topicCode, lineage = completeLineage });
            var yearCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{activityLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{activityLineage}\",\"{followsLineage}\",\"{completeLineage}\") and all(\"{testLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{testLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, yearLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = yearCode, lineage = completeLineage });
            var activityCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{subactivityLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{subactivityLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, activityLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = activityCode, lineage = completeLineage });
            var testCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{questionLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{questionLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.CreateVirtualAttribute(compositeName, testLineage, new GraphAttributeInput { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = testCode, lineage = completeLineage });
            await _graph.Store(compositeName);
        }

        [TestMethod]
        [Ignore]
        public async Task UpdatelineageTypes()
        {
            //sub-activity and subtest have been given lineages of activity and test, thus rules fire when they shouldn't. replace
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _primitives.Load(compositeName) as BlobGraphContent;
            foreach (var s in model.vertices.Values)
            {
                if (s.externalId.StartsWith("SUBACTIVITY"))
                {
                    await _graph.UpdateGraphObject(compositeName, new GraphObjectUpdate { id = s.id, lineage = subactivityLineage }, OntologyAction.build);
                }
                if (s.externalId.StartsWith("SUBTEST"))
                {
                    await _graph.UpdateGraphObject(compositeName, new GraphObjectUpdate { id = s.id, lineage = questionLineage }, OntologyAction.build);
                }
            }
            var node = await _primitives.GetGraphObjectById(compositeName, "1b35bb45-930a-4331-8421-d1c95f7a0bf7");
            var gh = new GraphHandler(_graph);
            var paths = new List<string> { consistsLineage, followsLineage };
            var subjectId = Guid.NewGuid().ToString();
            var userId = _config["userId"];
            var targetId = node.id;
            var next = await gh.GraphPass(userId, graphName, subjectId, targetId, paths, compositeName, new List<DarlCommon.DarlVar>());
            await _graph.Store(compositeName);
            var stream = await _graph.StoreGraphML(compositeName);
            using (var fileStream = File.Create(graphImage))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }


        [TestMethod]
        public async Task TestInference()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _primitives.Load(compositeName);
            var node = await _primitives.GetGraphObjectById(compositeName, "1b35bb45-930a-4331-8421-d1c95f7a0bf7");
            var ks = new KnowledgeState { Id = Guid.NewGuid().ToString(), subjectId = Guid.NewGuid().ToString(), userId = _config["userId"], knowledgeGraphName = graphName, data = new Dictionary<string, List<GraphAttribute>>() };
            var paths = new List<string> { consistsLineage, followsLineage };
            var order = _graph.GetExecutionOrder(model, node, paths);
            var nodes = await _graph.FindNext(model, order, ks, node, paths, completeLineage);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual("SUBACTIVITY1", nodes[0].externalId);
            ks.data.Add(nodes[0].id, new List<GraphAttribute> { new GraphAttribute { lineage = completeLineage } });
            nodes = await _graph.FindNext(model, order, ks, node, new List<string> { consistsLineage, followsLineage }, completeLineage);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual("SUBACTIVITY2", nodes[0].externalId);
            ks.data.Add(nodes[0].id, new List<GraphAttribute> { new GraphAttribute { lineage = completeLineage } });
            nodes = await _graph.FindNext(model, order, ks, node, new List<string> { consistsLineage, followsLineage }, completeLineage);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual("SUBACTIVITY3", nodes[0].externalId);
            var gh = new GraphHandler(_graph);
            var subjectId = Guid.NewGuid().ToString();
            var userId = _config["userId"];
            var targetId = node.id;
            var next = await gh.GraphPass(userId, graphName, subjectId, targetId, paths, compositeName, new List<DarlCommon.DarlVar>());
        }

        [TestMethod]
        public async Task TestGraphPass()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var gh = new GraphHandler(_graph);
            var node = await _primitives.GetGraphObjectById(compositeName, "1b35bb45-930a-4331-8421-d1c95f7a0bf7");
            var paths = new List<string> { consistsLineage, followsLineage };
            var subjectId = Guid.NewGuid().ToString();
            var userId = _config["userId"];
            var targetId = node.id;
            var complete = false;
            var next = await gh.GraphPass(userId, graphName, subjectId, targetId, paths, compositeName, new List<DarlCommon.DarlVar>());
            var count = 0;
            while (!complete)
            {
                var text = next.First().response.Value;
                Debug.WriteLine(text);
                if (text == "This process is complete.")
                    complete = true;
                else
                {
                    var current = next.Last();
                    current.response.Value = current.response.categories.Keys.First();
                    next = await gh.GraphPass(userId, graphName, subjectId, targetId, paths, compositeName, new List<DarlCommon.DarlVar> { current.response });
                }
                count++;
            }
            Assert.AreEqual(85, count);
        }

        [TestMethod]
        [Ignore]
        public async Task TestAddRecognition()
        {
            //Add basics, like Hello, help, default and then maths.
            var compositeName = $"{_config["userId"]}_{graphName}";
            var root = await _graph.CreateRecognitionRoot(compositeName, "default:");
            var helloRule = "output textual response;\nif anything then response will be randomtext(\"hello, can I help ? \", \"hi, what can I do for you ? \");";
            var hello = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "noun:01,4,05,11,03", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = helloRule } } });
            var defaultRule = "output textual response;\nif anything then response will be \"I don't know the answer to that.\";";
            var defaultAnswer = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "default:", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = defaultRule } } });
            var helpRule = "output textual response;\nif anything then response will be \"This is a simple initial demonstration of maths teaching functionality. Try typing 'maths'\";";
            var help = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "verb:397,2", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = helpRule } } });
            var mathRule = "output textual response;\nif anything then response will be \"Maths functionality goes here\";";
            var math = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "noun:01,0,0,15,21,0,08,02", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = mathRule } } });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = hello.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = defaultAnswer.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = help.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = math.id, lineage = followsLineage });
            var gh = new GraphHandler(_graph);
            var userId = _config["userId"];
            var subjectId = "default:";
            var results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "hello" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(helloRule, results[0].darl);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "help" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(helpRule, results[0].darl);
            Assert.AreEqual("This is a simple initial demonstration of maths teaching functionality. Try typing 'maths'", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "froopies" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(defaultRule, results[0].darl);
            Assert.AreEqual("I don't know the answer to that.", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "arithmetic" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(mathRule, results[0].darl);
            Assert.AreEqual("Maths functionality goes here", results[0].response.Value);
            var nodeId = "1b35bb45-930a-4331-8421-d1c95f7a0bf7";
            mathRule = $"output network completed \"{nodeId}\" \"{completeLineage}\";\n if anything then completed will be seek(\"{followsLineage}\", \"{consistsLineage}\");";
            await _graph.UpdateRecognitionObject(compositeName, new GraphObjectUpdate { id = math.id, properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = mathRule } } });
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "arithmetic" });

        }

        [TestMethod]
        [Ignore]
        public async Task AddRecognitionToKG()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var root = await _graph.CreateRecognitionRoot(compositeName, "default:");
            var helloRule = "output textual response;\nif anything then response will be randomtext(\"hello, can I help ? \", \"hi, what can I do for you ? \");";
            var hello = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "noun:01,4,05,11,03", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = helloRule } } });
            var defaultRule = "output textual response;\nif anything then response will be \"I don't know the answer to that.\";";
            var defaultAnswer = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "default:", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = defaultRule } } });
            var helpRule = "output textual response;\nif anything then response will be \"This is a simple initial demonstration of maths teaching functionality. Try typing 'maths'\";";
            var help = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "verb:397,2", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = helpRule } } });
            var nodeId = "1b35bb45-930a-4331-8421-d1c95f7a0bf7";
            var mathRule = $"output network completed \"{nodeId}\" \"{completeLineage}\";\n if anything then completed will be seek(\"{followsLineage}\", \"{consistsLineage}\");";
            var math = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "noun:01,0,0,15,21,0,08,02", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = mathRule } } });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = hello.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = defaultAnswer.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = help.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = root.id, endId = math.id, lineage = followsLineage });
            var navRoot = await _graph.CreateRecognitionRoot(compositeName, "navigation:"); //create a navigation tree
            var navHelpRule = "output textual response;\nif anything then response will be \"You can stop anytime by typing 'quit'.\";";
            var navHelp = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "verb:397,2", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = navHelpRule } } });
            var navQuitRule = "output categorical terminate {\"true\",\"false\"};\nif anything then terminate will be true;";
            var navQuit = await _graph.CreateRecognitionObject(compositeName, new GraphObjectInput { lineage = "verb:060", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = navQuitRule } } });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = navRoot.id, endId = navHelp.id, lineage = followsLineage });
            await _graph.CreateRecognitionConnection(compositeName, new GraphConnectionInput { startId = navRoot.id, endId = navQuit.id, lineage = followsLineage });
            await _graph.Store(compositeName);
        }

        [TestMethod]
        public async Task BotProcessingTest()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var userId = _config["userId"];
            var gh = new GraphHandler(_graph);
            var bp = new BotProcessing(_conn, _form, _rf, _trigger, _bplogger, _config, _context, _graph, gh);
            var conversationId = Guid.NewGuid().ToString();
            var res = await bp.InteractKGAsync(userId, graphName, conversationId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "hello" });
            Assert.AreEqual(1, res.Count);
            res = await bp.InteractKGAsync(userId, graphName, conversationId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "poops" });
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("I don't know the answer to that.", res[0].response.Value);
            res = await bp.InteractKGAsync(userId, graphName, conversationId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "I want to learn arithmetic" });
        }

        [TestMethod]
        public async Task AdaptTexts()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            //update virtual object rules
            var activityCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{subactivityLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{subactivityLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.UpdateVirtualObject(compositeName, new GraphObjectUpdate { lineage = activityLineage, properties = new List<GraphAttribute> { new GraphAttribute { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = activityCode, lineage = completeLineage } } });
            var testCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{questionLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{questionLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            await _graph.UpdateVirtualObject(compositeName, new GraphObjectUpdate { lineage = testLineage, properties = new List<GraphAttribute> { new GraphAttribute { confidence = 1.0, name = "completed", type = GraphAttribute.DataType.categorical, value = testCode, lineage = completeLineage } } });
            var navQuitRule = "output categorical terminate {\"true\",\"false\"};\nif anything then terminate will be true;";
            var robj = await _graph.FindRecognition(compositeName, "navigation:", "verb:060");
            await _graph.UpdateRecognitionObject(compositeName, new GraphObjectUpdate { id = robj.id, properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = navQuitRule } } });
            await _graph.Store(compositeName);




            var userId = _config["userId"];
            var activities = await _graph.GetGraphObjectsByLineage(compositeName, subactivityLineage);
            foreach(var o in activities)
            {
                var name = o.name;
                o.properties = new List<GraphAttribute>();
                o.properties.Add(new GraphAttribute { lineage = displayLineage, value = name, inferred = false, name = "display" });
            }
        }

        [TestMethod]
        public async Task TestDisplayGraphMethods()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var dmodel = await _graph.GetRealDisplayGraph(compositeName, "");
            Assert.AreEqual(1393, dmodel.nodes.Count);
            Assert.AreEqual(1775, dmodel.edges.Count);
            dmodel = await _graph.GetRealDisplayGraph(compositeName, questionLineage);
            Assert.AreEqual(361, dmodel.nodes.Count);
            Assert.AreEqual(0, dmodel.edges.Count);
            dmodel = await _graph.GetVirtualDisplayGraph(compositeName);
            Assert.AreEqual(25, dmodel.nodes.Count);
            Assert.AreEqual(36, dmodel.edges.Count);
            dmodel = await _graph.GetRecognitionDisplayGraph(compositeName, "default:");
            Assert.AreEqual(5, dmodel.nodes.Count);
            Assert.AreEqual(4, dmodel.edges.Count);
            dmodel = await _graph.GetRecognitionDisplayGraph(compositeName, "navigation:");
            Assert.AreEqual(3, dmodel.nodes.Count);
            Assert.AreEqual(2, dmodel.edges.Count);
            dmodel = await _graph.GetRecognitionDisplayGraph(compositeName, "poops:");
            Assert.AreEqual(0, dmodel.nodes.Count);
            Assert.AreEqual(0, dmodel.edges.Count);
        }



    }

}
    

