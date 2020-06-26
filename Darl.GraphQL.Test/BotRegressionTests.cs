using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
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
    public class BotRegressionTests
    {

        private ILocalStore _graph;
        private IConfiguration _config;
        private IBotProcessing _bot;
        private IConnectivity _conv;
        private IRuleFormInterface _rform;



        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinHostname")]).Returns("thinkbase.gremlin.cosmosdb.azure.com");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinPort")]).Returns("443");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinAuthKey")]).Returns("ffWKZWMJro4JHBaJAi4yG1o35ujaDvj0pIkrqsYEz4hCoHR9jvHr6YR3Pb2dxr8rw4obuO4ZvnJetejwJyrYQA==");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinDatabase")]).Returns("farleft");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("hypernymy");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("7d1a254f-d405-4385-acbc-308c8376f2e3");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("d70f1008-5758-41b5-9c44-bc90535aeabc");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("786e46c2-fa33-4124-af67-1bb14625c216");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("azure");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "botmodel")]).Returns("thousandquestions.model");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:boaiuserid")]).Returns("8c663676-a7dc-4561-af3d-89b38555837d");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");




            _config = configuration.Object;
            var logger = new Mock<ILogger<GraphProcessing>>();
            var formLogger = new Mock<ILogger<FormApi>>();
            var botLogger = new Mock<ILogger<BotProcessing>>();
            var connLogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            var licensing = new Mock<ILicensing>();
            _conv = new CosmosDBConnectivity(_config, connLogger.Object, licensing.Object);
            var trigger = new Mock<ITrigger>();
            var cache = new Mock<IDistributedCache>();
            var formApi = new FormApi(cache.Object, trigger.Object, formLogger.Object, _graph);
            _graph = new GraphProcessing(configuration.Object, logger.Object, context.Object);
            _rform = new RuleFormInterface(_conv);
            _bot = new BotProcessing(_conv, formApi, _rform, trigger.Object, botLogger.Object, _config, context.Object);
        }

        [TestMethod]
        public async Task TestBasicInteractions()
        {
            var conversationId = Guid.NewGuid().ToString();
            //simplest interaction
            var resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar {dataType = DarlVar.DataType.textual, Value = "Hi", name = "" });
            Assert.IsTrue(resp[0].response.Value == "hi, what can I do for you?" || resp[0].response.Value == "hello, can I help?");
            //value storage
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.textual, Value = "my name is andy", name = "" });
            Assert.AreEqual("thanks for that", resp[0].response.Value);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.textual, Value = "what is my name", name = "" });
            Assert.AreEqual("andy", resp[0].response.Value);
            //calling rulesets
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.textual, Value = "run a ruleset", name = "" });
            Assert.AreEqual("The following are demonstration rule sets you can run.", resp[0].response.Value);
            Assert.AreEqual("Choose a rule set to run", resp[1].response.Value);
            Assert.AreEqual(2, resp[1].response.categories.Count);
            //ruleset calling a ruleset
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.textual, Value = "UK Tax and NI", name = "" });
            Assert.AreEqual("This rule set is a UK Tax and NI calculator. It takes a yearly salary and calculates taxes, including employer's payments.", resp[0].response.Value);
            Assert.AreEqual("What is your income per year?", resp[1].response.Value);
            Assert.AreEqual(DarlVar.DataType.numeric, resp[1].response.dataType);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.numeric, Value = "50000", name = "" });
            Assert.AreEqual("Any dividend income? (Give the value after corporation taxes)", resp[0].response.Value);
            Assert.AreEqual(DarlVar.DataType.numeric, resp[0].response.dataType);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.numeric, Value = "30000", name = "" });
            Assert.AreEqual("Your age in years", resp[0].response.Value);
            Assert.AreEqual(DarlVar.DataType.numeric, resp[0].response.dataType);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.numeric, Value = "55", name = "" });
            Assert.AreEqual("Are you registered as blind?", resp[0].response.Value);
            Assert.AreEqual(DarlVar.DataType.categorical, resp[0].response.dataType);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.categorical, Value = "True", name = "" });
            Assert.AreEqual("Calculated results", resp[0].response.Value);
            Assert.AreEqual("Total taxes, per year 16543.80", resp[1].response.Value);
            Assert.AreEqual("Total taxes, per month 1378.65", resp[2].response.Value);
            Assert.AreEqual("National Insurance, per year 4337.32", resp[3].response.Value);
            Assert.AreEqual("National Insurance, per month 361.44", resp[4].response.Value);
            Assert.AreEqual("Your take home pay, per month 5649.46", resp[5].response.Value);
            Assert.AreEqual("Employers' NI, per year 5866.66", resp[6].response.Value);
            Assert.AreEqual("Percentage of the cost of your employment paid in tax 25.12", resp[7].response.Value);
            resp = await _bot.InteractAsync(_config["userId"], _config["botmodel"], conversationId, new DarlVar { dataType = DarlVar.DataType.textual, Value = "Hi", name = "" });
            Assert.IsTrue(resp[0].response.Value == "hi, what can I do for you?" || resp[0].response.Value == "hello, can I help?");
        }


    }
}
