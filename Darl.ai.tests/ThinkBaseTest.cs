using Darl.Common;
using Darl.GraphQL.Models.Connectivity;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlLanguage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
    public class ThinkBaseTest
    {
        IGraphPrimitives _primitives;
        IGraphModel _model;
        Mock<IGraphPrimitives> primitives;
        Mock<IGraphModel> model;
        ILogger<GraphProcessing> _logger;
        ILogger<GraphHandler> _ghlogger;
        IMetaStructureHandler _metaStruct;
        IDarlMetaRunTime _metaRunTime;
        IConfiguration _config;
        IDataLoader _dataLoader;

        private static readonly string industryLineage = "noun:01,2,07,10,14,3,1";
        private static readonly string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";
        private static readonly string jobLineage = "noun:01,0,2,00,23,19";
        private static readonly string areaLineage = "noun:01,1,00,10,09,5";
        private static readonly string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static readonly string courseLineage = "noun:01,0,2,00,23,29,02";
        private static readonly string abilityLineage = "noun:01,0,0,04";
        private static readonly string enableLineage = "verb:013,210";
        private static readonly string ruleLineage = "noun:01,0,2,00,23,44,15";
        private static readonly string personLineage = "noun:00,2,00";
        private static readonly string universityLineage = "noun:01,2,07,10,13,7,4";
        private static readonly string learningOutcomeLineage = "noun:01,0,0,15,16,2";
        private static readonly string ownLineage = "verb:393";
        private static readonly string consistsLineage = "verb:019,031";
        private static readonly string teachLineage = "verb:034,30,01,09,01";
        private static readonly string topicLineage = "noun:01,4,05,06";
        private static readonly string skillLineage = "noun:01,0,0,04";
        private static readonly string createLineage = "verb:023";
        private static readonly string requireLineage = "verb:145";
        private static readonly string descriptionLineage = "noun:01,4,05,21,05";
        private static readonly string functionLineage = "noun:01,0,2,00,23,16,21,1";
        private static readonly string careerLineage = "noun:01,0,2,00,00,15,20,01,1";
        private static readonly string huntingLineage = "noun:01,0,2,00,23,35";
        private static readonly string personalityLineage = "noun:01,1,09";
        private static readonly string liveLineage = "adjective:7763";
        private static readonly string studentLineage = "noun:00,2,00,175,0";
        private static readonly string mathsLineage = "noun:01,0,0,15,21,0,08,02";
        private static readonly string yearLineage = "noun:01,5,03,3,045";
        private static readonly string followsLineage = "verb:534";
        private static readonly string activityLineage = "noun:01,0,2,00,23";
        private static readonly string testLineage = "noun:01,0,2,00,38,09";
        private static readonly string completeLineage = "adjective:5500";
        private static readonly string answerLineage = "noun:01,4,05,21,19";
        private static readonly string rangeLineage = "noun:01,7,03";
        private static readonly string appraisalLineage = "noun:01,0,2,00,26,4,0";
        private static readonly string textLineage = "noun:01,4,04,02,07,01";
        private string id1;
        private string id2;
        private string id3;
        private string id4;

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(a => a[It.Is<string>(s => s == "licensing:darlMetaLicense")]).Returns("RwEAAB+LCAAAAAAAAApVkEtPwzAQhO+V+h984xCEyauUyrVoHqKOkjQlUVRxc4mhLnk6sUr49UQWCDh+s7Mzq0Uhf2F1z/B8BgDKxpbhdKB1QUWBoEI18D9aLujAmxpnJ3kNdBMEsgbGrWEB3VhZ9sq4B49RhuAfp9p0ZT80FROKJo5pxbAnwKYuxqsekASEw1Sl5G+LX1Fe4l62bSOGh+mU8obyKVnJKhT+S0Upf6vpIAXDkb93iR/LT+891+y83bsmLM6tvnBOZ/J6l5Ry8WQcD2PwrBVenmsb7ljEHHfJznMO22WXdHpXenlIuN4E8SKNDMs+biNikiVrLus1gr9d8xmCP9/7AhubQj1HAQAA");
            _config = configuration.Object;
            model = new Mock<IGraphModel>();
            var modelVertices = new Dictionary<string, GraphObject>();
            var recognitionRoots = new Dictionary<string, GraphObject>();
            model.Setup(a => a.modelName).Returns("poop");
            model.Setup(a => a.vertices).Returns(modelVertices);
            model.Setup(a => a.recognitionRoots).Returns(recognitionRoots);
            model.Setup(a => a.GetLineages(It.IsAny<GraphElementType>())).Returns(new List<Darl.Lineage.LineageRecord>());
            primitives = new Mock<IGraphPrimitives>();
            var logger = new Mock<ILogger<GraphProcessing>>();
            var ghlogger = new Mock<ILogger<GraphHandler>>();
            var metaStruct = new Mock<IMetaStructureHandler>();
            _metaStruct = metaStruct.Object;
            _logger = logger.Object;
            _ghlogger = ghlogger.Object;
            primitives.Setup(a => a.Load(It.IsAny<string>())).Returns(Task.FromResult<IGraphModel>(model.Object));
            _primitives = primitives.Object;
            id1 = Guid.NewGuid().ToString();
            id2 = Guid.NewGuid().ToString();
            model.Setup(a => a.GetConnectedObjects(It.IsAny<GraphObject>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GraphObject> { new GraphObject { id = id1 }, new GraphObject { id = id2 } });
            _model = model.Object;
            _metaRunTime = new DarlMetaRunTime(_config, new MetaStructureHandler());
            _dataLoader = new  DataLoader(metaStruct.Object);
        }

        [TestMethod]
        [Ignore]
        public async Task TestGraphMLLoad()
        {
            var graph = new GraphProcessing(_primitives, _logger, _metaStruct, _dataLoader);
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.left.graphml");
            await graph.LoadGraphML("", stream, null);
        }

        [TestMethod]
        public void TestSimpleMeta()
        {
            var source = "output categorical completed {true,false};\nif any(\"\",\"\") and all(\"\",\"\") then completed will be true;";
            var tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>() }, _model);
            Assert.IsTrue(tree.HasErrors());
            //            Assert.AreEqual(4, tree.ParserMessages.Count);
            source = "output categorical completed {true,false};\nif any(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") and all(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") then completed will be true;";
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>() }, _model);
            Assert.IsFalse(tree.HasErrors());
            source = "output categorical completed {true,false};\nif any(\"noun:01,2,07,10,14,3,1\") and all(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") then completed will be true;";
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>() }, _model);
            Assert.IsTrue(tree.HasErrors());
            source = "output categorical completed {true,false};\nif any(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") and all(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") or count(\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\",\"noun:01,2,07,10,14,3,1\") is = 3 then completed will be true;";
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>() }, _model);
            Assert.IsFalse(tree.HasErrors());
        }

        /// <summary>
        /// Test the three meta operators all any and count
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMetaEvaluation()
        {
            var topicCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if all(\"{yearLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and all(\"{yearLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            var tree = _metaRunTime.CreateTree(topicCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString() }, _model);
            var gos = _metaRunTime.ExploreGraph(tree);
            Assert.AreEqual(2, gos.Count);
            var ks = new KnowledgeState();
            var values = new List<DarlResult>();
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsTrue(values[0].IsUnknown());//two matching both empty
            ks.AddAttribute(id1, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsTrue(values[0].IsUnknown());//one matching one empty
            ks.AddAttribute(id2, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsFalse(values[0].IsUnknown());//both matching
            Assert.AreEqual(3, ks.RecordCount);
            //now consider any
            topicCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if any(\"{yearLineage}\",\"{consistsLineage}\",\"{completeLineage}\") and any(\"{yearLineage}\",\"{followsLineage}\",\"{completeLineage}\") then completed will be true;";
            tree = _metaRunTime.CreateTree(topicCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString() }, _model);
            ks = new KnowledgeState();
            values = new List<DarlResult>();
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsTrue(values[0].IsUnknown());//two matching both empty
            ks.AddAttribute(id1, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsFalse(values[0].IsUnknown());//one matching one empty
            Assert.AreEqual(2, ks.RecordCount);
            ks.AddAttribute(id2, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsFalse(values[0].IsUnknown());//both matching
            Assert.AreEqual(3, ks.RecordCount);
            //now test count operator
            topicCode = $"output categorical completed {{true,false}} \"{completeLineage}\";\n if count(\"{yearLineage}\",\"{consistsLineage}\",\"{completeLineage}\") is > 1 then completed will be true;";
            tree = _metaRunTime.CreateTree(topicCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString() }, _model);
            ks = new KnowledgeState();
            values = new List<DarlResult>();
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsTrue(values[0].IsUnknown());//two matching both empty   
            Assert.AreEqual(0, ks.RecordCount);
            ks.AddAttribute(id1, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsTrue(values[0].IsUnknown());//one matching     
            Assert.AreEqual(1, ks.RecordCount);
            ks.AddAttribute(id2, new GraphAttribute { lineage = completeLineage, confidence = 1.0 });
            await _metaRunTime.Evaluate(tree, values, ks);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("completed", values[0].name);
            Assert.IsFalse(values[0].IsUnknown());//both matching
            Assert.AreEqual(3, ks.RecordCount);
        }

        [TestMethod]
        public void TestSeekGrammar()
        {
            var seekCode = $"output categorical fred;\n output network completed \"id\" \"{completeLineage}\";\n if anything then completed will be seek(\"{followsLineage}\", \"{consistsLineage}\");";
            var tree = _metaRunTime.CreateTree(seekCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString() }, _model);

        }

        [TestMethod]
        public async Task TestAttributeGrammar()
        {
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "poop", dataType = DarlVar.DataType.textual });
            var attributeCode = $"output textual response;\n if anything then response will be attribute(\"{followsLineage}\");";
            var tree = _metaRunTime.CreateTree(attributeCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = followsLineage, value = "poop" } } }, _model);
            var list = new List<DarlResult>();
            var ks = new KnowledgeState();
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("poop", list[0].Value);
            //test using string literal for attribute
            attributeCode = $"lineage follow \"{followsLineage}\";\n output textual response;\n if anything then response will be attribute(follow);";
            tree = _metaRunTime.CreateTree(attributeCode, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = followsLineage, value = "poop" } } }, _model);
            list = new List<DarlResult>();
            ks = new KnowledgeState();
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("poop", list[0].Value);
        }

        [TestMethod]
        public async Task TestGraphPass()
        {
            var dataLoader = new DataLoader(_metaStruct);
            var graph = new GraphProcessing(_primitives, _logger, _metaStruct, dataLoader);
            var gh = new GraphHandler(_config, graph, _ghlogger, new MetaStructureHandler());
            var graphName = "graph1.graph";
            var paths = new List<string> { consistsLineage, followsLineage };
            var subjectId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var targetId = Guid.NewGuid().ToString();
            var completionLineage = completeLineage;
            var ks = new KnowledgeState();
            var next = await gh.GraphPass(ks,userId, graphName, subjectId, targetId, paths, completionLineage, new List<DarlCommon.DarlVar>(), null, GraphProcess.seek);
            Assert.AreEqual(1, next.Item1.Count);
        }

        [TestMethod]
        public async Task TestTextRecognition()
        {
            var dataLoader = new DataLoader(_metaStruct);
            var graph = new GraphProcessing(_primitives, _logger, _metaStruct, dataLoader);
            //create a simple recognition tree
            var recognitionIds = new Dictionary<string, GraphObject>();
            var defaultRule = "output textual response;\nif anything then response will be \"I don't know the answer to that.\";";
            var root = new GraphObject { id = Guid.NewGuid().ToString() };
            recognitionIds.Add(root.id, root);
            var subnode = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "default:", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = defaultRule } } };
            recognitionIds.Add(subnode.id, subnode);
            var conn = new GraphConnection { lineage = followsLineage, startId = root.id, endId = subnode.id };
            root.Out.Add(conn);
            subnode.In.Add(conn);
            subnode = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "who" };
            recognitionIds.Add(subnode.id, subnode);
            conn = new GraphConnection { lineage = followsLineage, startId = root.id, endId = subnode.id };
            root.Out.Add(conn);
            subnode.In.Add(conn);
            var subnode2 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "is" };
            recognitionIds.Add(subnode2.id, subnode2);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode.id, endId = subnode2.id };
            subnode.Out.Add(conn);
            subnode2.In.Add(conn);
            var subnode4 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "noun:00,2,00,029,12,06" };
            recognitionIds.Add(subnode4.id, subnode4);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode2.id, endId = subnode4.id };
            subnode2.Out.Add(conn);
            subnode4.In.Add(conn);
            var subnode5 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "dr" };
            recognitionIds.Add(subnode5.id, subnode5);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode2.id, endId = subnode5.id };
            subnode2.Out.Add(conn);
            subnode5.In.Add(conn);
            var subnode6 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "doc" };
            recognitionIds.Add(subnode6.id, subnode6);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode2.id, endId = subnode6.id };
            subnode2.Out.Add(conn);
            subnode6.In.Add(conn);
            var foundRule = "output textual response;\nif anything then response will be \"A complete prick.\";";
            var subnode3 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "andy", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = foundRule } } };
            recognitionIds.Add(subnode3.id, subnode3);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode4.id, endId = subnode3.id };
            subnode4.Out.Add(conn);
            subnode3.In.Add(conn);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode5.id, endId = subnode3.id };
            subnode5.Out.Add(conn);
            subnode3.In.Add(conn);
            conn = new GraphConnection { lineage = followsLineage, startId = subnode6.id, endId = subnode3.id };
            subnode6.Out.Add(conn);
            subnode3.In.Add(conn);
            var helloRule = "output textual response;\nif anything then response will be randomtext(\"hello, can I help ? \", \"hi, what can I do for you ? \");";
            var subnode7 = new GraphObject { id = Guid.NewGuid().ToString(), lineage = "noun:01,4,05,11,03", properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = helloRule } } };
            recognitionIds.Add(subnode7.id, subnode7);
            conn = new GraphConnection { lineage = followsLineage, startId = root.id, endId = subnode7.id };
            root.Out.Add(conn);
            subnode7.In.Add(conn);
            var gh = new GraphHandler(_config, graph, _ghlogger, new MetaStructureHandler());
            var graphName = "graph1.graph";
            var paths = new List<string> { consistsLineage, followsLineage };
            var subjectId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var targetId = Guid.NewGuid().ToString();
            var completionLineage = completeLineage;
            _model.recognitionRoots.Add(subjectId, root);
            model.SetupGet(a => a.recognitionVertices).Returns(recognitionIds);
            var results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "who is doctor andy" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(foundRule, results[0].darl);
            Assert.AreEqual("A complete prick.", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "who is doctor poops" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("I don't know the answer to that.", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "who is doc andy" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(foundRule, results[0].darl);
            Assert.AreEqual("A complete prick.", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "For fuck's sake, who is dr andy" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(foundRule, results[0].darl);
            Assert.AreEqual("A complete prick.", results[0].response.Value);
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "hello" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(helloRule, results[0].darl);
            //Now test the seek operator
            var nodeId = "1b35bb45-930a-4331-8421-d1c95f7a0bf7";
            var mathRule = $"output network completed \"{nodeId}\" \"{completeLineage}\";\n if anything then completed will be seek(\"{followsLineage}\", \"{consistsLineage}\");";
            //put in place of the hello rule
            subnode7.properties = new List<GraphAttribute> { new GraphAttribute { lineage = GraphObject.recognizedLineage, value = mathRule } };
            results = await gh.InterpretText(userId, graphName, subjectId, new DarlCommon.DarlVar { dataType = DarlCommon.DarlVar.DataType.textual, Value = "hello" });

        }

        [TestMethod]
        public async Task TestDisplayRules()
        {
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:00,1,00,3,10,09,06", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "Do something pointless", dataType = DarlVar.DataType.textual });
            var source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Activity_display_rule.darl")).ReadToEnd();
            var tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = "noun:00,1,00,3,10,09,06", value = "Do something pointless" } } }, _model);
            var list = new List<DarlResult>();
            var ks = new KnowledgeState();
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Do something pointless", list[1].Value);
            var c = _metaRunTime.CalculateSaliences(list, tree);
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("response", c.Keys.First());
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:00,1,00,3,10,09,06", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "What is 2 + 2?", dataType = DarlVar.DataType.textual });
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:01,4,05,21,19", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "4", dataType = DarlVar.DataType.numeric });
            source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.numeric_test_display_rule.darl")).ReadToEnd();
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = "noun:00,1,00,3,10,09,06", value = "What is 2 + 2?" }, new GraphAttribute { name = "noun:01,4,05,21,19", value = "4" } } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(4, list.Count);
            c = _metaRunTime.CalculateSaliences(list, tree);
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("response", c.Keys.First());
            list.Add(new DarlResult("response", 4));
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(4.0, list[0].Value);
            Assert.AreEqual("true", list[1].Value);
            Assert.AreEqual("What is 2 + 2?", list[2].Value);
            Assert.AreEqual("true", list[3].Value);
            Assert.AreEqual(4.0, list[4].Value);
            c = _metaRunTime.CalculateSaliences(list, tree);
            Assert.AreEqual(0, c.Count);
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:00,1,00,3,10,09,06", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "what should you do if you want to know the number of things?", dataType = DarlVar.DataType.textual });
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:01,4,05,21,19", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "count", dataType = DarlVar.DataType.textual });
            source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Textual_test_displayRule.darl")).ReadToEnd();
            list.Clear();
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = "noun:00,1,00,3,10,09,06", value = "what should you do if you want to know the number of things?" }, new GraphAttribute { name = "noun:01,4,05,21,19", value = "I should count them." } } }, _model);
            list.Add(new DarlResult("response", DarlResult.DataType.textual, 1.0));
            list[0].stringConstant = "I should count";
            list[0].Value = "I should count";
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("I should count", list[0].Value);
            Assert.AreEqual("true", list[1].Value);
            Assert.AreEqual("what should you do if you want to know the number of things?", list[2].Value);
            Assert.AreEqual("true", list[3].Value);
            Assert.AreEqual("I should count", list[4].Value);
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:01,7,03", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "\"30\",\"39\",\"40\",\"41\"", dataType = DarlVar.DataType.textual });
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:00,1,00,3,10,09,06", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "what is 40 - 1?", dataType = DarlVar.DataType.textual });
            model.Setup(a => a.FindDataAttribute(It.IsAny<string>(), "noun:01,4,05,21,19", It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "39", dataType = DarlVar.DataType.categorical });
            source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Categorical_test_displayRule.darl")).ReadToEnd();
            list.Clear();
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = "noun:00,1,00,3,10,09,06", value = "what should you do if you want to know the number of things?" }, new GraphAttribute { name = "noun:01,4,05,21,19", value = "I should count them." } } }, _model);
            list.Add(new DarlResult("response", DarlResult.DataType.categorical, 1.0));
            list[0].Value = "39";
            list[0].categories = new Dictionary<string, double> { { "39", 1.0 } };
            var inputs = tree.GetInputs();
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("39", list[0].Value);
            Assert.AreEqual("true", list[1].Value);
            Assert.AreEqual("true", list[3].Value);
            source = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Categorical_test_displayRuleConstants.darl")).ReadToEnd();
            list.Clear();
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), properties = new List<GraphAttribute> { new GraphAttribute { name = "noun:00,1,00,3,10,09,06", value = "what should you do if you want to know the number of things?" }, new GraphAttribute { name = "noun:01,4,05,21,19", value = "I should count them." } } }, _model);
            list.Add(new DarlResult("response", DarlResult.DataType.categorical, 1.0));
            list[0].Value = "39";
            list[0].categories = new Dictionary<string, double> { { "39", 1.0 } };
            inputs = tree.GetInputs();
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("39", list[0].Value);
            Assert.AreEqual("true", list[1].Value);
            Assert.AreEqual("true", list[3].Value);

        }

        [TestMethod]
        public async Task TestExists()
        {
            var source = "output categorical x {true,false};if exists() then x will be true; if not exists() then x will be false;";
            var list = new List<DarlResult>();
            var ks = new KnowledgeState();
            var tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), existence = new List<DarlTime?> { DarlTime.MinValue, DarlTime.MaxValue } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual("true", list[0].Value);
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), existence = new List<DarlTime?> { } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(true, list[0].IsUnknown());
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), existence = new List<DarlTime?> { new DarlTime(2030, 1, 1), new DarlTime(2035, 1, 1) } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            //test with set currentTime
            source = "output categorical x {true,false}; duration lifetime 1900 ; if durationof() is < lifetime  then x will be true; if durationof() is > lifetime then x will be false;";
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), existence = new List<DarlTime?> { new DarlTime(2030, 1, 1), new DarlTime(2037, 1, 1) } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual("false", list[0].Value);
            tree = _metaRunTime.CreateTree(source, new GraphObject { lineage = mathsLineage, In = new List<GraphConnection>(), id = Guid.NewGuid().ToString(), existence = new List<DarlTime?> { new DarlTime(2030, 1, 1), new DarlTime(2032, 1, 1) } }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual("true", list[0].Value);
        }
        [TestMethod]
        public async Task TestAggregations()
        {
            id3 = Guid.NewGuid().ToString();
            id4 = Guid.NewGuid().ToString();
            model.Setup(a => a.GetConnectedObjects(It.IsAny<GraphObject>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GraphObject> { new GraphObject { id = id1 }, new GraphObject { id = id2 }, new GraphObject { id = id3 }, new GraphObject { id = id4 } });
            model.Setup(a => a.FindDataAttribute(id1, It.IsAny<string>(), It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "2", dataType = DarlVar.DataType.numeric });
            model.Setup(a => a.FindDataAttribute(id2, It.IsAny<string>(), It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "3", dataType = DarlVar.DataType.numeric });
            model.Setup(a => a.FindDataAttribute(id3, It.IsAny<string>(), It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "4", dataType = DarlVar.DataType.numeric });
            model.Setup(a => a.FindDataAttribute(id4, It.IsAny<string>(), It.IsAny<KnowledgeState>())).Returns(new DarlVar { Value = "5", dataType = DarlVar.DataType.numeric });
            var source = "output numeric nsum; \n" +
                "if anything then nsum will be sum(attributes(\"noun:01\",\"noun:01\",\"noun:01\"));" +
                "\noutput numeric nproduct;\n " +
                "if anything then nproduct will be product(attributes(\"noun:01\",\"noun:01\",\"noun:01\"));\n" +
                "output numeric nmin;\n  " +
                "if anything then nmin will be minimum(attributes(\"noun:01\",\"noun:01\",\"noun:01\"));\n" +
                "output numeric nmax;\n  " +
                "if anything then nmax will be maximum(attributes(\"noun:01\",\"noun:01\",\"noun:01\"));\n";
            var list = new List<DarlResult>();
            var ks = new KnowledgeState();
            var tree = _metaRunTime.CreateTree(source, new GraphObject { id = Guid.NewGuid().ToString() }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(14.0, list[0].Value);
            Assert.AreEqual(120.0, list[1].Value);
            Assert.AreEqual(2.0, list[2].Value);
            Assert.AreEqual(5.0, list[3].Value);
            source = "output numeric nsum; \n" +
                "if anything then nsum will be sum(2,3,4,5);" +
                "\noutput numeric nproduct;\n " +
                "if anything then nproduct will be product(2,3,4,5);\n" +
                "output numeric nmin;\n  " +
                "if anything then nmin will be minimum(2,3,4,5);\n" +
                "output numeric nmax;\n  " +
                "if anything then nmax will be maximum(2,3,4,5);\n";
            list = new List<DarlResult>();
            ks = new KnowledgeState();
            tree = _metaRunTime.CreateTree(source, new GraphObject { id = Guid.NewGuid().ToString() }, _model);
            await _metaRunTime.Evaluate(tree, list, ks);
            Assert.AreEqual(14.0, list[0].Value);
            Assert.AreEqual(120.0, list[1].Value);
            Assert.AreEqual(2.0, list[2].Value);
            Assert.AreEqual(5.0, list[3].Value);
        }

        [TestMethod]
        public async Task TestMetaStructure()
        {
            var msh = new MetaStructureHandler();
            var model = new BlobGraphContent();
            var centre = new GraphObject { name = "centre", externalId = "centre", lineage = msh.CommonLineages["appraisal"], id = Guid.NewGuid().ToString() };
            centre.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "That's all we want to know." } };
            model.vertices.Add(centre.id, centre);
            var catNode = new GraphObject { name = "catNode", externalId = "catNode", lineage = msh.CommonLineages["appraisal"], id = Guid.NewGuid().ToString() };
            catNode.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "What category are you?" } };
            model.vertices.Add(catNode.id, catNode);
            var numNode = new GraphObject { name = "numNode", externalId = "numNode", lineage = msh.CommonLineages["appraisal"], id = Guid.NewGuid().ToString() };
            numNode.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "What number are you?" } };
            model.vertices.Add(numNode.id, numNode);
            var textNode = new GraphObject { name = "textNode", externalId = "textNode", lineage = msh.CommonLineages["appraisal"], id = Guid.NewGuid().ToString() };
            textNode.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "Give some text" } };
            model.vertices.Add(textNode.id, textNode);
            var conn1 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = centre.id, endId = catNode.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn1.id, conn1);
            centre.Out.Add(conn1);
            catNode.In.Add(conn1);
            var conn2 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = centre.id, endId = numNode.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn2.id, conn2);
            centre.Out.Add(conn2);
            numNode.In.Add(conn2);
            var conn3 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = centre.id, endId = textNode.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn3.id, conn3);
            centre.Out.Add(conn3);
            textNode.In.Add(conn3);
            var category1 = new GraphObject { name = "category1", externalId = "category1", lineage = msh.CommonLineages["category"], id = Guid.NewGuid().ToString() };
            category1.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "first category" }, new GraphAttribute { lineage = answerLineage, value = "first" } };
            model.vertices.Add(category1.id, category1);
            var category2 = new GraphObject { name = "category2", externalId = "category2", lineage = msh.CommonLineages["category"], id = Guid.NewGuid().ToString() };
            category2.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "second category" }, new GraphAttribute { lineage = answerLineage, value = "second" } };
            model.vertices.Add(category2.id, category2);
            var category3 = new GraphObject { name = "category3", externalId = "category3", lineage = msh.CommonLineages["category"], id = Guid.NewGuid().ToString() };
            category3.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["text"], value = "third category" }, new GraphAttribute { lineage = answerLineage, value = "third" } };
            model.vertices.Add(category3.id, category3);
            var conn4 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = catNode.id, endId = category1.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn4.id, conn4);
            catNode.Out.Add(conn4);
            category1.In.Add(conn4);
            var conn5 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = catNode.id, endId = category2.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn5.id, conn5);
            catNode.Out.Add(conn5);
            category2.In.Add(conn5);
            var conn6 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = catNode.id, endId = category3.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn6.id, conn6);
            catNode.Out.Add(conn6);
            category3.In.Add(conn6);
            var number1 = new GraphObject { name = "number1", externalId = "number2", lineage = msh.CommonLineages["number"], id = Guid.NewGuid().ToString() };
            model.vertices.Add(number1.id, number1);
            number1.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["number"], value = "16" } };
            var conn7 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = numNode.id, endId = number1.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn7.id, conn7);
            numNode.Out.Add(conn7);
            number1.In.Add(conn7);
            var number2 = new GraphObject { name = "number2", externalId = "number2", lineage = msh.CommonLineages["number"], id = Guid.NewGuid().ToString() };
            number2.properties = new List<GraphAttribute> { new GraphAttribute { lineage = msh.CommonLineages["number"], value = "70" } };
            model.vertices.Add(number2.id, number2);
            var conn8 = new GraphConnection { id = Guid.NewGuid().ToString(), startId = numNode.id, endId = number2.id, lineage = msh.CommonLineages["consist"] };
            model.edges.Add(conn8.id, conn8);
            numNode.Out.Add(conn8);
            number2.In.Add(conn8);
            var res = msh.AggregateChildren(catNode, model, msh.CommonLineages["consist"]);
            Assert.IsNotNull(res.Item2);
            Assert.AreEqual(DarlVar.DataType.categorical, res.Item1.dataType);
            Assert.AreEqual(DarlVar.DataType.categorical, res.Item2.response.dataType);
            Assert.AreEqual("What category are you?", res.Item2.response.Value);
            Assert.AreEqual(3, res.Item2.response.categories.Count);
            res = msh.AggregateChildren(numNode, model, msh.CommonLineages["consist"]);
            Assert.AreEqual(DarlVar.DataType.numeric, res.Item1.dataType);
            Assert.AreEqual(DarlVar.DataType.numeric, res.Item2.response.dataType);
            Assert.AreEqual("What number are you?", res.Item2.response.Value);
            Assert.AreEqual(2, res.Item2.response.values.Count);
        }

        /// <summary>
        /// tests for a bug in recognizing category literals
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void TestCategoryIdentifiers()
        {
            var source = " input categorical analytic {true,false};\n" +
            "input categorical rapid_change { true,false};\n" +
            "input categorical humans_do_it { true,false};\n" +
            "input categorical data_type { vectors,short_text,long_text,images,other};\n" +
            "input categorical examples { io,i,sys,assoc,other};\n" +
            "input categorical transparency { true, false};\n" +
            "input textual email;\n" +
            "input textual name;\n" +
            "output categorical type { analytical,expert,super,super_whitebox,unsuper,critic_whitebox,critic,nlp,other,assoc,assoc_whitebox,super_nlp};\n" +
            "output textual finished;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is true and rapid_change is false then type will be analytical;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is true and rapid_change is true then type will be expert;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is true then type will be expert;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is io and (transparency is false or data_type is images) then type will be super;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is io and transparency is true then type will be super_whitebox;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is i and transparency is false then type will be unsuper;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is i and transparency is true then type will be unsuper_whitebox;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is sys and transparency is false then type will be critic;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is sys and transparency is true then type will be critic_whitebox;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is assoc and transparency is false then type will be assoc;\n" +
            "if (data_type is vectors or data_type is short_text) and analytic is false and humans_do_it is false and examples is assoc and transparency is true then type will be assoc_whitebox;\n" +
            "if data_type is long_text and analytic is false and examples is io and transparency is true then type will be nlp;\n" +
            "if data_type is long_text and analytic is false and examples is io and transparency is false then type will be super_nlp;\n" +
            "if data_type is images and examples is io then type will be super;\n" +
            "if data_type is images and not examples is io then type will be other;\n" +
            "if data_type is long_text and not examples is io then type will be other;\n" +
            "otherwise if analytic is false and humans_do_it is false and (examples is other or data_type is other) then type will be other;\n";
            var darlruntime = new DarlRunTime();
            var darlTree = darlruntime.CreateTree("ruleset ai_triage {\n" + source + "\n}");
            var tree = _metaRunTime.CreateTree(source, new GraphObject(), null);
        }

        [TestMethod]
        public async Task TestCompletionRuleCreation()
        {
            var model = Serializer.Deserialize<BlobGraphContent>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.primary_math.graph"));
            var root = model.vertices.FirstOrDefault(a => a.Value.externalId == "MATH1");
            var msh = new MetaStructureHandler();
            var choices = msh.CreateCompletionRuleFirstPass(model, root.Value);
            var paths = new List<(string, string, string)> { (choices[0].Item1, choices[0].Item2, "all") };
            var code = msh.CreateCompletionRuleSecondPass(model, root.Value, paths, "and");
            var tree = _metaRunTime.CreateTree(code, root.Value, model);
            var linroot = model.virtualVertices["noun:01,0,0,15,21,0,08,02"];
            choices = msh.CreateCompletionRuleFirstPass(model, linroot);
            paths = new List<(string, string, string)>();
            foreach (var ch in choices)
            {
                paths.Add((ch.Item1, ch.Item2, "all"));
            }
            code = msh.CreateCompletionRuleSecondPass(model, root.Value, paths, "and");
            tree = _metaRunTime.CreateTree(code, root.Value, model);
            var ks = new KnowledgeState();
            var inputs = new List<DarlResult>();
            await _metaRunTime.Evaluate(tree, inputs, ks);
            Assert.AreEqual(1, inputs.Count);
            Assert.IsTrue(inputs[0].IsUnknown());
            Assert.AreEqual(0, ks.data.Count);
            //now repeat for any and add a single completion to the ks.
            choices = msh.CreateCompletionRuleFirstPass(model, linroot);
            paths = new List<(string, string, string)>();
            foreach (var ch in choices)
            {
                paths.Add((ch.Item1, ch.Item2, "any"));
            }
            code = msh.CreateCompletionRuleSecondPass(model, root.Value, paths, "or");
            tree = _metaRunTime.CreateTree(code, root.Value, model);
            ks = new KnowledgeState();
            ks.AddAttribute("1b35bb45-930a-4331-8421-d1c95f7a0bf7", new GraphAttribute { confidence = 1.0, lineage = completeLineage, name = "completed", value = "true" });
            inputs = new List<DarlResult>();

            await _metaRunTime.Evaluate(tree, inputs, ks);
            Assert.AreEqual(1, inputs.Count);
            Assert.IsFalse(inputs[0].IsUnknown());
            Assert.AreEqual(2, ks.data.Count);

            linroot = model.virtualVertices["noun:01,0,2,00,39,08,08,1"]; //question
            choices = msh.CreateCompletionRuleFirstPass(model, linroot);
            paths = new List<(string, string, string)>();
            foreach (var ch in choices)
            {
                paths.Add((ch.Item1, ch.Item2, "all"));
            }
            code = msh.CreateCompletionRuleSecondPass(model, root.Value, paths, "and");
            tree = _metaRunTime.CreateTree(code, root.Value, model);

        }

        [TestMethod]
        public void TestLinterforLineageConstants()
        {
            var source = "lineage correct \"adjective:3521\";\n" +
                            "input numeric response;\n" +
                            "output categorical completed { true,false} complete;\n" +
                            "output textual annotation;\n" +
                            "output categorical correct { true,false} correct;\n" +
                            "output numeric answer answer;\n" +
                            "if response is = attribute(answer) then correct will be true;\n" +
                            "if anything then answer will be response;\n" +
                            "if response is present then completed will be true;\n" +
                            "if anything then annotation will be attribute(textt); ";
            var res = _metaRunTime.CreateTreeEdit(source);
            Assert.AreEqual("Lineage constant textt not defined.", res.ParserMessages[0].Message);
        }

        [TestMethod]
        public async Task TestTextParsingAndSalience()
        {
            var model = Serializer.Deserialize<BlobGraphContent>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.discord_bot.graph"));
            primitives = new Mock<IGraphPrimitives>();
            primitives.Setup(a => a.Load(It.IsAny<string>())).Returns(Task.FromResult<IGraphModel>(model));
            model.recognitionRoots.Add("conversation", model.recognitionRoots["default:"]);
            _primitives = primitives.Object;
            var meta = new MetaStructureHandler();
            var dataLoader = new DataLoader(meta);
            var gp = new GraphProcessing(_primitives, _logger, meta, dataLoader);
            var gh = new GraphHandler(_config, gp, _ghlogger, new MetaStructureHandler());
            var res = await gh.InterpretText("user", "discord_bot.graph", "conversation", new DarlVar { dataType = DarlVar.DataType.textual, name = "text", Value = "who is andy" });
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("Dr Andy is the inventor of ThinkBase.", res[0].response.Value);
            res = await gh.InterpretText("user", "discord_bot.graph", "conversation", new DarlVar { dataType = DarlVar.DataType.textual, name = "text", Value = "who is dr andy" });
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("Dr Andy is the inventor of ThinkBase.", res[0].response.Value);
            res = await gh.InterpretText("user", "discord_bot.graph", "conversation", new DarlVar { dataType = DarlVar.DataType.textual, name = "text", Value = "who is that fucking idiot dr andy" });
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("Dr Andy is the inventor of ThinkBase.", res[0].response.Value);
            res = await gh.InterpretText("user", "discord_bot.graph", "conversation", new DarlVar { dataType = DarlVar.DataType.textual, name = "text", Value = "I don't know who that fucking turd  is called  dr andy" });
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("Dr Andy is the inventor of ThinkBase.", res[0].response.Value);
            res = await gh.InterpretText("user", "discord_bot.graph", "conversation", new DarlVar { dataType = DarlVar.DataType.textual, name = "text", Value = "quote" });
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("We'd like to build a chatbot for you. Answer the following to get an initial cost estimate.", res[0].response.Value);
            Assert.AreEqual(3, res[1].response.sequence.Count);
            Assert.AreEqual("bot_quote", res[1].response.sequence[0][0]);
            Assert.AreEqual("verb:019,031", res[1].response.sequence[1][0]);
            Assert.AreEqual("adjective:5500", res[1].response.sequence[2][0]);
            var ks = new KnowledgeState();
            primitives.Setup(a => a.GetKnowledgeState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(ks));
            var res2 = await gh.GraphPass(ks,"user", "discord_bot.graph", "conversation", res[1].response.sequence[0][0], res[1].response.sequence[1], res[1].response.sequence[2][0], new List<DarlVar>(), null, GraphProcess.seek);
        }

        [TestMethod]
        public async Task TestCursusHonorum()
        {
            var model = Serializer.Deserialize<BlobGraphContent>(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cursus_honorum.graph"));
            primitives = new Mock<IGraphPrimitives>();
            primitives.Setup(a => a.Load(It.IsAny<string>())).Returns(Task.FromResult<IGraphModel>(model));
//            primitives.Setup(a => a.GetRecognitionRoot(It.IsAny<IGraphModel>(), It.IsAny<string>())).Returns(Task.FromResult<GraphObject>(model.recognitionRoots["default:"]));
//            primitives.Setup(a => a.GetGraphObjectById(It.IsAny<string>(), It.IsAny<string>())).Returns((string compName, string id) => Task.FromResult<GraphObject>(model.vertices.FirstOrDefault(a => a.Value.externalId == id).Value));
            var ks = new KnowledgeState { subjectId = "person" };
            primitives.Setup(a => a.GetKnowledgeState(It.IsAny<string>(), "person", It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(ks));
            _primitives = primitives.Object;
            var meta = new MetaStructureHandler();
            var dataLoader = new DataLoader(meta);
            var gp = new GraphProcessing(_primitives, _logger, meta, dataLoader);
            var gh = new GraphHandler(_config, gp, _ghlogger, new MetaStructureHandler());
            //        var res = await gh.DiscoverForBot("user", "cursus_honorum.graph", "person", new List<string>(), "abcdef");

        }

        [TestMethod]
        public void TestKnowledgeStateSerialize()
        {
            var ks = new KnowledgeState 
            {
                created = DateTime.Now,
                subjectId = Guid.NewGuid().ToString(),
                knowledgeGraphName = "test.graph",
                userId = Guid.NewGuid().ToString(),
                processId = Guid.NewGuid().ToString(),
            };
            ks.data.Add(Guid.NewGuid().ToString(), new List<GraphAttribute> { 
                new GraphAttribute { 
                    lineage = abilityLineage,
                    type = GraphAttribute.DataType.textual,
                    value = "poops"
                }, 
                new GraphAttribute {
                    lineage = abilityLineage,
                    type = GraphAttribute.DataType.numeric,
                    value = "22"
                } 
            });
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<KnowledgeState>(ms, ks);
                ms.Position = 0;
                buffer =  ms.ToArray();
            }
            KnowledgeState returnedKS;
            using (var ms = new MemoryStream(buffer))
            {
                ms.Position = 0;
                returnedKS =  Serializer.Deserialize<KnowledgeState>(ms);
            }
            Assert.IsNotNull(returnedKS);
            Assert.AreEqual(ks.knowledgeGraphName, returnedKS.knowledgeGraphName);
            Assert.AreEqual(ks.userId, returnedKS.userId);
            Assert.AreEqual(ks.processId, returnedKS.processId);
            Assert.AreEqual(ks.subjectId, returnedKS.subjectId);
            Assert.AreEqual(ks.data.Count, returnedKS.data.Count);
            var index = ks.data.Keys.First();
            Assert.AreEqual(ks.data[index].Count, returnedKS.data[index].Count);
            Assert.AreEqual(ks.data[index][0].lineage, returnedKS.data[index][0].lineage);
            Assert.AreEqual(ks.data[index][0].type, returnedKS.data[index][0].type);
            Assert.AreEqual(ks.data[index][0].value, returnedKS.data[index][0].value);
        }



    }
}
