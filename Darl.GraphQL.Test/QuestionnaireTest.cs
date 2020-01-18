using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Darl.GraphQL.Models.Connectivity;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class QuestionnaireTest
    {
        public AppSettings appSettings = null;

        [TestInitialize()]
        public void Initialize()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.AppSettings.json"));
            var source = reader.ReadToEnd();
            appSettings = JsonConvert.DeserializeObject<AppSettings>(source);

        }


        [TestCleanup()]
        public void Cleanup()
        {

        }
 /* 
        [TestMethod]
      
        public async Task TestDynamicRuleSet()
        {
            var m = new Mock<IDistributedCache>();
            var f = new FormApi(m.Object,null);
            var cosmos = new CosmosDBConnectivity(new OptionsWrapper<AppSettings>(appSettings));
            var adminuserId = "786e46c2-fa33-4124-af67-1bb14625c216";

            var rs = await cosmos.GetRuleSet(adminuserId, "UK Tax and NI.rule");
            var tp = await cosmos.GetRuleSet(adminuserId, "ruleseteditor.rule");
            var res = await f.CreateDynamicRuleSetEditor(rs,tp);
        }*/
    }
}
