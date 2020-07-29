using Darl.GraphQL.Models.Connectivity;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using QuickGraph;
using QuickGraph.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class SmallKGNetworkTest
    {

        private IGraphProcessing _graph;
        private IConfiguration _config;
        private IGraphPrimitives _primitives;
        private ILocalStore _graphStore;

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
        private static string descriptionLineage = "noun:01,4,05,21,05";
        private static string functionLineage = "noun:01,0,2,00,23,16,21,1";
        private static string careerLineage = "noun:01,0,2,00,00,15,20,01,1";
        private static string huntingLineage = "noun:01,0,2,00,23,35";
        private static string personalityLineage = "noun:01,1,09";
        private static string liveLineage = "adjective:7763";
        private static string studentLineage = "noun:00,2,00,175,0";

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
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("7ecb39be-fb44-4c13-92df-68ec152a4edb");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("2495b08b-93c3-4498-85b7-f4bdd36b6f01");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            //configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("33db770b-29e9-46ae-8a19-c1947bd775d8");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("a26560b3-7778-410b-a54b-b65da6a9649a");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("azure");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");

            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobConnectivity>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            _primitives = new BlobGraphPrimitives(blob, cache.Object);
            _graph = new GraphProcessing(_primitives);
            _graphStore = new GraphLocalStore(_config,logger.Object, context.Object, _graph);
        }

        [TestMethod]
        public async Task TestInference()
        {
            var userId = _config["userId"];
            var res = await _graphStore.ReadAsync(new List<string> { "path", "kg1_graph", "STUD1", "BJT26" });
            Assert.AreNotEqual("There is no path found", res.Value);
        }

        [TestMethod]
        public async Task TestAttributes()
        {
            var userId = _config["userId"];
            var res = await _graphStore.ReadAsync(new List<string> { "attribute", "kg1_graph", "BJT267", "noun:01,4,05,21,05" });
            Assert.AreEqual("The mixing engineer is responsible for combining all of the different sonic elements of a recorded piece of music into a final version and balancing the distinct parts to achieve a desired effect.", res.Value);
        }
        [TestMethod]
        public async Task TestCategories()
        {
            var userId = _config["userId"];
            var res = await _graphStore.ReadAsync(new List<string> { "categories", "kg1_graph", "I1", "noun:01,0,0,15,07,02,04,1,02,1", "name" });
            Assert.AreEqual(10, res.categories.Count);
        }        
        
        [TestMethod]
        public async Task TestInferenceNopath()
        {
            var userId = _config["userId"];
            var res = await _graphStore.ReadAsync(new List<string> { "path", "kg1_graph","STUD1", "BJT408" });
            Assert.AreEqual("There is no path found", res.Value);
        }

        /// <summary>
        /// Adds a student starting point for graph inference
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddStudent()
        {
            var v = new GraphObjectInput { lineage = studentLineage, name = "Student", externalId = "STUD1" };
            var userId = _config["userId"];
            var res = await _graph.CreateGraphObject($"{userId}_kg1_graph", v, OntologyAction.build);
//            var studentId = "359f45d1-6c76-4761-bd9c-0ac8e23a6150";
            //connect it up to all the courses via a "can take" connection
            var courses = await _graph.GetGraphObjectsByLineage($"{userId}_kg1_graph", courseLineage);
            foreach(var c in courses)
            {
                if(c. externalId != null && c.externalId.StartsWith("C"))
                { 
                    var e = new GraphConnectionInput { lineage = requireLineage, name = "can Take", weight = 1.0, startId = res.id, endId = c.id };
                    await _graph.CreateGraphConnection($"{userId}_kg1_graph", e, OntologyAction.build);
                }
            }
            await _graph.Store($"{userId}_kg1_graph");

        }

        [TestMethod]
        public async Task TestBuildSmallKG()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.Ians_jobs.graphml"));
            var ivf = new IdentifiableVertexFactory<SimpleVertex>(MakeVertex);
            var ief = new IdentifiableEdgeFactory<SimpleVertex, SimpleEdge>(MakeEdge);
            var graph = new TempGraph();
            graph.DeserializeFromGraphML<SimpleVertex, SimpleEdge, TempGraph>(docsource, ivf,ief);
            var jobroles = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.JobRoles.json"));
            var jr = jobroles.ReadToEnd();
            var roles = JArray.Parse(jr);
            foreach (var jobrole in roles)
            {
                var id = jobrole.SelectToken("$.ImportCode").ToString();
                var job = graph.vertices[id];
                Assert.IsNotNull(job);
                AddProperty(job, jobrole, "Description", descriptionLineage);
                AddProperty(job, jobrole, "WhatDoesItDo", functionLineage);
                AddProperty(job, jobrole, "CareerPath", careerLineage);
                AddProperty(job, jobrole, "FindingWork", huntingLineage);
                AddProperty(job, jobrole, "InterpersonalSkills", personalityLineage);
                AddProperty(job, jobrole, "WorkLive", liveLineage);
                //now edges
                foreach( var jrs in jobrole.SelectToken("$.JobRoleSkills").ToList())
                {
                    var js = jrs.SelectToken("$.JobSkill");
                    var bsid = js.SelectToken("$.ImportCode").ToString();
                    var skill = graph.vertices[bsid];
                    Assert.IsNotNull(skill);
                    //add link between job and skill
//                    graph.AddEdge(new SimpleEdge { Source = job, Target = skill, edgelabel = "requires", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    graph.AddEdge(new SimpleEdge { Source = skill, Target = job, edgelabel = "Required for", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    //now look for further links
                    foreach (var jslo in js.SelectToken("$.JobSkillLearningOutcomes").ToList())
                    {
                        var lo = jslo.SelectToken("$.LearningOutcome");
                        var loid = js.SelectToken("$.ImportCode").ToString();
                        var loObj = graph.vertices[loid];
                        Assert.IsNotNull(loObj);
//                        graph.AddEdge(new SimpleEdge { Source = skill, Target = loObj, edgelabel = "requires", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                        graph.AddEdge(new SimpleEdge { Source = loObj, Target = skill, edgelabel = "Required for", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    }
                    foreach (var jsst in js.SelectToken("$.JobSkillSubjectTopics").ToList())
                    {
                        var st = jsst.SelectToken("$.SubjectTopic");
                        var stid = js.SelectToken("$.ImportCode").ToString();
                        var stObj = graph.vertices[stid];
                        Assert.IsNotNull(stObj);
 //                       graph.AddEdge(new SimpleEdge { Source = skill, Target = stObj, edgelabel = "requires", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                        graph.AddEdge(new SimpleEdge { Source = stObj, Target = skill, edgelabel = "Required for", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    }
                    foreach (var jsts in js.SelectToken("$.JobSkillTransferableSkills").ToList())
                    {
                        var ts = jsts.SelectToken("$.SubjectTopic");
                        var tsid = js.SelectToken("$.ImportCode").ToString();
                        var tsObj = graph.vertices[tsid];
                        Assert.IsNotNull(tsObj);
 //                       graph.AddEdge(new SimpleEdge { Source = skill, Target = tsObj, edgelabel = "requires", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                        graph.AddEdge(new SimpleEdge { Source = tsObj, Target = skill, edgelabel = "Required for", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    }
                    foreach (var jsu in js.SelectToken("$.JobSkillUnits").ToList())
                    {
                        var un = jsu.SelectToken("$.Unit");
                        var uid = js.SelectToken("$.ImportCode").ToString();
                        var uObj = graph.vertices[uid];
                        Assert.IsNotNull(uObj);
//                        graph.AddEdge(new SimpleEdge { Source = skill, Target = uObj, edgelabel = "requires", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                        graph.AddEdge(new SimpleEdge { Source = uObj, Target = skill, edgelabel = "Required for", id = Guid.NewGuid().ToString(), lineage = requireLineage });
                    }
                }
            }
            //now add sector information
            var JobSource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.WM_MusicIndustry.json"));
            var jobdoc = JobSource.ReadToEnd();
            var industries = JArray.Parse(jobdoc);
            var industryVertex = new SimpleVertex { label = "Music industry", id = "I1", lineage = industryLineage };
            graph.AddVertex(industryVertex);
            var sectors = industries.SelectTokens("$..Sectors").First().ToList();
            int sectorIndex = 0;
            int areaIndex = 0;
            int typeIndex = 0;
            foreach (var sector in sectors)
            {
                var sectorName = sector.SelectToken("$.Name").ToString();
                var sectorKey = $"S{sectorIndex}";
                var areas = sector.SelectTokens("$.Areas").First().ToList();
                foreach (var area in areas)
                {
                    var areaName = area.SelectToken("$.Name").ToString();
                    var areaKey = $"A{areaIndex}";
                    var types = area.SelectTokens("$.Types").First().ToList();
                    foreach (var t in types)
                    {
                        var typeName = t.SelectToken("$.Name").ToString();
                        var typeKey = $"T{areaIndex}";
                        var jobs = t.SelectTokens("$.Roles").First().ToList();
                        foreach (var r in jobs)
                        {
                            if (r.HasValues)
                            {
                                var jobName = r.SelectToken("$.Name").ToString();
                                var match = graph.vertices.Values.Where(a => a.label == jobName).FirstOrDefault();
                                if(match != null)
                                {
                                    SimpleVertex sectorVertex;
                                    if (!graph.vertices.ContainsKey(sectorKey))
                                    {
                                        sectorVertex = new SimpleVertex { label = sectorName, id = sectorKey, lineage = sectorLineage };
                                        graph.AddVertex(sectorVertex);
                                        graph.AddEdge(new SimpleEdge { edgelabel = "consists of", lineage = consistsLineage, weight = 1.0, Source = industryVertex, Target = sectorVertex ,id = Guid.NewGuid().ToString() });
                                    }
                                    else
                                    {
                                        sectorVertex = graph.vertices[sectorKey];
                                    }
                                    SimpleVertex areaVertex;
                                    if (!graph.vertices.ContainsKey(areaKey))
                                    {
                                        areaVertex = new SimpleVertex { label = areaName, id = areaKey, lineage = areaLineage };
                                        graph.AddVertex(areaVertex);
                                        graph.AddEdge(new SimpleEdge { edgelabel = "consists of", lineage = consistsLineage, weight = 1.0, Source = sectorVertex, Target = areaVertex, id = Guid.NewGuid().ToString() });
                                    }
                                    else
                                    {
                                        areaVertex = graph.vertices[areaKey];
                                    }
                                    SimpleVertex typeVertex;
                                    if (!graph.vertices.ContainsKey(typeKey))
                                    {
                                        typeVertex = new SimpleVertex { label = typeName, id = typeKey, lineage = typeLineage };
                                        graph.AddVertex(typeVertex);
                                        graph.AddEdge(new SimpleEdge { edgelabel = "consists of", lineage = consistsLineage, weight = 1.0, Source = areaVertex, Target = typeVertex, id = Guid.NewGuid().ToString() });
                                    }
                                    else
                                    {
                                        typeVertex = graph.vertices[typeKey];
                                    }
                                    graph.AddEdge(new SimpleEdge { edgelabel = "consists of", lineage = consistsLineage, weight = 1.0, Source = typeVertex, Target = match, id = Guid.NewGuid().ToString() });

                                }
                            }
                        }
                        typeIndex++;
                    }
                    areaIndex++;
                }
                sectorIndex++;
            }
            
            //now load the entire thing into the cloud
            var client = new GraphQLHttpClient(_config["darlDevUrl"], new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _config["darlDevAPiKey"]);
            var userId = _config["userId"];

            var nameIdLookup = new Dictionary<string, string>();
            foreach (var lv in graph.vertices.Values)
            {
                var v = new GraphObjectInput { lineage = lv.lineage, name = lv.label ?? lv.id,  externalId = lv.id, properties = lv.properties };
                /*                var req = new GraphQLHttpRequest() { Variables = new { go = v }, Query = @"mutation cgo($go: graphObjectInput!){createGraphObject(graphObject: $go, ontology: BUILD){name id lineage inferred virtual}}", OperationName = "cgo" };
                                var resp = await client.SendQueryAsync<dynamic>(req);
                                nameIdLookup.Add(lv.id, ((Newtonsoft.Json.Linq.JObject)resp.Data).SelectToken("createGraphObject.id").ToString());
                                Thread.Sleep(100);*/
                try
                {
                    var res = await _graph.CreateGraphObject($"{userId}_kg1_graph", v, OntologyAction.build);
                    nameIdLookup.Add(lv.id, res.id);
                }
                catch(Exception ex)
                {

                }
            }
            foreach (var le in graph.edges.Values)
            {
                var e = new GraphConnectionInput { lineage = le.lineage, name = le.edgelabel, weight = le.weight, startId = nameIdLookup[le.Source.id], endId = nameIdLookup[le.Target.id] };
                /*                var req = new GraphQLHttpRequest() { Variables = new { gc = e }, Query = @"mutation cgc($gc: graphConnectionInput!){createGraphConnection(graphConnection: $gc, ontology: BUILD){name id}}" };
                                var resp = await client.SendQueryAsync<GraphConnection>(req);
                                Thread.Sleep(100);*/
                try
                {
                    await _graph.CreateGraphConnection($"{userId}_kg1_graph", e, OntologyAction.build);
                }
                catch (Exception ex)
                {

                }
            }
            await _graph.Store($"{userId}_kg1_graph");
        }

        SimpleVertex MakeVertex(String id)
        {
            //create lineage from id type
            var lineage = "";
            if(id.StartsWith("BJT"))
            {//it's a job
                lineage = jobLineage;
            }
            else if(id.StartsWith("LO"))
            {//it's a learning outcome
                lineage = learningOutcomeLineage;
            }
            else if(id.StartsWith("BS"))
            {//it's a basic skill
                lineage = skillLineage;
            }
            else if (id.StartsWith("U"))
            {//it's a Unit
                lineage = courseLineage;
            }
            else if (id.StartsWith("TS"))
            {//it's a transferrable skill
                lineage = skillLineage;
            }
            else if (id.StartsWith("C"))
            {//it's a course
                lineage = courseLineage;
            }
            else if (id.StartsWith("ST"))
            {//it's a subject topic
                lineage = topicLineage;
            }
            return new SimpleVertex { id = id, lineage = lineage };
        }

        SimpleEdge MakeEdge(SimpleVertex source, SimpleVertex target, String id)
        {
            //define lineage and label based on source and target
            var lineage = "";
            var name = "";
            if(source.id.StartsWith("BJT"))
            {//it's a job
                if(target.id.StartsWith("BJT"))
                {
                    name = "can lead to";
                }
            }
            else if (source.id.StartsWith("LO"))
            {//it's a learning outcome
                if (target.id.StartsWith("BS"))
                {
                    name = "Required for";
                }
            }
            else if (source.id.StartsWith("BS"))
            {//it's a basic skill
                if (target.id.StartsWith("BJT"))
                {
                    name = "Required for";
                }
            }
            else if (source.id.StartsWith("U"))
            {//it's a Unit
                if (target.id.StartsWith("BS"))
                {
                    name = "teaches";
                }
            }
            else if (source.id.StartsWith("TS"))
            {//it's a transferrable skill
                lineage = enableLineage;
                name = "teaches";
            }
            else if (source.id.StartsWith("C"))
            {//it's a course
                name = "teaches";
            }
            else if (source.id.StartsWith("ST"))
            {//it's a subject topic
                name = "teaches";
            }
            lineage = enableLineage;
            return new SimpleEdge { id = id, Source = source, Target = target, lineage = lineage, edgelabel = name ?? "Required for", weight = 1.0 };
        }

        void AddProperty(SimpleVertex v, JToken j, string name, string lineage)
        {
            var path = $"$.{name}";
            if (j.SelectToken(path) != null && !string.IsNullOrEmpty(j.SelectToken(path).ToString()))
            {
                v.properties.Add(new GraphAttribute { lineage = lineage, value = j.SelectToken(path).ToString() });
            }
        }
    }

    public class TempGraph : QuickGraph.IMutableVertexAndEdgeListGraph<SimpleVertex, SimpleEdge>
    {
        public Dictionary<string, SimpleVertex> vertices { get; set; } = new Dictionary<string, SimpleVertex>();
        public Dictionary<string, SimpleEdge> edges { get; set; } = new Dictionary<string, SimpleEdge>();

        public bool IsEdgesEmpty => throw new NotImplementedException();

        public int EdgeCount => throw new NotImplementedException();

        public IEnumerable<SimpleEdge> Edges => throw new NotImplementedException();

        public bool IsDirected => throw new NotImplementedException();

        public bool AllowParallelEdges => throw new NotImplementedException();

        public bool IsVerticesEmpty => throw new NotImplementedException();

        public int VertexCount => throw new NotImplementedException();

        public IEnumerable<SimpleVertex> Vertices => throw new NotImplementedException();

        public event VertexAction<SimpleVertex> VertexAdded;
        public event VertexAction<SimpleVertex> VertexRemoved;
        public event EdgeAction<SimpleVertex, SimpleEdge> EdgeAdded;
        public event EdgeAction<SimpleVertex, SimpleEdge> EdgeRemoved;

        public bool AddEdge(SimpleEdge edge)
        {
            edges.Add(edge.id, edge);
            return true;
        }

        public int AddEdgeRange(IEnumerable<SimpleEdge> edges)
        {
            throw new NotImplementedException();
        }

        public bool AddVertex(SimpleVertex v)
        {
            vertices.Add(v.id, v);
            return true;
        }

        public int AddVertexRange(IEnumerable<SimpleVertex> vertices)
        {
            throw new NotImplementedException();
        }

        public bool AddVerticesAndEdge(SimpleEdge edge)
        {
            throw new NotImplementedException();
        }

        public int AddVerticesAndEdgeRange(IEnumerable<SimpleEdge> edges)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void ClearOutEdges(SimpleVertex v)
        {
            throw new NotImplementedException();
        }

        public bool ContainsEdge(SimpleEdge edge)
        {
            throw new NotImplementedException();
        }

        public bool ContainsEdge(SimpleVertex source, SimpleVertex target)
        {
            throw new NotImplementedException();
        }

        public bool ContainsVertex(SimpleVertex vertex)
        {
            throw new NotImplementedException();
        }

        public bool IsOutEdgesEmpty(SimpleVertex v)
        {
            throw new NotImplementedException();
        }

        public int OutDegree(SimpleVertex v)
        {
            throw new NotImplementedException();
        }

        public SimpleEdge OutEdge(SimpleVertex v, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SimpleEdge> OutEdges(SimpleVertex v)
        {
            throw new NotImplementedException();
        }

        public bool RemoveEdge(SimpleEdge edge)
        {
            throw new NotImplementedException();
        }

        public int RemoveEdgeIf(EdgePredicate<SimpleVertex, SimpleEdge> predicate)
        {
            throw new NotImplementedException();
        }

        public int RemoveOutEdgeIf(SimpleVertex v, EdgePredicate<SimpleVertex, SimpleEdge> predicate)
        {
            throw new NotImplementedException();
        }

        public bool RemoveVertex(SimpleVertex v)
        {
            throw new NotImplementedException();
        }

        public int RemoveVertexIf(VertexPredicate<SimpleVertex> pred)
        {
            throw new NotImplementedException();
        }

        public void TrimEdgeExcess()
        {
            throw new NotImplementedException();
        }

        public bool TryGetEdge(SimpleVertex source, SimpleVertex target, out SimpleEdge edge)
        {
            throw new NotImplementedException();
        }

        public bool TryGetEdges(SimpleVertex source, SimpleVertex target, out IEnumerable<SimpleEdge> edges)
        {
            throw new NotImplementedException();
        }

        public bool TryGetOutEdges(SimpleVertex v, out IEnumerable<SimpleEdge> edges)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class SimpleVertex
    {
        [XmlAttribute]
        public string id { get; set; }
        [XmlAttribute]
        public string label { get; set; }

        [XmlAttribute]
        public int r { get; set; }
        [XmlAttribute]
        public int g { get; set; }
        [XmlAttribute]
        public int  b { get; set; }
        [XmlAttribute]
        public float x { get; set; }
        [XmlAttribute]
        public float y { get; set; }
        [XmlAttribute]
        public float size { get; set; }

        public string lineage { get; set; }

        public List<GraphAttribute> properties { get; set; } = new List<GraphAttribute>();

    }

    public class SimpleEdge : IEdge<SimpleVertex>
    {
        [XmlAttribute]
        public string edgelabel { get; set; }
        [XmlAttribute]
        public double weight { get; set; }
        [XmlAttribute]
        public string id { get; set; }

        public SimpleVertex Source { get; set; }

        public SimpleVertex Target { get; set; }

        public string lineage { get; set; }
    }
}
