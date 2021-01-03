using Darl.GraphQL.Models.Connectivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class CreateDefaultRulesforKG
    {
        CosmosDBConnectivity cosmos;

        [TestInitialize()]
        public void Initialize()
        {
            var logger = new Mock<ILogger<CosmosDBConnectivity>>();
            var config = new Mock<IConfiguration>();
            var licensing = new Mock<ILicensing>();
            var cache = new Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            config.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            config.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            cosmos = new CosmosDBConnectivity(config.Object, logger.Object, licensing.Object, cache.Object);
        }

        [TestMethod]
        public async Task TestModelDownload()
        {
            var displayLineage = "noun:00,1,00,3,10,09,06";
            var display = 
                "//Prototype for display\n" +
                "//Change the data type of response to suit your data value and add any constraints.\n" +
                "input numeric response;\n" +
                "output textual annotation;\n" +
                "output categorical completed {true,false} \"adjective:5500\";\n" +
                "//if you want to mark the response add the following\n" +
                "//output categorical correct{true,false} \"adjective:3521\";\n" +
                "//Make output answer match your response data type.\n" +
                "//output numeric answer \"noun:01,4,05,21,19\";\n" +
                "//if response is = attribute(\"noun:01,4,05,21,19\") then correct will be true;\n" +
                "//if anything then answer will be response;\n" +
                "if response is present then completed will be true;\n" +
                "if anything then annotation will be attribute(\"noun:00,1,00,3,10,09,06\");\n";
            await cosmos.DeleteDefault($"default_rule_{displayLineage}");
            await cosmos.CreateDefault($"default_rule_{displayLineage}", display);

            var complete = "output categorical completed {true, false} \"adjective: 5500\";\n" +
                "//Fill in the children who have to be complete before this node is complete\n" +
                "//by adding the node, connection and attribute lineages.\n" +
                "//The latter will normally be \"adjective: 5500\".\n" +
                "//If there are different kinds check them individually and 'and' the results.\n" +
                "if all(\"node lineage\", \"connection lineage\", \"attribute lineage\") then completed will be true;\n";
            var completeLineage = "adjective:5500";
            await cosmos.DeleteDefault($"default_rule_{completeLineage}");
            await cosmos.CreateDefault($"default_rule_{completeLineage}", complete);
        }
    }
}
