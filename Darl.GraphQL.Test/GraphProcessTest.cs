using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
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
            var telemetry = new TelemetryClient( new TelemetryConfiguration
                {
                    TelemetryChannel = new Mock<ITelemetryChannel>().Object
                }) ;
            _config = configuration.Object;
            _graph = new GraphProcessing(configuration.Object, telemetry);
        }

        [TestMethod]
        public  async Task CreateAndDeleteObjectTest()
        {
            var res = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = "noun:00,2,00", name = "Andrew Edmonds", firstname = "Andrew", secondname = "Edmonds", inferred  = false, existence = new List<DateTime> {new DateTime(1955,11,6), DateTime.MaxValue } });
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
            var del = await _graph.DeleteGraphObject(_config["userId"], res.id);
            obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            Assert.IsNull(obj);

        }
    }
}
