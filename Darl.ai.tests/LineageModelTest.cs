/// </summary>

﻿using Darl.Lineage;
using Darl.Lineage.Bot;
using DarlCommon;
using DarlLanguage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Darl_standard_core.test
{
    [TestClass]
    public class LineageModelTest
    {
        [Ignore]
        public void TestLineageModelAIMLLoad()
        {

            var lm = new LineageModel();
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (file.EndsWith("aiml"))
                {
                    lm.LoadAIML(Assembly.GetExecutingAssembly().GetManifestResourceStream(file));
                }
            }
            lm.PostProcessAIML();
            var model = "bot.model";
            using (var file = File.Open(model, FileMode.OpenOrCreate))
            {
                lm.Store(file);
            }
            var lm2 = new LineageModel();
            lm2 = LineageModel.Load(File.OpenRead(model));
            List<DarlVar> values = new List<DarlVar>();
            var res1 = lm2.Match("what is ai", values);
            var res2 = lm2.Match("who is lauren", values);
            var res3 = lm2.Match("zoom zoom zoom", values);
        }

        [TestMethod]
        public void TestLineageCreateTree()
        {
            var lm = new LineageModel();
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (file.EndsWith("aiml"))
                {
                    lm.LoadAIML(Assembly.GetExecutingAssembly().GetManifestResourceStream(file));
                }
            }
            lm.PostProcessAIML();

        }

        [Ignore]
        public void TestLineageTreeMatch()
        {
            var lm = new LineageModel();
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (file.EndsWith("aiml"))
                {
                    lm.LoadAIML(Assembly.GetExecutingAssembly().GetManifestResourceStream(file));
                }
            }
            lm.PostProcessAIML();
            //var sb = new StringBuilder();
            //lm.ReadTree(sb);
            //File.WriteAllText("treecontents.txt", sb.ToString());
            List<DarlVar> values = new List<DarlVar>();
            var res1 = lm.Match("what is ai", values);
            var res2 = lm.Match("who is lauren", values);
            var res3 = lm.Match("zoom zoom zoom", values);
        }

        [TestMethod]
        public void TestAIMLContent()
        {
            var lm = new LineageModel();
            foreach (var file in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (file.EndsWith("aiml"))
                {
                    lm.LoadAIML(Assembly.GetExecutingAssembly().GetManifestResourceStream(file));
                }
            }
            lm.PostProcessAIML();
            /*            var sb = new StringBuilder();
                        foreach(var n in LineageModelAIMLExtensions.nodes)
                        {
                            sb.AppendLine(n.ToString());
                        }
                        File.WriteAllText("nodes.txt", sb.ToString());
                        sb = new StringBuilder();
                        foreach (var s in LineageModelAIMLExtensions.sets)
                        {
                            sb.AppendLine(s.ToString());
                        }
                        File.WriteAllText("sets.txt", sb.ToString());*/

        }

        [Ignore]
        public void TestLoadedBotProcessing()
        {
            var s = LineageLibrary.LookupWord("calorie");
            var s1 = LineageLibrary.LookupWord("he");
            var s2 = LineageLibrary.LookupWord("she");
            var s3 = LineageLibrary.LookupWord("it");
            var s4 = LineageLibrary.LookupWord("i");
            var s5 = LineageLibrary.LookupWord("you");
            var s6 = LineageLibrary.LookupWord("me");
            var s7 = LineageLibrary.LookupWord("they");
            var model = "bot.model";
            var lm2 = new LineageModel();
            lm2 = LineageModel.Load(File.OpenRead(model));
            List<DarlVar> values = new List<DarlVar>();
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.TestQuestions.txt"));
            StringBuilder sb = new StringBuilder();
            string line;
            int hitCount = 0;
            int total = 0;
            while ((line = str.ReadLine()) != null)
            {
                var res = lm2.Match(line, values);
                if (res.Count > 0)
                {
                    hitCount++;
                }
                else
                {
                    sb.AppendLine(line);
                }
                total++;
            }
            str.Close();
            Console.WriteLine($"{hitCount.ToString()} questions out of {total.ToString()} recognised.");
            File.WriteAllText("missedtexts.txt", sb.ToString());
            using (var file = File.Open("model.tree", FileMode.OpenOrCreate))
            {
                Serializer.Serialize<LineageMatchTree>(file, lm2.tree);
            }

        }

        [TestMethod]
        public void TestCreateTreeFromTestQuestions()
        {
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.TestQuestions.txt"));
            string line;
            var source = new List<LineageTemplateSet>();
            while ((line = str.ReadLine()) != null)
            {
                var lts = new LineageTemplateSet();
                lts.templates = new List<LineageTemplate>();
                lts.templates.Add(LineageModelAIMLExtensions.CreateLineageTemplate(line));
                source.Add(lts);
            }
            var tree = new LineageMatchTree(source);
            using (var file = File.Open("tree.tree", FileMode.OpenOrCreate))
            {
                Serializer.Serialize<LineageMatchTree>(file, tree);
            }
            using (var file = File.OpenRead("tree.tree"))
            {
                var res = Serializer.Deserialize<LineageMatchTree>(file);
            }
            var children = tree.Navigate("who/is");
        }

        [Ignore]
        public void CreateModelBasedOnTexts()
        {
            var model = "bot.model";
            var lm2 = new LineageModel();
            lm2 = LineageModel.Load(File.OpenRead(model));
            var lm3 = new LineageModel();
            List<DarlVar> values = new List<DarlVar>();
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.TestQuestions.txt"));
            StringBuilder sb = new StringBuilder();
            string line;
            var source = new List<LineageTemplateSet>();
            while ((line = str.ReadLine()) != null)
            {
                var lts = new LineageTemplateSet();
                lts.templates = new List<LineageTemplate>();
                lts.templates.Add(LineageModelAIMLExtensions.CreateLineageTemplate(line));
                var res = lm2.Match(line, values);
                if (res.Count > 0)
                {
                    foreach (var r in res)
                        lts.payload += r;
                }
                else
                {
                    lts.payload = $"if anything then response will be \"answer for {line}\";";
                }
                source.Add(lts);
            }
            var tree = new LineageMatchTree(source);
            using (var file = File.Open("thousandquestions.model", FileMode.OpenOrCreate))
            {
                Serializer.Serialize<LineageMatchTree>(file, tree);
            }
            str.Close();
        }

        [Ignore]
        public void UpdateModel()
        {
            LineageMatchTree res;
            using (var file = File.OpenRead("thousandquestions.model"))
            {
                res = Serializer.Deserialize<LineageMatchTree>(file);
            }
            var lm = new LineageModel();
            lm.tree = res;
            using (var file = File.Open("thousandquestions.model", FileMode.OpenOrCreate))
            {
                Serializer.Serialize<LineageModel>(file, lm);
            }


        }

        [TestMethod]
        public void TestRenameDeletePasteAdd()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            vm.tree.Rename("add/a", "some");
            List<DarlVar> values = new List<DarlVar>();
            var res1 = vm.Match("add some contact", values);
            Assert.IsTrue(res1.Count > 0);
            Assert.IsTrue(res1.Count > 0 && ((MatchedAnnotation)res1[1]).annotation.darl[0].Contains("ADD A CONTACT"));
            Assert.AreEqual("default:", res1[0].path);
            Assert.AreEqual("add/some/contact", res1[1].path);
            vm.tree.Delete("add/some");
            var res = vm.Match("add some contact", values);
            Assert.IsTrue(res.Count > 0 && ((MatchedAnnotation)res[0]).annotation.darl[0].Contains("I didn't understand")); //default response
            Assert.AreEqual("default:", res[0].path);
            vm.tree.Add("add", "albatross");
            Assert.IsTrue(vm.tree.Find("add/albatross") != null);
            vm.tree.Paste("add/albatross", new List<string> { "address/and/phone" }, "move_node");
            Assert.IsTrue(vm.tree.Find("address/and/phone") == null); //gone from previous location
            Assert.IsTrue(vm.tree.Find("add/albatross/phone/number") != null); //added to new with children.
                                                                               //            var fsp = vm.tree.FindSearchPaths("who is");
                                                                               //            Assert.AreEqual(2, fsp.Count);
            vm.tree.SaveAttributes("who/is", "If anything then result will be \"pooh\";", new List<string> { "imp1", "imp2" }, new List<string>());
            Assert.AreEqual(1, vm.tree.Find("who/is").annotation.darl.Count);
            Assert.AreEqual(2, vm.tree.Find("who/is").annotation.implications.Count);
        }

        [TestMethod]
        public void TestRenameComposite()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.tree.Rename("noun:01,4,05,11,03", "noun:01,4,05,11,03|hey");
        }

        [TestMethod]
        public void TestCompositeAtRoot()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.tree.Add("#", "ok|okay");
            vm.tree.SaveAttributes("ok|okay", "if anything then response will be randomtext(\"good\",\"that's fine\",\"glad I could help\");", new List<string>(), new List<string>());
            vm.tree.Rename("noun:01,4,05,11,03", "noun:01,4,05,11,03|hey");
            vm.tree.CreateExecutionTree();
            var res = vm.Match("ok", new List<DarlVar>());
            Assert.AreEqual(2, res.Count); //more than just the default present.
        }

        [TestMethod]
        public void TestMatchOrder()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.tree.Add("#", "bot");
            vm.tree.SaveAttributes("bot", "if anything then response will be \"bot?\");", new List<string>(), new List<string>());
            vm.PhraseCreate("who/is/this/bot");
            vm.tree.SaveAttributes("who/is/this/bot", "if anything then response will be \"this bot\");", new List<string>(), new List<string>());
            var res = vm.Match("who is this bot", new List<DarlVar>());
            Assert.AreEqual(3, res.Count); //more than just the default present.
            Assert.AreEqual("who/is/this/bot", res[2].path); //the longer match has won, despite occuring earlier
        }

        [TestMethod]
        public void TestPathLineage()
        {
            LineageModel vm;
            using (var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions4.model"))
            {
                vm = Serializer.Deserialize<LineageModel>(file);
            }
            LineageMatchNode.comp.lineageMatch = true;
            vm.tree.CreateExecutionTree();
            List<DarlVar> values = new List<DarlVar>();
            var res1 = vm.Match("hi", values);
            Assert.AreEqual("default:", res1[0].path);
            Assert.AreEqual("noun:01,4,05,11,03", res1[1].path);
        }



        [TestMethod]
        public void TestDefaultHandling()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            vm.tree.Add("", "default:");
            vm.tree.SaveAttributes("default:", "If anything then result will be \"I don't know the answer to that.\";", new List<string> { "imp1", "imp2" }, new List<string>());
            List<DarlVar> values = new List<DarlVar>();
            var res = vm.Match("snurgleburgle", values);
            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public void TestSymbolHandling()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            //test add and rename to recognise a symbol definition.
            vm.tree.Add("", "default:");
            Assert.AreEqual(LineageType.Default, vm.tree.Find("default:").element.type);
            vm.tree.Rename("add/a", "value:number");
            Assert.AreEqual(LineageType.value, vm.tree.Find("add/value:number").element.type);
        }

        [TestMethod]
        public void TestValueExtraction()
        {

            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            List<DarlVar> values = new List<DarlVar>();
            vm.tree.Rename("run/ruleset/value:(text1)", "value:(text1)");
            var tokens = LineageLibrary.SimpleTokenizer("run ruleset fred");
            var darl = vm.tree.Match(tokens, values);
            Assert.AreEqual(2, darl.Count);
            Assert.AreEqual(1, darl[1].values.Count);
            Assert.AreEqual("value:", darl[1].values[0].name);
            Assert.AreEqual("fred", darl[1].values[0].Value);
            Assert.AreEqual(DarlVar.DataType.textual, darl[1].values[0].dataType);
        }

        [TestMethod]
        public void TestMultiMatch()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            List<DarlVar> values = new List<DarlVar>();
            var matches = vm.Match("i am 25 hey are you still there", values);
        }

        [TestMethod]
        public void TestCompositePathMatch()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            List<DarlVar> values = new List<DarlVar>();
            vm.tree.CreateExecutionTree();
            var matches = vm.Match("who is dr andy", values);
            Assert.IsTrue(matches[1].path.Contains("doctor|dr|doc"));
        }

        [TestMethod]
        public void TestValueNoMatch()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions4.model"));
            vm.tree.CreateExecutionTree();
            List<DarlVar> values = new List<DarlVar>();
            var matches = vm.Match("i am old", values);
            Assert.AreEqual("default:", matches[0].path);
            Assert.AreEqual(1, matches.Count);
        }

        [TestMethod]
        public void TestAddDescriptions()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            vm.AddDescriptions();
            Assert.IsTrue(vm.tree.root.children["access"].element.description.Contains("Literal"));
        }


        [TestMethod]
        public void TestRationalize()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions.model"));
            vm.tree.Add("", "noun:01,4,05,11,03");
            Assert.AreEqual(114, vm.tree.root.children.Count);
            vm.Rationalize();
            Assert.AreEqual(112, vm.tree.root.children.Count);
        }

        [TestMethod]
        public void TestDefaultBug()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions2.model"));
            vm.tree.CreateExecutionTree();
            List<DarlVar> values = new List<DarlVar>();
            var res = vm.Match("hi", values);
        }

        [TestMethod]
        public void TestIncrementVersion()
        {
            var lm = new LineageModel();
            lm.modelSettings.Add("version", JsonConvert.SerializeObject(new DarlVar { unknown = false, Value = "1.0.0.0", dataType = DarlVar.DataType.textual }));
            lm.IncrementVersion();
            var newVersion = JsonConvert.DeserializeObject<DarlVar>(lm.modelSettings["version"]);
            Assert.AreEqual("1.0.0.1", newVersion.Value);
            Assert.AreEqual(DarlVar.DataType.textual, newVersion.dataType);
            Assert.AreEqual("version", newVersion.name);
            lm = new LineageModel();
            lm.modelSettings.Add("version", JsonConvert.SerializeObject(new DarlVar { unknown = false, Value = "57", dataType = DarlVar.DataType.numeric }));
            lm.IncrementVersion();
            newVersion = JsonConvert.DeserializeObject<DarlVar>(lm.modelSettings["version"]);
            Assert.AreEqual("58", newVersion.Value);
            Assert.AreEqual(DarlVar.DataType.numeric, newVersion.dataType);
            Assert.AreEqual("version", newVersion.name);
            lm = new LineageModel(); //test textual not in version format
            lm.modelSettings.Add("version", JsonConvert.SerializeObject(new DarlVar { unknown = false, Value = "first", dataType = DarlVar.DataType.textual, name = "version" }));
            lm.IncrementVersion();
            newVersion = JsonConvert.DeserializeObject<DarlVar>(lm.modelSettings["version"]);
            Assert.AreEqual("first", newVersion.Value); //no change
            Assert.AreEqual(DarlVar.DataType.textual, newVersion.dataType);
            Assert.AreEqual("version", newVersion.name);
            lm = new LineageModel(); //test bad number
            lm.modelSettings.Add("version", JsonConvert.SerializeObject(new DarlVar { unknown = false, Value = "first", dataType = DarlVar.DataType.numeric, name = "version" }));
            lm.IncrementVersion();
            newVersion = JsonConvert.DeserializeObject<DarlVar>(lm.modelSettings["version"]);
            Assert.AreEqual("first", newVersion.Value); //no change
            Assert.AreEqual(DarlVar.DataType.numeric, newVersion.dataType);
            Assert.AreEqual("version", newVersion.name);
        }

        [TestMethod]
        public void TestNavigateExecutionTree()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            var nodes = vm.tree.Navigate("who/is");
            var res = vm.tree.NavigateExecutionTree(@"who/is/dr/andy");
            Assert.IsFalse(res.Any());
            var fs = vm.tree.FindExecutionTree(@"who/is/dr/andy");
            Assert.IsNotNull(fs);
            Assert.IsNotNull(fs.annotation.darl);
            Assert.IsTrue(fs.annotation.darl.Any());
        }

        [TestMethod]
        public void TestCreateCompositeCode()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            var res = vm.CreateCompositeCode("if anything then response will be \"bugger\";");
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(res);
            Assert.IsFalse(tree.HasErrors());
            Assert.AreEqual(251, res.Length);
        }

        [TestMethod]
        public void TestReconcileCode()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            var fs = vm.tree.FindExecutionTree(@"who/is/dr/andy").annotation.darl[0];
            //single text replacement
            var nc = vm.ReconcileCode(fs, new BotFragment { Response = "An old fart", CallRuleset = "", RandomResponses = new List<string>() }, "who/is/dr/andy");
            Assert.IsTrue(nc.Contains("if anything then response will be \"An old fart\""));
            //single text replacement and added call.
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "An old fart", CallRuleset = "buggery.rule", RandomResponses = new List<string>() }, "who/is/dr/andy");
            Assert.IsTrue(nc.Contains("if anything then response will be \"An old fart\""));
            Assert.IsTrue(nc.Contains("if anything then Call[\"\"] will be \"buggery.rule\""));
            //single to random text and call
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "", CallRuleset = "buggery.rule", RandomResponses = new List<string> { "poop", "whoop", "bum" } }, "who/is/dr/andy");
            Assert.IsTrue(nc.Contains("if anything then response will be randomtext( \"poop\" , \"whoop\" , \"bum\" )"));
            Assert.IsTrue(nc.Contains("if anything then Call[\"\"] will be \"buggery.rule\""));
            //randomtext to single text
            fs = vm.tree.FindExecutionTree(@"noun:01,4,05,11,03").annotation.darl[0];
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "Fuck you.", CallRuleset = "", RandomResponses = new List<string>() }, "noun:01,4,05,11,03");
            Assert.IsTrue(nc.Contains("if anything then response will be \"Fuck you.\""));
            //no text causing deletion
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "", CallRuleset = "", RandomResponses = new List<string>() }, "noun:01,4,05,11,03");
            Assert.IsTrue(string.IsNullOrEmpty(nc));
            //no call causing deletion
            fs = vm.tree.FindExecutionTree(@"talk/to/dr/andy").annotation.darl[0];
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "", CallRuleset = "", RandomResponses = new List<string>() }, "talk/to/dr/andy");
            Assert.IsTrue(string.IsNullOrEmpty(nc));
            fs = vm.tree.FindExecutionTree("camera").annotation.darl[0];
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "No, you're a camera", CallRuleset = null, RandomResponses = new List<string>() }, "camera");
            Assert.IsTrue(nc.Contains("if anything then response will be \"No, you're a camera\""));
            fs = vm.tree.FindExecutionTree("call/me/a/cab").annotation.darl[0];
            nc = vm.ReconcileCode(fs, new BotFragment { Response = "OK. You're a cab. Happy?", CallRuleset = null, RandomResponses = new List<string>() }, "call/me/a/cab");
            Assert.IsTrue(nc.Contains("if anything then response will be \"OK. You're a cab. Happy?\""));
            //now check behaviour for brand new node
            vm.tree.Add("delete/all", "people");
            nc = vm.ReconcileCode(null, new BotFragment { Response = "That would be cruel.", CallRuleset = null, RandomResponses = new List<string>() }, "delete/all/people");
            Assert.IsTrue(nc.Contains("if anything then response will be \"That would be cruel.\";"));
            vm.tree.Add("delete/all", "cats");
            nc = vm.ReconcileCode(null, new BotFragment { Response = "", CallRuleset = null, RandomResponses = new List<string> { "really?", "no way.", "I like cats" } }, "delete/all/cats");
            Assert.IsTrue(nc.Contains("if anything then response will be randomtext(\"really?\",\"no way.\",\"I like cats\");"));
            vm.tree.Add("delete/all", "donkeys");
            nc = vm.ReconcileCode(null, new BotFragment { Response = "", CallRuleset = "delete_donkeys.rule", RandomResponses = new List<string>() }, "delete/all/donkeys");
            Assert.IsTrue(nc.Contains("if anything then Call[\"\"] will be \"delete_donkeys.rule\";"));
            vm.tree.Add("delete/all", "bots");
            nc = vm.ReconcileCode(null, new BotFragment { Response = "Let's talk about that...", CallRuleset = "delete_bots.rule", RandomResponses = new List<string>() }, "delete/all/donkeys");
            Assert.IsTrue(nc.Contains("if anything then Call[\"\"] will be \"delete_bots.rule\";"));
            Assert.IsTrue(nc.Contains("if anything then response will be \"Let's talk about that...\";"));

        }

        [TestMethod]
        public void TestNavigate()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            var res = vm.tree.Navigate("call/me/a/cab");
        }


        [TestMethod]
        public void TestExtractCompositeCode()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            var fs = vm.tree.FindExecutionTree(@"who/is/dr/andy").annotation.darl[0];
            var res = vm.CreateCompositeCode(fs);
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(res);
            Assert.IsFalse(tree.HasErrors());
            var cc = vm.extractCompositeCode(tree, runtime);
            //may differ in whitespace
            Assert.AreEqual(cc.Replace(" ", "").Trim(), fs.Replace(" ", "").Trim());
        }

        [TestMethod]
        public void TestBestMatch()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.tree.CreateExecutionTree();
            var res = vm.BestMatch("I don't know who is dr bumface");
            Assert.IsTrue(res.Contains("doctor|dr|doc"));
            res = vm.BestMatch("hi you");
            Assert.IsTrue(res.Contains("noun:01,4,05,11,03"));
            res = vm.BestMatch("i am 62 you old fart");
            Assert.IsTrue(res.Contains("i/am/value:number,integer"));
            res = vm.BestMatch("you bummed 52 people"); // test recognises numeric value
            Assert.IsTrue(res.Contains("you/bummed/value:number,integer/people"));
            res = vm.BestMatch("warboogle");
            Assert.IsTrue(res.Contains("warboogle"));
            //test with apostrophes
            res = vm.BestMatch("who's your daddy");
            Assert.IsTrue(res.Contains("who's/your/daddy"));

        }

        [TestMethod]
        public void TestOldFileLoad()
        {
            var model = LineageModel.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestionsold.model"));
        }


        [TestMethod]
        public void TestSaveAttributeBug()
        {
            var path = "calendar";
            var darl = "if anything then response will be \"answer for CALENDAR\";";
            var model = LineageModel.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestionsold.model"));
            var newCode = model.ReconcileCode(darl, new BotFragment { CallRuleset = null, RandomResponses = new List<string>(), Response = "We don't have a calendar" }, path);
            model.tree.SaveAttributes(path, newCode.Trim(), new List<string>(), new List<string> { });
            model.Store(File.Create("temp.model"));
        }

        [TestMethod]
        public void TestSaveAttributeNoDARLBug()
        {//reconcile code fails if path contains "|". 
            var path = "make/a/bot|chatbot";
            var model = LineageModel.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestionsold.model"));
            model.PhraseCreate(path);
            var newCode = model.ReconcileCode("", new BotFragment { CallRuleset = "full_service.rule", RandomResponses = new List<string>(), Response = "" }, path);
            Assert.IsTrue(newCode.Length > 1);
        }

        [TestMethod]
        public void TestLoadEmpty()
        {
            LineageModel lm = new LineageModel();
            LineageModel newlm;
            using (MemoryStream ms = new MemoryStream())
            {
                lm.Store(ms);
                ms.Position = 0;
                newlm = LineageModel.Load(ms);
            }
            newlm.PhraseCreate("poop/whoop");
        }

        [TestMethod]
        public void TestFuzzy()
        {
            LineageModel vm = Serializer.Deserialize<LineageModel>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.thousandquestions5.model"));
            vm.tree.CreateExecutionTree();
            var matches = vm.tree.Match(new List<string> { "who", "is", "dr", "andu" }, new List<DarlVar>(), true);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual(0.75, matches[1].confidence);
            Assert.AreEqual("who/is/doctor|dr|doc/andy", matches[1].path);
            matches = vm.tree.Match(new List<string> { "who", "is", "doctoe", "andy" }, new List<DarlVar>(), true);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual(0.83, matches[1].confidence, 0.01);
            Assert.AreEqual("who/is/doctor|dr|doc/andy", matches[1].path);
            matches = vm.tree.Match(new List<string> { "who", "is", "doctoe", "andu" }, new List<DarlVar>(), true);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual(0.75, matches[1].confidence, 0.01);
            Assert.AreEqual("who/is/doctor|dr|doc/andy", matches[1].path);
            matches = vm.tree.Match(new List<string> { "whop", "is", "doctoe", "andu" }, new List<DarlVar>(), true);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual(0.75, matches[1].confidence, 0.01);
            Assert.AreEqual("who/is/doctor|dr|doc/andy", matches[1].path);
        }


        /*
                [TestMethod]
                public void TestConvert2()
                {
                    LineageModel2 vm2 = null;

                    try
                    {
                        using (var file = File.OpenRead("thousandquestions.model"))
                        {
                            vm2 = Serializer.Deserialize<LineageModel2>(file);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                    //now convert to LineageModel and save as converted file

                    LineageModel vm = new LineageModel() { form = vm2.form, modelSettings = vm2.modelSettings, ruleSkeleton = vm2.ruleSkeleton, texts = vm2.texts, tree = new LineageMatchTree { root = vm2.tree.root } };
                    using (var file = File.Open("thousandquestions.model", FileMode.OpenOrCreate))
                    {
                        Serializer.Serialize<LineageModel>(file, vm);
                    }

                }*/

    }

}
