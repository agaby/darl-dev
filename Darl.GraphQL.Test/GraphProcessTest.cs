using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class GraphProcessTest
    {
        private GraphProcessing _graph;
        private IConfiguration _config;

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();

            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinHostname")]).Returns("thinkbase.gremlin.cosmosdb.azure.com");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinPort")]).Returns("443");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinAuthKey")]).Returns("ffWKZWMJro4JHBaJAi4yG1o35ujaDvj0pIkrqsYEz4hCoHR9jvHr6YR3Pb2dxr8rw4obuO4ZvnJetejwJyrYQA==");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinDatabase")]).Returns("farleft");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("hypernymy");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            var logger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            _graph = new GraphProcessing(configuration.Object, logger.Object, context.Object);
        }

        [TestMethod]
        public async Task CreateAndDeleteObjectTest()
        {
            var res = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = "noun:00,2,00", name = "Andrew Edmonds", firstname = "Andrew", secondname = "Edmonds", inferred = false, existence = new List<DateTime> { new DateTime(1955, 11, 6), DateTime.MaxValue } });
            Assert.AreEqual(res.inferred, false);
            Assert.AreEqual(res.lineage, "noun:00,2,00");
            Assert.AreEqual(res.name, "andrew edmonds");
            Assert.AreEqual(res.firstname, "andrew");
            Assert.AreEqual(res.secondname, "edmonds");
            var objlist = await _graph.GetGraphObjects(_config["userId"], "andrew edmonds", "noun:00,2,00");
            Assert.AreEqual(objlist.Count, 1);
            Assert.AreEqual(res.id, objlist[0].id);
            var obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            Assert.AreEqual(res.id, obj.id);
            Assert.AreEqual(res.name, obj.name);
            Assert.AreEqual(res.existence.Count, 2);
            Assert.AreEqual(res.existence[0], new DateTime(1955, 11, 6));
            Assert.AreEqual(res.existence[1], DateTime.MaxValue);
            //add a property
            var up = await _graph.UpdateGraphObject(_config["userId"], new GraphObjectUpdate { id = res.id, lineage = "noun:00,2,00", properties = new List<StringStringPair> { new StringStringPair("noun:01,4,09,01,3,4,5", "Andy is a bit of a dork, really.") } }, true);
            Assert.AreEqual(1, up.properties.Count);
            //add another 
            var res2 = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = "noun:00,2,00", name = "Anneke Edmonds", firstname = "Anneke", secondname = "Edmonds", inferred = false, existence = new List<DateTime> { new DateTime(1961, 8, 27), DateTime.MaxValue } });
            var obj2 = await _graph.GetGraphObjectById(_config["userId"], res2.id);
            Assert.AreEqual(res2.id, obj2.id);
            //add a marry link
            var conn1 = await _graph.CreateGraphConnection(_config["userId"], new GraphConnectionInput { lineage = "verb:197,6,07", existence = new List<DateTime> { new DateTime(1988, 8, 27), DateTime.MaxValue }, name = "married", startId = res.id, endId = res2.id }, true);
            Assert.AreEqual(conn1.startId, res.id);
            Assert.AreEqual(conn1.endId, res2.id);
            Assert.AreEqual(conn1.lineage, "verb:197,6,07");
            Assert.AreEqual(conn1.existence.Count, 2);
            //look for links in objects,
            obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            obj2 = await _graph.GetGraphObjectById(_config["userId"], res2.id);

            //Update conn1
            var del = await _graph.DeleteGraphObject(_config["userId"], res.id);
            var del2 = await _graph.DeleteGraphObject(_config["userId"], res2.id);
            obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            Assert.IsNull(obj);
        }

        [TestMethod]
        public async Task TestRead1()
        {
            var res = await _graph.ReadAsync(new List<string> { "text", "jeremy corbyn", "noun:00,2,00" });
            Assert.IsTrue(res.stringConstant.Length > 50);
        }

        [TestMethod]
        public async Task TestRead2()
        {

            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbyn", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(10, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead3()
        {
            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbyn", "noun:00,2,00", "noun:01,2,07,10" });
            Assert.AreEqual(15, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead4()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremy Corbyn", "Paul Mason", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead5()
        {
            var res = await _graph.ReadAsync(new List<string> { "text", "jeremy corbin", "noun:00,2,00" });
            Assert.IsTrue(res.stringConstant.Length > 50);
        }

        [TestMethod]
        public async Task TestRead6()
        {

            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbin", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(10, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead7()
        {
            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremi Corbyn", "noun:00,2,00", "noun:01,2,07,10" });
            Assert.AreEqual(15, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead8()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremy Corbin", "Paul Masoni", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        public async Task TestRead9()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremi Corbyn", "Pail Mason", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }


        [TestMethod]
        public async Task NearestVertexTest()
        {
            using (var gremlinClient = new GremlinClient(_graph.gremlinDreamerServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await _graph.FindNearestNameVertex(gremlinClient, "noun:00,2,00", "Corbyn", 0.7f, "Jeremy");
                Assert.AreEqual(1, res.Count);
                res = await _graph.FindNearestNameVertex(gremlinClient, "noun:00,2,00", "Corbin", 0.7f, "Jeremy");
                Assert.AreEqual(1, res.Count);
                Assert.AreEqual(res[0].name, "jeremy corbyn");
            }
        }

        [TestMethod]
        public async Task TestCreateGraph()
        {
            await _graph.CreateNewGraph("33db770b-29e9-46ae-8a19-c1947bd775d8");
        }

    }
}
