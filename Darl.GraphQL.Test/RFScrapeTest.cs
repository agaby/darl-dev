using Darl.GraphQL.Models.Connectivity;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
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
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");

            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            _primitives = new BlobGraphPrimitives(blob, cache.Object);
            _graph = new GraphProcessing(_primitives);
            _graphStore = new GraphLocalStore(_config, logger.Object, context.Object, _graph);
        }


        [TestMethod]
        public async Task TestScrapeMathsSite()
        {
            var compositeName = _config["userId"] + "_rf1.graph";
            var sb = new StringBuilder();
            var topics = new HashSet<string>();
            //emit top level node "maths"
            var toplevel = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = "maths", lineage = mathsLineage, externalId = "MATH1" }, OntologyAction.build);
            GraphObject currentTopic = null;
            GraphObject currentYear = null;
            var doc = new HtmlDocument();
            doc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.HTMLPage1.html"));
            var nodes = doc.DocumentNode.Descendants("a").ToList();
            foreach(var a in nodes)
            {
                var label = a.ParentNode.InnerText;
                bool matches = false;
                switch(label)
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
                                foreach(var child in wrapper.ChildNodes)
                                {
                                    if (child.Name == "h2")
                                    {
                                        if (child.InnerText.Trim().StartsWith("Activity"))
                                        {
                                            sb.AppendLine($"Activity: {HttpUtility.HtmlDecode(child.InnerText.Trim())}");
                                            //add an activity object
                                            activityFound = true;
                                        }
                                        else
                                            break;
                                    }
                                    else if (child.Name == "p" && activityFound)
                                    {
                                        if(child.ChildNodes != null )
                                        {
                                            foreach(var c in child.ChildNodes)
                                            {
                                                if(c.Name == "img")
                                                {
                                                    sb.AppendLine($"Activity image: ![{c.Attributes["alt"].Value}]({c.Attributes["src"].Value})");
                                                    //add a sub-activity object
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                                            sb.AppendLine($"Activity text: {HttpUtility.HtmlDecode(child.InnerText.Trim())}");
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
                                for(int n = 0; n < headers.Count; n++)
                                {
                                    sb.AppendLine($"Exemplification: {HttpUtility.HtmlDecode(headers[n].InnerText.Trim())}");
                                    //add a test object
                                    var x = headers[n].NextSibling;
                                    while( x != null && n + 1 < headers.Count && x != headers[n+1])
                                    {
                                        if(x.Name == "ul")
                                        {
                                            foreach(var c in x.ChildNodes)
                                            {
                                                if(c.Name == "li")
                                                {
                                                    if (!string.IsNullOrEmpty(c.InnerText.Trim()))
                                                        sb.AppendLine($"Exemplification text: {HttpUtility.HtmlDecode(c.InnerText.Trim())}");
                                                }
                                            }
                                        }
                                        else if(x.Name == "p")
                                        {
                                            if (!string.IsNullOrEmpty(x.InnerText.Trim()))
                                                sb.AppendLine($"Exemplification text: {HttpUtility.HtmlDecode(x.InnerText.Trim())}");
                                        }
                                        x = x.NextSibling;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if(label.StartsWith("Y"))//it's a topic heading
                        {
                            var splits = label.Split(':');
                            var year = splits[0];
                            var topic = splits[1].Trim();
                            if (!topics.Contains(topic))
                            {
                                currentTopic = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = topic, lineage = mathsLineage, externalId = $"TOPIC{topics.Count + 1}" }, OntologyAction.build);
                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = toplevel.id, endId = currentTopic.id, lineage = consistsLineage, name = "consists of", weight = 1.0}, OntologyAction.build);
                                topics.Add(topic);
                                currentYear = null;
                            }
                            var oldYear = currentYear != null ? currentYear : null;
                            currentYear = await _graph.CreateGraphObject(compositeName, new GraphObjectInput { name = year, lineage = yearLineage, externalId = year }, OntologyAction.build);
                            await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentTopic.id, endId = currentYear.id, lineage = consistsLineage, name = "consists of", weight = 1.0 }, OntologyAction.build);
                            if(oldYear != null)//set precedence
                            {
                                await _graph.CreateGraphConnection(compositeName, new GraphConnectionInput { startId = currentYear.id, endId = oldYear.id, lineage = followsLineage, name = "follows", weight = 1.0 }, OntologyAction.build);
                            }
                            sb.AppendLine($"Topic: {topic}");
                            sb.AppendLine($"Year: {year}");
                        }
                        break;
                }
                if (a.Attributes["href"] != null)
                {
                    var url = a.Attributes["href"].Value;
                    if(url.StartsWith("/resources/") && matches)
                    {//its one we want
                        sb.AppendLine($"URL: {url}");
                        var web = new HtmlWeb();
                        var subDoc = web.Load("https://www.ncetm.org.uk" + url);
                    }
                }
            }
            File.WriteAllText("scrape_1st_pass.txt", sb.ToString());
        }
    }
}
