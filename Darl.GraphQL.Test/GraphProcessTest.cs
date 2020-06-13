using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.SoftMatch;
using GraphQL;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class GraphProcessTest
    {
        private GraphProcessing _graph;
        private IConfiguration _config;

        private static string industryLineage = "noun:01,2,07,10,14,3,1";
        private static string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";
        private static string jobLineage = "noun:01,0,2,00,23,19";
        private static string areaLineage = "noun:01,1,00,10,09,5";
        private static string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static string courseLineage = "noun:01,0,2,00,23,29,02";
        private static string abilityLineage = "noun:01,0,0,04";
        private static string enableLineage = "verb:013,210";
        private static string ruleLineage = "noun:01,0,2,00,23,44,15";
        private static string personLineage = "noun:00,2,00";
        private static string universityLineage = "noun:01,2,07,10,13,7,4";
        private static string learningOutcomeLineage = "noun:01,0,0,15,16,2";
        private static string ownLineage = "verb:393";
        private static string consistsLineage = "verb:019,031";
        private static string teachLineage = "verb:034,30,01,09,01";
        private static string topicLineage = "noun:01,4,05,06";
        private static string skillLineage = "noun:01,0,0,04";
        private static string createLineage = "verb:023";
        private static string requireLineage = "verb:145";

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
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("4110279c-956d-4532-a31e-c8dbd5052eb3");
//            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("7d1a254f-d405-4385-acbc-308c8376f2e3");
//            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("azure");
//            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            var logger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            _graph = new GraphProcessing(configuration.Object, logger.Object, context.Object);
        }

        [TestMethod]
        [Ignore]
        public async Task CreateAndDeleteObjectTest()
        {
            var res = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = "noun:00,2,00", name = "Andrew Edmonds", firstname = "Andrew", secondname = "Edmonds", existence = new List<DateTime> { new DateTime(1955, 11, 6), DateTime.MaxValue } });
            Assert.AreEqual(res.inferred, false);
            Assert.AreEqual(res.lineage, "noun:00,2,00");
            Assert.AreEqual(res.name, "andrew edmonds");
            Assert.AreEqual(res.firstname, "andrew");
            Assert.AreEqual(res.secondname, "edmonds");
            var objlist = await _graph.GetGraphObjects(_config["userId"], "andrew edmonds", "noun:00,2,00");
            Assert.AreEqual(objlist.Count, 1);
            Assert.AreEqual(res.id, objlist[0].id);
            var obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            Assert.AreEqual(res.id, obj.id);
            Assert.AreEqual(res.name, obj.name);
            Assert.AreEqual(res.existence.Count, 2);
            Assert.AreEqual(res.existence[0], new DateTime(1955, 11, 6));
            Assert.AreEqual(res.existence[1], DateTime.MaxValue);
            //add a property
            var up = await _graph.UpdateGraphObject(_config["userId"], new GraphObjectUpdate { id = res.id, lineage = "noun:00,2,00", properties = new List<StringStringPair> { new StringStringPair("noun:01,4,09,01,3,4,5", "Andy is a bit of a dork, really.") } }, IGraphProcessing.OntologyAction.build);
            Assert.AreEqual(1, up.properties.Count);
            //add another 
            var res2 = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = "noun:00,2,00", name = "Anneke Edmonds", firstname = "Anneke", secondname = "Edmonds", existence = new List<DateTime> { new DateTime(1961, 8, 27), DateTime.MaxValue } });
            var obj2 = await _graph.GetGraphObjectById(_config["userId"], res2.id);
            Assert.AreEqual(res2.id, obj2.id);
            //add a marry link
            var conn1 = await _graph.CreateGraphConnection(_config["userId"], new GraphConnectionInput { lineage = "verb:197,6,07", existence = new List<DateTime> { new DateTime(1988, 8, 27), DateTime.MaxValue }, name = "married", startId = res.id, endId = res2.id }, IGraphProcessing.OntologyAction.build);
            Assert.AreEqual(conn1.startId, res.id);
            Assert.AreEqual(conn1.endId, res2.id);
            Assert.AreEqual(conn1.lineage, "verb:197,6,07");
            Assert.AreEqual(conn1.existence.Count, 2);
            //look for links in objects,
            obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            obj2 = await _graph.GetGraphObjectById(_config["userId"], res2.id);

            //Update conn1
            var del = await _graph.DeleteGraphObject(_config["userId"], res.id);
            var del2 = await _graph.DeleteGraphObject(_config["userId"], res2.id);
            obj = await _graph.GetGraphObjectById(_config["userId"], res.id);
            Assert.IsNull(obj);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead1()
        {
            var res = await _graph.ReadAsync(new List<string> { "text", "jeremy corbyn", "noun:00,2,00" });
            Assert.IsTrue(res.stringConstant.Length > 50);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead2()
        {

            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbyn", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(10, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead3()
        {
            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbyn", "noun:00,2,00", "noun:01,2,07,10" });
            Assert.AreEqual(15, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead4()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremy Corbyn", "Paul Mason", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead5()
        {
            var res = await _graph.ReadAsync(new List<string> { "text", "jeremy corbin", "noun:00,2,00" });
            Assert.IsTrue(res.stringConstant.Length > 50);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead6()
        {

            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremy Corbin", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(10, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead7()
        {
            var res = await _graph.ReadAsync(new List<string> { "links", "Jeremi Corbyn", "noun:00,2,00", "noun:01,2,07,10" });
            Assert.AreEqual(15, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead8()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremy Corbin", "Paul Masoni", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }

        [TestMethod]
        [Ignore]
        public async Task TestRead9()
        {
            var res = await _graph.ReadAsync(new List<string> { "path", "Jeremi Corbyn", "Pail Mason", "noun:00,2,00", "noun:00,2,00" });
            Assert.AreEqual(5, res.stringConstant.Split('\n').Length);
        }


        [TestMethod]
        [Ignore]
        public async Task NearestVertexTest()
        {
            using (var gremlinClient = new GremlinClient(_graph.ServerFactory(_config["userId"]), new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var res = await _graph.FindNearestNameVertex(gremlinClient, "noun:00,2,00", "Corbyn", 0.7f, "Jeremy");
                Assert.AreEqual(1, res.Count);
                res = await _graph.FindNearestNameVertex(gremlinClient, "noun:00,2,00", "Corbin", 0.7f, "Jeremy");
                Assert.AreEqual(1, res.Count);
                Assert.AreEqual(res[0].name, "jeremy corbyn");
            }
        }

        [TestMethod]
        [Ignore]
        public async Task TestCreateGraph()
        {
            await _graph.CreateNewGraph("7d1a254f-d405-4385-acbc-308c8376f2e3", "/lineage");
        }
        [TestMethod]
        [Ignore]
        public async Task TestInferPath()
        {
            var userId = "33db770b-29e9-46ae-8a19-c1947bd775d8";
            var startId = "bd7fab60-4657-4a05-bda6-a025f19e432a";
            var endId = "aaf09adf-9550-417a-a539-b713a32ed725";
            var end = new GraphObjectInput { name = "artist manager", lineage = "noun:01,0,2,00,23,19" };
            var start = new GraphObjectInput { name = "applicant", lineage = "noun:00,2,00" };
            var res = await _graph.InferPath(start, end, userId, "artist_manager_inferred");
            Assert.IsTrue(res.unknown);
            Assert.AreEqual(0.0, res.confidence);
            Assert.AreEqual(43, res.recommendations.Count);
            start = new GraphObjectInput
            {
                name = "applicant",
                lineage = "noun:00,2,00",
                properties = new List<StringStringPair> {
                    new StringStringPair("management_assistant_achieved","1"),
                    new StringStringPair("budgeting_achieved", "0"),
                    new StringStringPair("music_business_experience_achieved", "0"),
                    new StringStringPair("music_licensing_and_performing_rights_achieved", "0"),
                    new StringStringPair("music_publishing_achieved", "0"),
                    new StringStringPair("negotiation_achieved", "0"),
                    new StringStringPair("networking_achieved", "0"),
                    new StringStringPair("organizational_skills_achieved", "0"),
                    new StringStringPair("record_industry_contacts_achieved", "0"),
                    new StringStringPair("record_production_process_achieved", "0"),
                    new StringStringPair("schedule_management_achieved", "0"),
                    new StringStringPair("tour_planning_achieved", "0"),
                    new StringStringPair("verbal_communication_achieved", "0"),
                    new StringStringPair("local_Artist_Manager_achieved", "0")
            }
            };
            res = await _graph.InferPath(start, end, userId, "artist_manager_inferred");
            Assert.AreEqual(29, res.recommendations.Count);
            Assert.IsFalse(res.unknown);
            Assert.AreEqual(1.0, res.confidence);
            start = new GraphObjectInput
            {
                name = "applicant",
                lineage = "noun:00,2,00",
                properties = new List<StringStringPair> {
                    new StringStringPair("unit_1_achieved","1"),
                    new StringStringPair("unit_2_achieved","1"),
                    new StringStringPair("unit_3_achieved","1"),
                    new StringStringPair("unit_4_achieved","1"),
                    new StringStringPair("unit_5_achieved","1"),
                    new StringStringPair("unit_6_achieved","1"),
                    new StringStringPair("unit_7_achieved","1"),
                    new StringStringPair("unit_8_achieved","1"),
                    new StringStringPair("unit_9_achieved","1"),
                    new StringStringPair("unit_10_achieved","1"),
                    new StringStringPair("unit_11_achieved","1"),
                    new StringStringPair("unit_12_achieved","1"),
                    new StringStringPair("unit_13_achieved","1"),
                    new StringStringPair("unit_14_achieved","1"),
                    new StringStringPair("unit_15_achieved","1"),
                    new StringStringPair("unit_16_achieved","1"),
                    new StringStringPair("unit_17_achieved","1"),
                    new StringStringPair("unit_18_achieved","1"),
                    new StringStringPair("unit_19_achieved","1"),
                    new StringStringPair("unit_20_achieved","1"),
                    new StringStringPair("unit_21_achieved","1"),
                    new StringStringPair("unit_22_achieved","1"),
                    new StringStringPair("unit_23_achieved","1"),
                    new StringStringPair("unit_24_achieved","1"),
                    new StringStringPair("unit_25_achieved","1"),
                    new StringStringPair("unit_26_achieved","1"),
                    new StringStringPair("unit_27_achieved","1"),
                    new StringStringPair("unit_28_achieved","1"),
                    new StringStringPair("unit_29_achieved","1")
            }
            };
            res = await _graph.InferPath(start, end, userId, "artist_manager_inferred");
            Assert.AreEqual(0, res.recommendations.Count);
            Assert.IsFalse(res.unknown);
            Assert.AreEqual(1.0, res.confidence);
            start = new GraphObjectInput
            {
                name = "applicant",
                lineage = "noun:00,2,00",
                properties = new List<StringStringPair> {

                    new StringStringPair("unit_2_achieved","1"),
                    new StringStringPair("unit_4_achieved","1"),
                    new StringStringPair("unit_5_achieved","1"),
                    new StringStringPair("unit_6_achieved","1"),
                    new StringStringPair("unit_7_achieved","1"),
                    new StringStringPair("unit_8_achieved","1"),
                    new StringStringPair("unit_9_achieved","1"),
                    new StringStringPair("unit_10_achieved","1"),
                    new StringStringPair("unit_11_achieved","1"),
                    new StringStringPair("unit_13_achieved","1"),
                    new StringStringPair("unit_16_achieved","1"),
                    new StringStringPair("unit_17_achieved","1"),
                    new StringStringPair("unit_18_achieved","1"),
                    new StringStringPair("unit_19_achieved","1"),
                    new StringStringPair("unit_20_achieved","1"),
                    new StringStringPair("unit_21_achieved","1"),
                    new StringStringPair("unit_22_achieved","1"),
                    new StringStringPair("unit_23_achieved","1"),
                    new StringStringPair("unit_24_achieved","1"),
                    new StringStringPair("unit_25_achieved","1"),
                    new StringStringPair("unit_26_achieved","1"),
                    new StringStringPair("unit_27_achieved","1"),
                    new StringStringPair("unit_28_achieved","1"),
                    new StringStringPair("unit_29_achieved","1")
            }
            };
            res = await _graph.InferPath(start, end, userId, "artist_manager_inferred");
            Assert.AreEqual(17, res.recommendations.Count);
            Assert.IsTrue(res.unknown);
            Assert.AreEqual(0.0, res.confidence);
        }

        [TestMethod]
        public async Task TestCreateJobsAnd()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.WM_MusicIndustry.json"));
            var doc = docsource.ReadToEnd();
            var industries = JArray.Parse(doc);
//            var res = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput { lineage = industryLineage, name = "Music industry", inferred = false });
            var sectors = industries.SelectTokens("$..Sectors").First().ToList();
            foreach(var sector in sectors)
            {
                //get name
                var sectorName = sector.SelectToken("$.Name").ToString();
                //get id
                //Add an object
                var areas = sector.SelectTokens("$.Areas").First().ToList();
                foreach(var area in areas)
                {
                    var AreaName = area.SelectToken("$.Name").ToString();
                    //add an object
                    var types = area.SelectTokens("$.Types").First().ToList();
                    foreach(var t in types)
                    {
                        var typeName = t.SelectToken("$.Name").ToString();
                        //add an object
                        var roles = t.SelectTokens("$.Roles").First().ToList();
                        foreach (var r in roles)
                        {
                            if(r.HasValues)
                            { 
                                var jobName = r.SelectToken("$.Name").ToString();
                                //add an object
                                var res = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                                {
                                    lineage = jobLineage,
                                    name = jobName,
                                    properties =
                                    new List<StringStringPair>
                                    {
                                        new StringStringPair(sectorLineage,sectorName),
                                        new StringStringPair(areaLineage, AreaName),
                                        new StringStringPair(typeLineage, typeName),
                                        new StringStringPair(industryLineage, "Music industry")
                                   }
                                }, IGraphProcessing.OntologyAction.build);
                            }

                        }
                    }
                }
            }
            docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.CourseUnitsJson.json"));
            doc = docsource.ReadToEnd();
            var courses = JArray.Parse(doc);
            //add institution object
            var instRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
            {
                lineage = universityLineage,
                name = "University",
            }, IGraphProcessing.OntologyAction.build);
            var institutionId = instRes.id;   
            foreach (var course in courses)
            {
                var courseName = course.SelectToken("$.Name").ToString();
                var courseId = course.SelectToken("$.Id").ToString();
                var courseUnits = course.SelectTokens("$.CourseUnits").First().ToList();
                //add course object
                var courseRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                {
                    lineage = courseLineage,
                    name = courseName,
                    externalId = courseId
                }, IGraphProcessing.OntologyAction.build);
                var courseGraphId = courseRes.id;
                //add link back to institution
                await _graph.CreateGraphConnection(_config["userId"],
                    new GraphConnectionInput
                    {
                        startId = institutionId,
                        endId = courseGraphId,
                        name = "owns",
                        lineage = ownLineage,
                        weight = 1.0
                    },
                    IGraphProcessing.OntologyAction.ignore); ;
                foreach (var cunit in courseUnits)
                {
                    var unit = cunit.SelectToken("$.Unit");
                    var unitName = unit.SelectToken("$.Name").ToString();
                    var unitId = unit.SelectToken("$.Id").ToString();
                    //add unit object
                    var unitRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                    {
                        lineage = courseLineage,
                        name = unitName,
                        externalId = unitId
                    }, IGraphProcessing.OntologyAction.build);
                    var unitGraphId = unitRes.id;
                    //add link back to course
                    await _graph.CreateGraphConnection(_config["userId"],
                        new GraphConnectionInput
                        {
                            startId = courseGraphId,
                            endId = unitGraphId,
                            name = "consists of",
                            lineage = consistsLineage,
                            weight = 1.0
                        },
                        IGraphProcessing.OntologyAction.build);
                    //now learning outcomes
                    var learningOutcomes = unit.SelectTokens("$.UnitLearningOutcomes").First().ToList();
                    foreach(var lo in learningOutcomes)
                    {
                        //get subobject
                        var los = lo.SelectToken("$.LearningOutcome");
                        var loName = los.SelectToken("$.Name").ToString();
                        var loId = los.SelectToken("$.Id").ToString();
                        //add lo object
                        var loRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                        {
                            lineage = learningOutcomeLineage,
                            name = loName,
                            externalId = loId
                        }, IGraphProcessing.OntologyAction.build);
                        var loGraphId = loRes.id;
                        //add link back to unit
                        await _graph.CreateGraphConnection(_config["userId"],
                            new GraphConnectionInput
                            {
                                startId = unitGraphId,
                                endId = loGraphId,
                                name = "teaches",
                                lineage = teachLineage,
                                weight = 1.0
                            },
                            IGraphProcessing.OntologyAction.build);
                    }
                    //Subject topics
                    var subjectTopics = unit.SelectTokens("$.UnitSubjectTopics").First().ToList();
                    foreach (var st in subjectTopics)
                    {
                        //get subobject
                        var sts = st.SelectToken("$.SubjectTopic");
                        var stName = sts.SelectToken("$.Name").ToString();
                        var stId = sts.SelectToken("$.Id").ToString();
                        //add subject topic object
                        var stsRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                        {
                            lineage = topicLineage,
                            name = stName,
                            externalId = stId
                        }, IGraphProcessing.OntologyAction.build);
                        var stsGraphId = stsRes.id;
                        //add link back to unit
                        await _graph.CreateGraphConnection(_config["userId"],
                             new GraphConnectionInput
                             {
                                 startId = unitGraphId,
                                 endId = stsGraphId,
                                 name = "consists of",
                                 lineage = consistsLineage,
                                 weight = 1.0
                             },
                             IGraphProcessing.OntologyAction.build);
                    }
                    //transferable Skills
                    var TransferableSkill = unit.SelectTokens("$.UnitTransferableSkills").First().ToList();
                    foreach (var ts in TransferableSkill)
                    {
                        //get subobject
                        var tss = ts.SelectToken("$.TransferableSkill");
                        var stName = tss.SelectToken("$.Name").ToString();
                        var stId = tss.SelectToken("$.Id").ToString();
                        //add transferable skill object
                        var tssRes = await _graph.CreateGraphObject(_config["userId"], new GraphObjectInput
                        {
                            lineage = skillLineage,
                            name = stName,
                            externalId = stId
                        }, IGraphProcessing.OntologyAction.build);
                        var tssGraphId = tssRes.id;
                        //add link back to unit
                        await _graph.CreateGraphConnection(_config["userId"],
                             new GraphConnectionInput
                             {
                                 startId = unitGraphId,
                                 endId = tssGraphId,
                                 name = "creates",
                                 lineage = createLineage,
                                 weight = 1.0
                             },
                             IGraphProcessing.OntologyAction.build);
                    }

                }
            }
        }

        [TestMethod]
        public async Task TestSoftMatchJobs()
        {
//            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.musicbusinessworldwide.json"));
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.jobs.json"));
            var doc = docsource.ReadToEnd();
            var jobs = JArray.Parse(doc).ToList();
            var values = new List<List<string>>();
            var indexes  = new List<List<string>>();
            var filtergraph = new MatchList();
            var dict = new List<KeyValuePair<string,string>> { new KeyValuePair<string, string>("use", "skills qualifications responsibilities knowledge experience job requirements role") };
            filtergraph.CreateTree(dict);

            foreach (var job in jobs)
            {
                var jobTitle = job.SelectToken("$.Title").ToString();
                var data = job.SelectToken("$.Data");
                // The format of the data is an array of arrays of strings.
                // Each subarray has a title text which varies from  one job listing to the next. In the best of all worlds we would 
                // read these to determine the intent of the sub section. For instance, "about us" is not relevant. 
                // "previous experience" is relevant for building the "leads to" job to job relationships, but not skills, etc.
                // since these titles themselves are textual and vary from agent to agent we will need to softmatch them.
                // in this first pass we will try to match all the text items.
                var list = new List<String>();
                var sections = data.ToList();
                foreach(var section in sections)
                {
                    var filterString = section.ToObject<JProperty>().Name.ToLower();
                    try
                    {
                        if (filtergraph.Find(filterString) != null) //try to filter irrelevant subsections
                        {
                            var subsections = section.First().ToList();
                            foreach (var s in subsections)
                            {
                                list.Add(s.ToString());
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        throw new ExecutionError($"Filter error on text '{filterString}' exception {ex.Message}");
                    }

                }
                if (list.Any())
                {
                    values.Add(list);
                    indexes.Add(new List<string> { jobTitle });
                }
            }
            var report = await _graph.UpdateKGFromAssociationData(_config["userId"], new List<KGTrainingValue> { 
            new KGTrainingValue{ index = true, valueLineages = new List<string>{ jobLineage }, valueProperty = new List<string>{sectorLineage, areaLineage, typeLineage, "name" }, values= indexes },
            new KGTrainingValue{ index = false, valueLineages = new List<string>{ courseLineage }, valueProperty = new List<string>{"name" }, values= values }
            },requireLineage, "requires");

        }

        [TestMethod]
        [Ignore]
        public async Task TestGettingProperties()
        {
            var res = await _graph.GetGraphObjectProperty(_config["userId"], "5aba5cea-7c7e-4b1e-8abc-057c445cdaee","lineage");
        }

        [TestMethod]
        [Ignore]
        public void TestDataFieldNameDetection()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.datafieldnames.json"));
            var doc = docsource.ReadToEnd();
            var records = JsonConvert.DeserializeObject<List<string>>(doc);
            
            var filtergraph = new MatchList();
            var dict = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("use", "skills qualifications responsibilities knowledge experience job requirements role") };
            filtergraph.CreateTree(dict);
            var trainingData = new List<Classification>();
            foreach(var r in records)
            {
                var res = filtergraph.Find(r);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task TestGetConnectionByIDs()
        {
            var res = await _graph.GetConnectionByIds(_config["userId"], "3dc5afec-457a-4d79-bb81-abfd764f1c2e", "4bf1c3c4-a919-4c8d-a662-597b423743cf", teachLineage);
            Assert.AreEqual(teachLineage, res.lineage);
            var weight = await _graph.GetGraphConnectionProperty(_config["userId"], "3dc5afec-457a-4d79-bb81-abfd764f1c2e", "4bf1c3c4-a919-4c8d-a662-597b423743cf", teachLineage, "weight");
            await _graph.SetGraphConnectionProperty(_config["userId"], "3dc5afec-457a-4d79-bb81-abfd764f1c2e", "4bf1c3c4-a919-4c8d-a662-597b423743cf", teachLineage, "weight", "2.0");
            weight = await _graph.GetGraphConnectionProperty(_config["userId"], "3dc5afec-457a-4d79-bb81-abfd764f1c2e", "4bf1c3c4-a919-4c8d-a662-597b423743cf", teachLineage, "weight");
            res = await _graph.GetConnectionByIds(_config["userId"], "3dc5afec-457a-4d79-bb81-abfd764f1c2e", "7a00d122-50ba-471b-8fbc-8a033f5f267c", teachLineage);
           Assert.IsNull(res);
        }

    }
    class Classification
    {
        public string text { get; set; }

        public string category { get; set; }
    }
}
