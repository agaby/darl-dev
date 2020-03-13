using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class DownloadTests
    {
        string userId = "5ee43551-c05c-4cff-8582-c08f23f84c14";
        string modelName = "far_left.model";


        [TestInitialize()]
        public void Initialize()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.AppSettings.json"));
            var source = reader.ReadToEnd();
        }

        [TestMethod]
        public async Task TestModelDownload()
        {
            var logger = new Mock<ILogger<CosmosDBConnectivity>>();
            var config = new Mock<IConfiguration>();
            var licensing = new Mock<ILicensing>();
            var cosmos = new CosmosDBConnectivity(config.Object, logger.Object, licensing.Object);
            var m = await cosmos.GetBotModel(userId, modelName);
            File.WriteAllBytes(modelName, m.Model);
        }
    }
}
