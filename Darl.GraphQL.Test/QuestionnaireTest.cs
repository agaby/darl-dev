/// <summary>
/// QuestionnaireTest.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class QuestionnaireTest
    {

        [TestInitialize()]
        public void Initialize()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.AppSettings.json"));
            var source = reader.ReadToEnd();
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
