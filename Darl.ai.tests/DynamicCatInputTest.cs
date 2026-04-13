/// <summary>
/// DynamicCatInputTest.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Forms;
using Darl.Lineage.Bot.Stores;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DynamicCatInputTest
    {
        ILocalStore _graph;
        ILocalStore _user;

        [TestInitialize()]
        public void Initialize()
        {
            var gp = new Mock<ILocalStore>();
            var testCats = new DarlResult(DarlResult.DataType.categorical, 1.0);
            testCats.categories = new Dictionary<string, double> { { "J1%%bum%%", 1 }, { "J2%%poop%%", 1 }, { "J3%%fart%%", 1 }, { "J4%%wee%%", 1 } };
            var testText = new DarlResult("att", "text", DarlResult.DataType.textual);
            gp.Setup(a => a.ReadAsync(It.Is<List<string>>(i => i[0] == "categories"))).Returns(Task.FromResult(testCats));
            gp.Setup(a => a.ReadAsync(It.Is<List<string>>(i => i[0] == "attribute"))).Returns(Task.FromResult(testText));
            _graph = gp.Object;
            var userStore = new Dictionary<string, string> { { "artist", "334" } };
            _user = new BotDataStore(new LocalBotData(userStore));
        }

        [TestMethod]
        public async Task TestDynamicCatInputEvaluate()
        {
            //create a ruleset file
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.dynamiccategoricalinput.darl"));
            var source = reader.ReadToEnd();
            var ruleForm = await FormFormatExtensions.CreateNewRuleForm("dyncat.rule", "Dr Andy's IP");
            ruleForm.darl = source;
            var stores = new Dictionary<string, DarlLanguage.Processing.ILocalStore> { { "Graph", _graph } };
            //            await ruleForm.UpdateFromCode(stores);
            //            Assert.AreEqual("dyn.J1", ruleForm.language.LanguageList[1].Name);
            //            Assert.AreEqual("bum", ruleForm.language.LanguageList[1].Text);
            DarlForms form = new DarlForms();
            QuestionCache qc = new QuestionCache { stores = stores };
            var qsp = await form.Start(ruleForm, qc);
            Assert.AreEqual("bum", qsp.questions[0].categories[0]);
            qsp.questions[0].sResponse = qsp.questions[0].categories[1]; // poop
            qsp = await form.Next(qsp, ruleForm, qc);
            Assert.AreEqual("Choice", qsp.responses[0].annotation);
            Assert.AreEqual("true", qsp.responses[0].mainText);
        }

        [TestMethod]
        public async Task TestDynamicCatInputWithEmbeddedStore()
        {
            var source = "ruleset grateful_dead_artists\n{\n store UserData;\n store Graph;\n input dynamic categorical song Graph[\"categories\", UserData[\"artist\"], \"noun:00,2,00,015,01\", \"name\"];\n if song is present then UserData[\"song\"] will be song;} ";
            var ruleForm = await FormFormatExtensions.CreateNewRuleForm("song.rule", "Dr Andy's IP");
            ruleForm.darl = source;
            var stores = new Dictionary<string, DarlLanguage.Processing.ILocalStore> { { "Graph", _graph }, { "UserData", _user } };
            DarlForms form = new DarlForms();
            QuestionCache qc = new QuestionCache { stores = stores };
            var qsp = await form.Start(ruleForm, qc);
            Assert.AreEqual(4, qsp.questions[0].categories.Count);
            Assert.AreEqual("bum", qsp.questions[0].categories[0]);
        }

        [TestMethod]
        public async Task TestStoreWithEmbeddedDynCat()
        {
            var source = "ruleset grateful_dead_artists\n{\n store UserData;\n store Graph;\n store Call;\n output textual response;\n output textual performances;\n output textual songType;\n output textual songName;\n input dynamic categorical song Graph[\"categories\", UserData[\"artist\"], \"noun:01,4,14,1,10,33\", \"name\"];\n if song is present then UserData[\"song\"] will be song;\n if song is present then performances will be Graph[\"attribute\", song, \"noun:01,5,04,3,07\"];\n if song is present then songType will be Graph[\"attribute\", song, \"noun:01,0,0,15,07,02,02,0,01\"];\n  if song is present then songName will be Graph[\"attribute\", song, \"name\"];\n if song is present then response will be document(\"You selected song '%% songName %%' of type %% songType %%, performed %% performances %% times.\",\n { performances,songName,songType});\n }\n";
            var ruleForm = await FormFormatExtensions.CreateNewRuleForm("song.rule", "Dr Andy's IP");
            ruleForm.darl = source;
            var stores = new Dictionary<string, DarlLanguage.Processing.ILocalStore> { { "Graph", _graph }, { "UserData", _user } };
            DarlForms form = new DarlForms();
            QuestionCache qc = new QuestionCache { stores = stores };
            var qsp = await form.Start(ruleForm, qc);
            Assert.AreEqual(4, qsp.questions[0].categories.Count);
            Assert.AreEqual("bum", qsp.questions[0].categories[0]);
            qsp.questions[0].sResponse = qsp.questions[0].categories[1]; // poop
            qsp = await form.Next(qsp, ruleForm, qc);
            Assert.AreEqual("You selected song 'text' of type text, performed text times.", qsp.responses[0].mainText);
        }

    }
}
