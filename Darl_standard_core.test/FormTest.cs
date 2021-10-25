using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DarlCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Darl.Forms;

namespace Darl_standard_core.test
{
    [TestClass]
    public class FormTest
    {
        [TestMethod]
        public async Task TestEUClaim()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EU_delayed_flight_claim_copy.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            QuestionCache qc = new QuestionCache();
            await rf.UpdateFromCode();
            var qsp = await form.Start(rf, qc);
            qsp.questions[0].sResponse = qsp.questions[0].categories[1]; // one
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].dResponse = 5;
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = qsp.questions[0].categories[2]; // all others
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = qsp.questions[0].categories[0]; //true
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].dResponse = 1600;
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "VS006";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "XYZ56R";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "06/11/2017";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "07.28";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "Werdna Sdnomde";
            qsp = await form.Next(qsp, rf, qc);
            Assert.IsTrue(qsp.complete);
            Assert.AreEqual(1, qsp.responses.Count);
            Assert.IsNull(qsp.questions);
            Assert.IsNotNull(qsp.responses[0].mainText);
            Assert.AreEqual(1249,qsp.responses[0].mainText.Length);
        }

        [TestMethod]
        public async Task TestMilitaryService()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.military_service.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            QuestionCache qc = new QuestionCache();
            await rf.UpdateFromCode();
            var qsp = await form.Start(rf, qc);
            qsp.questions[0].sResponse = qsp.questions[0].categories[0];
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = qsp.questions[0].categories[0];
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "andy@darl.ai";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "Andy Edmonds";
            qsp = await form.Next(qsp, rf, qc);
        }

        [TestMethod]
        public async Task TestEUDelayedWithTemporal()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EU_delayed_flight_claim_copy.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            var newSource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EUClaimTemporal.darl"));
            rf.darl = newSource.ReadToEnd();
            await rf.UpdateFromCode();
            QuestionCache qc = new QuestionCache();
            var qsp = await form.Start(rf, qc);
            qsp.questions[0].sResponse = "2016-08-07";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].dResponse = 5.5;
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "both";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "all_others";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].dResponse = 5000;
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "jhkhkh";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "jhkhkh";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "andy edmonds";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "08:00";
            qsp = await form.Next(qsp, rf, qc);

        }



        [TestMethod]
        public void TestFormFormatExtensions()
        {
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.UK_Tax_NI.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            var insert = rf.format.CreateTestDataSchema();
            var wrapper = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.dataschema.json")).ReadToEnd();
            var res = wrapper.Replace("#insert", insert);
            //check valid json
            var obj = JsonConvert.DeserializeObject(res);
        }

        [TestMethod]
        public async Task TestUpdateFromCode()
        {
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EU_delayed_flight_claim_copy.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            var res = await rf.UpdateFromCode();
            Assert.AreEqual(5, res.Count);
        }

        [TestMethod]
        public async Task TestUpdateFromCodeEmptyRuleset()
        {
            var rf = new RuleForm { darl = "ruleset fred {}" };
            await rf.UpdateFromCode();
        }

        [TestMethod]
        public async Task TestRuleFormCreate()
        {
            var rf = await FormFormatExtensions.CreateNewRuleForm("life rule set", "Jordan Peterson");
            Assert.AreEqual("Jordan Peterson", rf.author);
            Assert.AreEqual("ruleset life_rule_set\r\n{\r\n}", rf.darl);
            Assert.AreEqual("0.0", rf.version);
            Assert.AreEqual("life rule set", rf.name);
            Assert.AreEqual(1, rf.format.DefaultQuestions);
            Assert.AreEqual(false, rf.format.Edited);
            Assert.AreEqual(0, rf.format.InputFormatList.Count);
            Assert.AreEqual(0, rf.format.OutputFormatList.Count);
            Assert.AreEqual(0, rf.language.LanguageList.Count);
            Assert.AreEqual("en", rf.language.DefaultLanguage);
        }

        [TestMethod]
        public async Task TestMLModelCreate()
        {
            var rf = await FormFormatExtensions.CreateNewMLModel("iris.mlmodel", "Jordan Peterson");
            Assert.AreEqual("Jordan Peterson", rf.author);
            Assert.AreEqual("ruleset iris\r\n{\r\n}", rf.darl);
            Assert.AreEqual("0.0", rf.version);
            Assert.AreEqual("iris.mlmodel", rf.name);
            Assert.AreEqual("iris.rule", rf.destinationRulesetName);
        }

        [TestMethod]
        [Ignore]
        public async Task TestDarlFromJson()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Darl_standard_core.test.example_schema.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                var rf = await FormFormatExtensions.CreateRuleFormFromJsonSchema("data collection", "Andy edmonds", result);
                //save to file
                File.WriteAllText(@"C:\Users\Andrew\documents\visual studio 2017\Projects\Darl_standard\Darl_standard_core.test\data_collection.rf", JsonConvert.SerializeObject(rf));
                File.WriteAllText(@"C:\Users\Andrew\documents\visual studio 2017\Projects\Darl_standard\Darl_standard_core.test\data_collection.darl", rf.darl);
            }

        }

        [TestMethod]
        public async Task TestUpdateFromCodeNew()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.empty_eu.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            await rf.UpdateFromCode();
            Assert.IsNotNull(rf.format);
            Assert.IsNotNull(rf.language);
        }

        [TestMethod]
        public async Task TestUpdateFromCodeChange()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EU_delayed_flight_claim_copy.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            var newSource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.EUClaimTemporal.darl"));
            rf.darl = newSource.ReadToEnd();
            await rf.UpdateFromCode();
            Assert.IsNotNull(rf.format);
            Assert.IsNotNull(rf.language);
            Assert.AreEqual("Qualifying date", rf.language.LanguageList[37].Text);
        }

        /// <summary>
        /// Tests change to salience calcs in RuleRootNode where salience is divided not by the number of rules, but the number of unsatisfied rules, thus upping
        /// the salience of inputs that become more pivotal.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestAiTriageSalience()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.ai_triage.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            QuestionCache qc = new QuestionCache();
            await rf.UpdateFromCode();
            var qsp = await form.Start(rf, qc);
            qsp.questions[0].sResponse = "vectors of numbers and/or labels";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "true";
            qsp = await form.Next(qsp, rf, qc);
            Assert.AreEqual("rapid_change", qsp.questions[0].reference);
        }

        [TestMethod]
        public async Task TestUsingAISalience()
        {
            DarlForms form = new DarlForms();
            var jss = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.is_it_using_AI.rule"));
            RuleForm rf = JsonConvert.DeserializeObject<RuleForm>(source.ReadToEnd(), jss);
            QuestionCache qc = new QuestionCache();
            await rf.UpdateFromCode();
            var qsp = await form.Start(rf, qc);
            qsp.questions[0].sResponse = "no";
            qsp = await form.Next(qsp, rf, qc);
            qsp.questions[0].sResponse = "yes";
            qsp = await form.Next(qsp, rf, qc);
            Assert.AreEqual("movebyitself", qsp.questions[0].reference);
        }


    }
}
