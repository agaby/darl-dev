using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using DarlCommon;
using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class BotTests
    {


        [TestInitialize()]
        public void Initialize()
        {

        }

        [TestMethod]
        [Ignore]
        public async Task TestInteract()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions2.model"));
            vm.tree.SaveAttributes("noun:01,4,05,11,03", "if anything then response will be \"hi too\";", new List<string>(), new List<string>());
            List<DarlVar> values = new List<DarlVar>();
            var res = await vm.Interact("hi", values);
            Assert.AreEqual("hi too", res);
        }

        [TestMethod]
        //        [Ignore]
        public async Task TestInteractMultiLevel()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions2.model"));
            vm.tree.SaveAttributes("noun:01,4,05,11,03", "if anything then response will be \"hi too\";", new List<string>(), new List<string>());
            List<DarlVar> values = new List<DarlVar>();
            var res = await vm.Interact("hi calendar", values);
            //            Assert.AreEqual("hi too\nanswer for CALENDAR", res);
            var res2 = await vm.Interact("poop", values);
        }

        [TestMethod]
        public void TestCheckModelAgainstFramework()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions2.model"));
            vm.modelSettings = new Dictionary<string, string>() { { "name", "{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"DarlBot\"}" }, { "copyright", "{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}" }, { "version", "{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}" } };
            vm.form = "{\"InputFormatList\": [],\"OutputFormatList\": [{\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\"}]}";
            var bf = JsonConvert.DeserializeObject<BotFormat>(vm.form);
            bf.Stores.Add("UserData");
            bf.Stores.Add("ConversationData");
            bf.Stores.Add("PrivateConversationData");
            bf.Stores.Add("Bot");
            vm.form = JsonConvert.SerializeObject(bf);
            vm.ruleSkeleton = "ruleset botRuleset\n{\n\n/*%% rule_insertion_point %%*/\n}\n";
            var res = vm.CheckModelAgainstFramework();
            Assert.AreEqual(30, res.Count);
            res = vm.CheckModelAgainstFramework(true);
            Assert.AreEqual(30, res.Count);
            res = vm.CheckModelAgainstFramework();
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public void TestBotFormatToDarlConversion()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKTax.darl"));
            string source = reader.ReadToEnd();
            var bf = BotFormatExtensions.ToBotFormat(source);
            LineageModel vm = new LineageModel
            {
                form = JsonConvert.SerializeObject(bf)
            };
            var darl = vm.CreateCodeFromFormat();
            var rt = new DarlRunTime();
            var tree = rt.CreateTree(darl);
            Assert.IsFalse(tree.HasErrors());
        }


        [TestMethod]
        public async Task TestBotValueExtraction()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions3.model"));
            vm.tree.CreateExecutionTree();
            var stores = new Dictionary<string, ILocalStore>
            {
                { "UserData", new SettingsStore(new Dictionary<string, string>()) },
                { "ConversationData", new SettingsStore(new Dictionary<string, string>()) },
                { "PrivateConversationData", new SettingsStore(new Dictionary<string, string>()) },
                { "Bot", new SettingsStore(vm.modelSettings) }
            };
            List<DarlVar> values = new List<DarlVar>();
            stores.Add("Value", new ValuesStore(values));
            //            var resp = await vm.InteractTest(new DarlVar { Value = "how old am i", dataType = DarlVar.DataType.textual }, values, stores);
            var resp = await vm.InteractTest(new DarlVar { Value = "I am 28", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual("Thanks, I'm much younger", resp[0].response.Value);
            var res = JsonConvert.DeserializeObject<DarlVar>(((SettingsStore)stores["UserData"]).settings["age"]);
            Assert.AreEqual("28", res.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "how old am i", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual("28", resp[0].response.Value);
            Assert.IsNotNull(((ValuesStore)stores["Value"]).values);
        }

        [TestMethod]
        [Ignore]
        public async Task TestBotRuleCall()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions4.model"));
            vm.tree.CreateExecutionTree();
            List<DarlVar> values = new List<DarlVar>();
            var stores = vm.CreateStores("", new CallTest(), values, new LocalBotData(new Dictionary<string, string>()), new LocalBotData(new Dictionary<string, string>()), new LocalBotData(new Dictionary<string, string>()));
            var resp = await vm.InteractTest(new DarlVar { Value = "run ruleset \"UK Tax and NI.rule\"", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(2, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "whiffle", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("You must give a number", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "50000", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "10000", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "500", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("The value must be between 0 and 120 inclusive", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "50", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "back", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "50", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "Poop", dataType = DarlVar.DataType.categorical }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("You must give one of the choices", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "False", dataType = DarlVar.DataType.categorical }, values, stores);
            Assert.AreEqual(8, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "weeeble", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("I didn't understand, can you rephrase that?", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "run ruleset \"UK Tax and NI.rule\"", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(2, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "whiffle", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("You must give a number", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "50000", dataType = DarlVar.DataType.numeric }, values, stores);
            Assert.AreEqual(1, resp.Count);
            resp = await vm.InteractTest(new DarlVar { Value = "quit", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("Quitting rule set.", resp[0].response.Value);
            resp = await vm.InteractTest(new DarlVar { Value = "weeeble", dataType = DarlVar.DataType.textual }, values, stores);//test back at root
            Assert.AreEqual(1, resp.Count);
            Assert.AreEqual("I didn't understand, can you rephrase that?", resp[0].response.Value);
        }

        [TestMethod]
        public async Task TestBotValueCopy()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions4.model"));
            vm.tree.CreateExecutionTree();
            var values = new List<DarlVar>();
            //preload some of the ruleset data values and check they are copied
            var userDataStore = new Dictionary<string, string>
            {
                { "EARNED_INCOME", "50000" },
                { "DIVIDEND_INCOME", "10000" }
            };
            var stores = vm.CreateStores("", new CallTest(), values, new LocalBotData(userDataStore), new LocalBotData(new Dictionary<string, string>()), new LocalBotData(new Dictionary<string, string>()));
            var resp = await vm.InteractTest(new DarlVar { Value = "run ruleset \"UK Tax and NI.rule\"", dataType = DarlVar.DataType.textual }, values, stores);
            Assert.AreEqual(2, resp.Count);

        }

        [TestMethod]
        public void TestPhraseCreate()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.PhraseCreate("wurble/snurt/grooble");
            Assert.IsNotNull(vm.tree.Find("wurble/snurt/grooble"));
            vm.PhraseCreate("who/is/dr/bob");
            Assert.IsNotNull(vm.tree.Find("who/is/doctor|dr|doc/bob"));
            vm.PhraseCreate("what/do/you/know");
            Assert.IsNotNull(vm.tree.Find("what/do/you/know"));
            var res = vm.Match("what do you know", new List<DarlVar>());
            //test duplicate behavior
            vm.PhraseCreate("what/do/you/know");
        }

        [TestMethod]
        public void TestPhraseMerge()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.PhraseMerge("call/dr/andy", "talk/to/doctor|dr|doc/andy");
            var n = vm.tree.Find("call/dr/andy");
            Assert.IsNotNull(n);
            Assert.IsNotNull(n.annotation);
            Assert.IsTrue(n.annotation.darl[0].Contains("Call["));
        }

        [TestMethod]
        public void TestPhraseDelete()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            Assert.IsNotNull(vm.tree.Find("talk/to/doctor|dr|doc/andy"));
            vm.PhraseDelete("talk/to/doctor|dr|doc/andy");
            Assert.IsNull(vm.tree.Find("talk/to/doctor|dr|doc/andy"));
        }

        [TestMethod]
        public void TestPhraseSeparate()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            Assert.IsNotNull(vm.tree.Find("bug"));
            vm.PhraseCreate("bug|defect|mistake");
            Assert.IsNull(vm.tree.Find("bug"));
            Assert.IsNotNull(vm.tree.Find("bug|defect|mistake").annotation);
            //now we have a multimatch element at the root, separate bug off
            vm.PhraseSeparate("bug");
            Assert.IsNotNull(vm.tree.Find("bug"));
            Assert.IsNotNull(vm.tree.Find("defect|mistake"));
            Assert.IsNotNull(vm.tree.Find("bug").annotation);
            Assert.IsNotNull(vm.tree.Find("defect|mistake").annotation);
            vm.tree.Find("defect|mistake").annotation.darl[0] = "if anything then Class[\"\"] will be poo;";
            Assert.IsFalse(vm.tree.Find("bug").annotation.darl[0].Contains("poo"));
            Assert.IsTrue(vm.tree.Find("defect|mistake").annotation.darl[0].Contains("poo"));
        }

        [TestMethod]
        public async Task TestSimpleMatch()
        {
            LineageModel lm = new LineageModel();
            var name = "fred";
            lm.ruleSkeleton = "ruleset botRuleset\n{\n /*%% rule_insertion_point %%*/\n}";
            lm.modelSettings.Add("name", $"{{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"{name}\"}}");
            lm.modelSettings.Add("copyright", "{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}");
            lm.modelSettings.Add("version", "{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}");
            lm.form = "{\"InputFormatList\": [], \"OutputFormatList\": [{\"Categories\": null,\"Sets\": null,\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\",\"ValueFormat\": null},{\"Categories\": null,\"Sets\": null,\"Name\": \"link\",\"OutputType\": \"textual\",\"displayType\": \"Link\",\"ValueFormat\": null}],\"Stores\": [\"UserData\",\"ConversationData\",\"PrivateConversationData\",\"Bot\",\"Value\",\"Call\",\"Word\",\"Rest\",\"Collateral\"],\"Strings\": {}, \"Constants\": {}, \"Sequences\": {}}";
            lm.tree = new LineageMatchTree();
            lm.PhraseCreate("default:");
            lm.tree.SaveAttributes("default:", "if anything then response will be \"I don't know the answer to that\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:019/jeremy/corbyn");
            lm.tree.SaveAttributes("who/verb:019/jeremy/corbyn", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            var input = new DarlVar();
            input.dataType = DarlVar.DataType.textual;
            input.name = "input";
            input.Value = "Who is Jeremy Corbyn?";
            var responses = await lm.InteractTest(input, new List<DarlVar>(), new Dictionary<string, ILocalStore>());
            Assert.AreEqual("A total cunt.", responses[0].response.Value);
        }
        [TestMethod]
        public async Task TooManyJohnsTest()
        {
            LineageModel lm = new LineageModel();
            var name = "fred";
            lm.ruleSkeleton = "ruleset botRuleset\n{\n /*%% rule_insertion_point %%*/\n}";
            lm.modelSettings.Add("name", $"{{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"{name}\"}}");
            lm.modelSettings.Add("copyright", "{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}");
            lm.modelSettings.Add("version", "{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}");
            lm.form = "{\"InputFormatList\": [], \"OutputFormatList\": [{\"Categories\": null,\"Sets\": null,\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\",\"ValueFormat\": null},{\"Categories\": null,\"Sets\": null,\"Name\": \"link\",\"OutputType\": \"textual\",\"displayType\": \"Link\",\"ValueFormat\": null}],\"Stores\": [\"UserData\",\"ConversationData\",\"PrivateConversationData\",\"Bot\",\"Value\",\"Call\",\"Word\",\"Rest\",\"Collateral\"],\"Strings\": {}, \"Constants\": {}, \"Sequences\": {}}";
            lm.tree = new LineageMatchTree();
            lm.PhraseCreate("default:");
            lm.tree.SaveAttributes("default:", "if anything then response will be \"I don't know the answer to that\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/jeremy/corbyn");
            lm.tree.SaveAttributes("who/verb:487/jeremy/corbyn", "if anything then response will be \"A total complete cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/austin");
            lm.tree.SaveAttributes("who/verb:487/john/austin", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/cryer");
            lm.tree.SaveAttributes("who/verb:487/john/cryer", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/hopkins");
            lm.tree.SaveAttributes("who/verb:487/john/hopkins", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/humphreys");
            lm.tree.SaveAttributes("who/verb:487/john/humphreys", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/mccallister");
            lm.tree.SaveAttributes("who/verb:487/john/mccallister", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/mcdonnell");
            lm.tree.SaveAttributes("who/verb:487/john/mcdonnell", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/odowd");
            lm.tree.SaveAttributes("who/verb:487/john/odowd", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/pilger");
            lm.tree.SaveAttributes("who/verb:487/john/pilger", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/ross");
            lm.tree.SaveAttributes("who/verb:487/john/ross", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/smith");
            lm.tree.SaveAttributes("who/verb:487/john/smith", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/john/williams");
            lm.tree.SaveAttributes("who/verb:487/john/williams", "if anything then response will be \"A total cunt.\";", new List<string>(), new List<string>());
            var input = new DarlVar();
            input.dataType = DarlVar.DataType.textual;
            input.name = "input";
            input.Value = "Who is Jeremy Corbyn?";
            var responses = await lm.InteractTest(input, new List<DarlVar>(), new Dictionary<string, ILocalStore>());
            Assert.AreEqual("A total complete cunt.", responses[0].response.Value);
            input.Value = "Who is john mcdonnell?";
            responses = await lm.InteractTest(input, new List<DarlVar>(), new Dictionary<string, ILocalStore>());
            Assert.AreEqual("A total cunt.", responses[0].response.Value);
            //now test with fuzzy on
            input.Value = "Who is john mcdonell?";
            responses = await lm.InteractTest(input, new List<DarlVar>(), new Dictionary<string, ILocalStore>(), true);
            Assert.AreEqual("A total cunt.", responses[0].response.Value);
            input.Value = "Who is Jeremy Corbin?";
            responses = await lm.InteractTest(input, new List<DarlVar>(), new Dictionary<string, ILocalStore>(), true);
            Assert.AreEqual("A total complete cunt.", responses[0].response.Value);

        }

        [TestMethod]
        public async Task TestGraphStore()
        {
            LineageModel lm = new LineageModel();
            var name = "fred";
            lm.ruleSkeleton = "ruleset botRuleset\n{\n /*%% rule_insertion_point %%*/\n}";
            lm.modelSettings.Add("name", $"{{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"{name}\"}}");
            lm.modelSettings.Add("copyright", "{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}");
            lm.modelSettings.Add("version", "{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}");
            lm.form = "{\"InputFormatList\": [], \"OutputFormatList\": [{\"Categories\": null,\"Sets\": null,\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\",\"ValueFormat\": null},{\"Categories\": null,\"Sets\": null,\"Name\": \"link\",\"OutputType\": \"textual\",\"displayType\": \"Link\",\"ValueFormat\": null}],\"Stores\": [\"UserData\",\"ConversationData\",\"PrivateConversationData\",\"Bot\",\"Value\",\"Call\",\"Word\",\"Rest\",\"Collateral\",\"Graph\"],\"Strings\": {}, \"Constants\": {}, \"Sequences\": {}}";
            lm.tree = new LineageMatchTree();
            lm.PhraseCreate("default:");
            lm.tree.SaveAttributes("default:", "if anything then response will be \"I don't know the answer to that\";", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/value:text");
            lm.tree.SaveAttributes("who/verb:487/value:text", "output textual val;\n if anything then val will be Value[\"value:text\"];\n if anything then response will be Graph[\"text\",val,\"noun:00,2,00\"];", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/value:text/verb:248");
            lm.tree.SaveAttributes("who/verb:487/value:text/verb:248", "output textual val;\n if anything then val will be Value[\"value:text\"];\n if anything then response will be Graph[\"links\",val,\"noun:00,2,00\"];", new List<string>(), new List<string>());
            lm.PhraseCreate("who/verb:487/verb:248/to/value:text");
            lm.tree.SaveAttributes("who/verb:487/verb:248/to/value:text", "output textual val;\n if anything then val will be Value[\"value:text\"];\n if anything then response will be Graph[\"links\",val,\"noun:00,2,00\"];", new List<string>(), new List<string>());
            lm.PhraseCreate("how/verb:487/value:text/and/value:text/verb:248");
            lm.tree.SaveAttributes("how/verb:487/value:text/and/value:text/verb:248", "output textual val1;\noutput textual val2;\n if anything then val1 will be Value[\"value:text\",\"0\"];\nif anything then val2 will be Value[\"value:text\",\"1\"];\n if anything then response will be Graph[\"path\",val1,val2,\"noun:00,2,00\",\"noun:00,2,00\"];", new List<string>(), new List<string>());
            var stores = new Dictionary<string, ILocalStore>();
            var values = new List<DarlVar>();
            var graphStore = new Mock<ILocalStore>();
            graphStore.Setup(a => a.ReadAsync(It.Is<List<string>>(a => a[0] == "text" && a[1] == "jeremy corbyn"))).Returns(Task.FromResult(new DarlResult("result", "A load of text related to Jeremy Corbyn All about his turdiness and vacuity........", DarlResult.DataType.textual)));
            graphStore.Setup(a => a.ReadAsync(It.Is<List<string>>(a => a[0] == "links" && a[1] == "jeremy corbyn"))).Returns(Task.FromResult(new DarlResult("result", "A bunch of people linked to that turd.", DarlResult.DataType.textual)));
            graphStore.Setup(a => a.ReadAsync(It.Is<List<string>>(a => a[0] == "path" && a[1] == "jeremy corbyn" && a[2] == "paul mason"))).Returns(Task.FromResult(new DarlResult("result", "path between jc and pm", DarlResult.DataType.textual)));
            graphStore.Setup(a => a.ReadAsync(It.Is<List<string>>(a => a[1] != "jeremy corbyn"))).Returns(Task.FromResult(new DarlResult("result", 0.0, true)));
            stores.Add("Graph", graphStore.Object);
            stores.Add("Value", new ValuesStore(values));
            var input = new DarlVar();
            input.dataType = DarlVar.DataType.textual;
            input.name = "input";
            input.Value = "Who is Jeremy Corbyn?";
            var responses = await lm.InteractTest(input, values, stores);
            Assert.AreEqual(2, responses.Count);
            Assert.IsTrue(responses.Last().response.Value.Length > 20);
            input.Value = "Who is Jeremy Corbyn connected to?";
            responses = await lm.InteractTest(input, values, stores);
            Assert.AreEqual(4, responses.Count);
            Assert.AreEqual("A bunch of people linked to that turd.", responses.Last().response.Value);
            input.Value = "Who is connected to Jeremy Corbyn ?";
            responses = await lm.InteractTest(input, values, stores);
            Assert.AreEqual(6, responses.Count);
            Assert.AreEqual("A bunch of people linked to that turd.", responses.Last().response.Value);
            input.Value = "how are Jeremy Corbyn and Paul Mason connected ?";
            responses = await lm.InteractTest(input, values, stores);
            Assert.AreEqual(1, responses.Count);
            Assert.AreEqual("path between jc and pm", responses.Last().response.Value);
        }


    }

    internal class CallTest : IRuleFormInterface
    {
        public async Task<DarlCommon.RuleForm> Get(string address)
        {
            Assert.IsTrue(address.Length > 0);
            Assert.IsFalse(address.Contains("\""));
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UK Tax and NI.rule"));
            string source = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<RuleForm>(source);
        }

        public Task<string> GetCollateral(string user, string v)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDetails(string address)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetListings()
        {
            throw new NotImplementedException();
        }
    }


}
